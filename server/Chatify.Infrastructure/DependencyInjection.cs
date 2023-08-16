using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using AutoMapper.Internal;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Common;
using Chatify.Infrastructure.Authentication;
using Chatify.Infrastructure.Authentication.External.Facebook;
using Chatify.Infrastructure.Authentication.External.Github;
using Chatify.Infrastructure.Authentication.External.Google;
using Chatify.Infrastructure.ChatGroups.Services;
using Chatify.Infrastructure.Common;
using Chatify.Infrastructure.Common.Caching;
using Chatify.Infrastructure.Common.Security;
using Chatify.Infrastructure.Data;
using Chatify.Infrastructure.Data.Repositories;
using Chatify.Infrastructure.FileStorage;
using Chatify.Infrastructure.Mailing;
using Chatify.Infrastructure.Messages;
using Chatify.Infrastructure.Messages.BackgroundJobs;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Shared.Abstractions.Serialization;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Api;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Shared.Infrastructure.Events;
using Chatify.Shared.Infrastructure.Serialization;
using Chatify.Shared.Infrastructure.Time;
using LanguageExt.UnitsOfMeasure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Quartz;
using StackExchange.Redis;
using AuthenticationService = Chatify.Infrastructure.Authentication.AuthenticationService;
using Extensions = Chatify.Shared.Infrastructure.Events.Extensions;
using IAuthenticationService = Chatify.Application.Authentication.Contracts.IAuthenticationService;

namespace Chatify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddAuthorization()
            .AddData(configuration)
            .AddCassandraIdentity()
            .AddSeeding(configuration)
            .AddRepositories()
            .AddBackgroundJobs()
            .AddServices(configuration)
            .AddCaching(configuration)
            .AddContexts();

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
        => services
            .AddQuartz(opts =>
            {
                opts.UseMicrosoftDependencyInjectionJobFactory();
                opts.AddJob<ProcessChatMessageJob>(config =>
                    config.WithIdentity(new JobKey(nameof(ProcessChatMessageJob))).StoreDurably());
                opts.AddJob<ProcessChatMessageReplyJob>(config =>
                    config.WithIdentity(new JobKey(nameof(ProcessChatMessageReplyJob))).StoreDurably());
            })
            .AddQuartzHostedService(opts =>
                opts.WaitForJobsToComplete = true);

    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddAuthenticationServices(configuration)
            .AddEvents(new[] { Assembly.GetExecutingAssembly() }, Extensions.EventDispatcherType.FireAndForget)
            .AddTransient<IEmailSender, NullEmailSender>()
            .AddSingleton<IClock, UtcClock>()
            .AddTransient<IFileUploadService, LocalFileSystemUploadService>()
            .AddTransient<IGuidGenerator, SnowflakeUuidGenerator>()
            .AddTransient<IPasswordHasher, PasswordHasher>()
            .AddTransient<IPagingCursorHelper, CassandraPagingCursorHelper>()
            .AddTransient<ISerializer, SystemTextJsonSerializer>()
            .AddScoped<IChatGroupsFeedService, ChatGroupsFeedService>()
            .AddNotifications()
            .AddCounters();

    public static IServiceCollection AddNotifications(
        this IServiceCollection services)
    {
        services
            .AddScoped<INotificationService, SignalRNotificationService>()
            .AddSignalR(opts => { opts.KeepAliveInterval = 30.Seconds(); })
            .AddHubOptions<ChatifyHub>(opts =>
            {
                opts.EnableDetailedErrors = false;
                opts.MaximumParallelInvocationsPerClient = 10;
            }).AddJsonProtocol();

        return services;
    }

    public static IEndpointRouteBuilder MapNotifications(
        this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapHub<ChatifyHub>(ChatifyHub.Endpoint, opts =>
            {
                opts.Transports = HttpTransportType.LongPolling
                                  | HttpTransportType.WebSockets
                                  | HttpTransportType.ServerSentEvents;
            });

        return routeBuilder;
    }

    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var endpoint = configuration.GetValue<string>("Redis:Endpoint")!;
        var connectionMultiplexer = ConnectionMultiplexer.Connect(endpoint);

        return services
            .AddSingleton<IConnectionMultiplexer>(connectionMultiplexer)
            .AddScoped<ICacheService, RedisCacheService>()
            .AddSingleton<IDatabase>(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
    }

    public static IServiceCollection AddCounters(this IServiceCollection services)
    {
        var counterTypes = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericType: false } && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICounterService<,>)))
            .Select(t => ( t,
                t.ImplementedInterfaces.First(i => i.GetGenericTypeDefinition() == typeof(ICounterService<,>)) ))
            .ToList();

        foreach ( var (service, @interface) in counterTypes )
        {
            services.AddScoped(@interface, service);
        }

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var repositoryTypes = Assembly
            .GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainRepository<,>)))
            .ToList();

        foreach ( var repositoryType in repositoryTypes )
        {
            // Register type using all implemented interfaces:
            foreach ( var implementedInterface in repositoryType.ImplementedInterfaces )
            {
                services.AddScoped(implementedInterface, repositoryType);
            }
        }

        var cassandraRepositoryTypes = Assembly
            .GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.GetTypeInheritance().Any(t =>
                t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BaseCassandraRepository<,,>)))
            .ToList();

        foreach ( var repositoryType in cassandraRepositoryTypes )
        {
            var baseType = repositoryType
                .GetTypeInheritance()
                .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BaseCassandraRepository<,,>));

            var interfaces = repositoryType.ImplementedInterfaces;

            services.AddScoped(baseType, repositoryType);
            foreach ( var @interface in interfaces )
            {
                services.AddScoped(@interface, repositoryType);
            }
        }

        return services;

        // return services
        //     .AddScoped<IChatMessageReplyRepository, ChatMessageReplyRepository>()
        //     .AddScoped<IChatGroupAttachmentRepository, ChatGroupAttachmentRepository>()
        //     .AddScoped<IChatGroupMemberRepository, ChatGroupMembersRepository>()
        //     .AddScoped<IFriendInvitationRepository, FriendInvitationRepository>()
        //     .AddScoped<IFriendshipsRepository, FriendshipsRepository>();
    }

    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IGoogleOAuthClient, GoogleOAuthClient>(client =>
        {
            client.BaseAddress = new Uri(GoogleOAuthClient.Endpoint);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }).AddPolicyHandler(RetryPolicy);

        services.AddHttpClient<IFacebookOAuthClient, FacebookOAuthClient>(client =>
        {
            client.BaseAddress = new Uri(FacebookOAuthClient.Endpoint);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }).AddPolicyHandler(RetryPolicy);

        var options = new GithubOptions();
        configuration.GetSection(nameof(GithubOptions)).Bind(options);

        return services
            .AddSingleton(options)
            .AddScoped<IGithubOAuthClient, GithubOAuthClient>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IEmailConfirmationService, EmailConfirmationService>();
    }

    private static readonly IAsyncPolicy<HttpResponseMessage> RetryPolicy
        = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}