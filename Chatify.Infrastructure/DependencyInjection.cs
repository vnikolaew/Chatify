using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using AutoMapper.Internal;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Authentication;
using Chatify.Infrastructure.Authentication.External.Facebook;
using Chatify.Infrastructure.Authentication.External.Google;
using Chatify.Infrastructure.Common;
using Chatify.Infrastructure.Data;
using Chatify.Infrastructure.Data.Repositories;
using Chatify.Infrastructure.Data.Seeding;
using Chatify.Infrastructure.FileStorage;
using Chatify.Infrastructure.Mailing;
using Chatify.Infrastructure.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Shared.Abstractions.Serialization;
using Chatify.Shared.Abstractions.Time;
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
using StackExchange.Redis;
using AuthenticationService = Chatify.Infrastructure.Authentication.AuthenticationService;
using IAuthenticationService = Chatify.Application.Authentication.Contracts.IAuthenticationService;

namespace Chatify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddData(configuration)
            .AddSeeding(configuration)
            .AddRepositories()
            .AddServices()
            .AddCaching(configuration)
            .AddContexts();

    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddAuthenticationServices()
            .AddEvents(new[] { Assembly.GetExecutingAssembly() })
            .AddTransient<IEmailSender, NullEmailSender>()
            .AddSingleton<IClock, UtcClock>()
            .AddTransient<IFileUploadService, LocalFileSystemUploadService>()
            .AddTransient<IGuidGenerator, TimeUuidGenerator>()
            .AddTransient<IPasswordHasher, PasswordHasher>()
            .AddTransient<ISerializer, SystemTextJsonSerializer>()
            .AddNotifications()
            .AddCounters();

    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services
            .AddSignalR(opts => { opts.KeepAliveInterval = 30.Seconds(); })
            .AddHubOptions<ChatifyHub>(opts =>
            {
                opts.EnableDetailedErrors = false;
                opts.MaximumParallelInvocationsPerClient = 10;
            }).AddJsonProtocol();

        return services.AddScoped<INotificationService, SignalRNotificationService>();
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
        var connectionMultiplexer = ConnectionMultiplexer
            .Connect(endpoint);

        return services
            .AddSingleton<IConnectionMultiplexer>(connectionMultiplexer)
            .AddSingleton<IDatabase>(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
    }

    public static IServiceCollection AddCounters(this IServiceCollection services)
    {
        var counterTypes = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericType: false } && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICounterService<,>)))
            .ToList();

        foreach (var counterType in counterTypes)
        {
            services.AddScoped(counterType.GetInterfaces().First(), counterType);
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

        foreach (var repositoryType in repositoryTypes)
        {
            // Register type using all implemented interfaces:
            foreach (var implementedInterface in repositoryType.ImplementedInterfaces)
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

        foreach (var repositoryType in cassandraRepositoryTypes)
        {
            var baseType = repositoryType
                .GetTypeInheritance()
                .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BaseCassandraRepository<,,>));

            services.AddScoped(baseType, repositoryType);
        }

        return services
            .AddScoped<IChatMessageReplyRepository, ChatMessageReplyRepository>()
            .AddScoped<IChatGroupMemberRepository, ChatGroupMembersRepository>()
            .AddScoped<IFriendInvitationRepository, FriendInvitationRepository>()
            .AddScoped<IFriendshipsRepository, FriendshipsRepository>();
    }

    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services)
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

        return services
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IEmailConfirmationService, EmailConfirmationService>();
    }

    private static readonly IAsyncPolicy<HttpResponseMessage> RetryPolicy
        = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}