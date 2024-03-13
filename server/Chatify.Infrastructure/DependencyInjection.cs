using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using AutoMapper.Extensions.ExpressionMapping;
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
using Chatify.Infrastructure.Data.Conversions;
using Chatify.Infrastructure.Data.Repositories;
using Chatify.Infrastructure.Data.Services;
using Chatify.Infrastructure.FileStorage;
using Chatify.Infrastructure.Mailing;
using Chatify.Infrastructure.Messages;
using Chatify.Infrastructure.Messages.BackgroundJobs;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Services;
using Chatify.Infrastructure.Services.External;
using Chatify.Shared.Abstractions.Common;
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
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Quartz;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Modeling;
using Redis.OM.Searching;
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
            .AddData(configuration)
            .AddCassandraIdentity(configuration)
            .AddSeeding()
            .AddRepositories()
            .AddBackgroundJobs()
            .AddApplicationTelemetry()
            .AddGrpcClients(configuration)
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

    public static IServiceCollection AddMappers(this IServiceCollection services)
        => services.AddAutoMapper(config =>
        {
            config
                .AddExpressionMapping()
                .AddMaps(
                    typeof(IAssemblyMarker),
                    typeof(Application.IAssemblyMarker));

            config.AllowNullDestinationValues = true;
        });

    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<HostOptions>(opts =>
            {
                opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                opts.ServicesStartConcurrently = false;
            })
            .AddAuthenticationServices(configuration)
            .AddEvents(new[] { Assembly.GetExecutingAssembly() }, Extensions.EventDispatcherType.FireAndForget)
            .AddTransient<IEmailSender, NullEmailSender>()
            .AddSingleton<IClock, UtcClock>()
            .AddTransient<IFileUploadService, LocalFileSystemUploadService>()
            .AddTransient<IGuidGenerator, TimeUuidGenerator>()
            .AddTransient<IPasswordHasher, PasswordHasher>()
            .AddTransient<IPagingCursorHelper, CassandraPagingCursorHelper>()
            .AddTransient<ISerializer, SystemTextJsonSerializer>()
            .AddSingleton<IOpenGraphMetadataEnricher, OpenGraphMetadataEnricher>()
            .AddSingleton<IMessageContentNormalizer, MessageContentNormalizer>()
            .AddScoped<IChatGroupsFeedService, ChatGroupsFeedService>()
            .AddNotifications()
            .AddCounters();

    public static IServiceCollection AddApplicationTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(builder => { builder.AddService("Chatify"); })
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                    .AddSource("Chatify")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App1").AddTelemetrySdk())
                    .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:14268"); })
                    .AddRedisInstrumentation();
            })
            .WithMetrics(builder =>
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddCassandraInstrumentation());
        return services;
    }

    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services
            .AddScoped<INotificationService, SignalRNotificationService>()
            .AddSignalR(opts => { opts.KeepAliveInterval = 30.Seconds(); })
            .AddJsonProtocol()
            .AddHubOptions<ChatifyHub>(opts =>
            {
                opts.EnableDetailedErrors = false;
                opts.MaximumParallelInvocationsPerClient = 10;
            });

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

        var multiplexer = RedisRetryPolicy.Execute(
            () => ConnectionMultiplexer.Connect(endpoint, opts => { opts.AbortOnConnectFail = false; }));

        services
            .AddSingleton(sp =>
                new RedisConnectionProvider(sp.GetRequiredService<IConnectionMultiplexer>()))
            .AddSingleton<IRedisConnectionProvider>(sp =>
                sp.GetRequiredService<RedisConnectionProvider>());

        RedisSerializationSettings.JsonSerializerOptions
            .Converters.Add(new IPAddressConverter());
        RedisSerializationSettings.JsonSerializerOptions.IncludeFields = false;

        var cacheIndexTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        t.GetCustomAttribute<DocumentAttribute>() is not null)
            .ToList();
        foreach ( var cacheIndexType in cacheIndexTypes )
        {
            services.AddScoped(
                typeof(RedisCollection<>).MakeGenericType(cacheIndexType),
                sp =>
                {
                    var redisCollectionMethod = typeof(RedisConnectionProvider)
                        .GetMethods()
                        .FirstOrDefault(m => m.Name.Contains(nameof(RedisConnectionProvider.RedisCollection)));

                    var provider = sp.GetRequiredService<RedisConnectionProvider>();
                    return redisCollectionMethod!
                        .MakeGenericMethod(cacheIndexType)
                        .Invoke(provider, Array.Empty<object?>())!;
                });

            services.AddScoped(
                typeof(IRedisCollection<>),
                typeof(RedisCollection<>));
        }

        return services
            .AddSingleton<IConnectionMultiplexer>(multiplexer)
            .AddSingleton(multiplexer)
            .AddScoped<ICacheService, RedisCacheService>()
            .AddSingleton<IDatabase>(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase())
            .AddSingleton<IServer>(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetServer(endpoint));
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

        services.AddTransient<IEntityChangeTracker, EntityChangeTracker>();
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

    private static readonly ResiliencePipeline<IConnectionMultiplexer> RedisRetryPolicy
        = new ResiliencePipelineBuilder<IConnectionMultiplexer>()
            .AddRetry(new RetryStrategyOptions<IConnectionMultiplexer>
            {
                ShouldHandle = args =>
                    ValueTask.FromResult(args.Outcome.Exception is RedisConnectionException),
                OnRetry = arguments =>
                {
                    Console.WriteLine($"Retrying Redis connection. Attempt: {arguments.AttemptNumber}");
                    return ValueTask.CompletedTask;
                },
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(8),
                MaxRetryAttempts = 5,
                DelayGenerator = arguments =>
                    ValueTask.FromResult(( TimeSpan? )TimeSpan.FromSeconds(Math.Pow(2, arguments.AttemptNumber)))
            }).Build();

    public static TSettings AddSettings<TSettings>(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        string? sectionName = default)
        where TSettings : class, new()
    {
        var settings = new TSettings();
        configuration.Bind(sectionName ?? typeof(TSettings).Name, settings);

        // serviceCollection.Configure<TSettings>(configuration);
        serviceCollection.AddSingleton(settings);
        return settings;
    }
}