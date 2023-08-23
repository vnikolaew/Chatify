using System.Net.Sockets;
using System.Reflection;
using AspNetCore.Identity.Cassandra;
using AspNetCore.Identity.Cassandra.Extensions;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Serialization;
using Chatify.Infrastructure.Data.Mappings.Serialization;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Seeding;
using Chatify.Infrastructure.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using ISession = Cassandra.ISession;

namespace Chatify.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddSeeding(
        this IServiceCollection services)
        => services
            .AddHostedService<DatabaseSeedingService>()
            .Scan(s => s.FromAssemblyOf<ISeeder>()
                .AddClasses(c => c.AssignableTo<ISeeder>()
                    .Where(t => t is { IsAbstract: false, IsInterface: false }), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime())
            .AddScoped<CompositeSeeder>();

    public static IServiceCollection AddCassandra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Figure out auto-registration of serializers:
        // var typeSerializers = Assembly
        //     .GetExecutingAssembly()
        //     .GetTypes()
        //     .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(TypeSerializer)))

        return services
            .Configure<CassandraOptions>(configuration.GetSection("Cassandra"))
            .AddSingleton(x => x.GetRequiredService<IOptions<CassandraOptions>>().Value)
            .AddTransient<IMapper>(sp =>
                new Mapper(sp.GetRequiredService<ISession>()))
            .AddSingleton<ISession>(
                sp =>
                {
                    var requiredService = sp.GetRequiredService<CassandraOptions>();
                    var logger = sp.GetRequiredService<ILogger<CassandraOptions>>();
                    var options = new QueryOptions();

                    if ( requiredService.Query is { ConsistencyLevel: not null } &&
                         Enum.TryParse(requiredService.Query.ConsistencyLevel.ToString(), true,
                             out ConsistencyLevel result) )
                        options.SetConsistencyLevel(result);

                    var cluster = Cluster.Builder()
                        .AddContactPoints(requiredService.ContactPoints)
                        .WithPort(requiredService.Port)
                        .WithCredentials(
                            requiredService.Credentials.UserName,
                            requiredService.Credentials.Password)
                        .WithTypeSerializers(
                            new TypeSerializerDefinitions()
                                .Define(new UserStatusTypeSerializer())
                                .Define(new UserNotificationTypeSerializer()))
                        .WithQueryOptions(options).Build();

                    ISession session = null!;
                    RetryPolicy(requiredService.RetryCount, logger)
                        .Execute(() => session = cluster.Connect());
                    if ( session is null )
                        throw new ApplicationException("FATAL ERROR: Cassandra session could not be created");

                    logger.LogInformation("Cassandra session created");
                    return session;
                });
    }

    private static RetryPolicy RetryPolicy(
        int retryCount,
        ILogger<CassandraOptions> logger)
        => Policy
            .Handle<SocketException>()
            .Or<NoHostAvailableException>()
            .WaitAndRetry(retryCount,
                ( Func<int, TimeSpan> )( retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2.0, retryAttempt)) ),
                ( Action<Exception, TimeSpan, Context> )( (exception, retryCount, _) =>
                    logger.LogWarning("Retry {RetryCount} due to: {Exception}", retryCount, exception) ));

    public static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mappingsList = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        t.IsSubclassOf(typeof(Cassandra.Mapping.Mappings)))
            .Select(Activator.CreateInstance)
            .Cast<Cassandra.Mapping.Mappings>()
            .Where(m => m is not null)
            .ToList();

        foreach ( var mapping in mappingsList )
            MappingConfiguration.Global.Define(mapping);

        return services
            .AddCassandra(configuration)
            .AddTransient<Mapper>(sp => ( sp.GetRequiredService<IMapper>() as Mapper )!)
            .AddHostedService<RedisIndicesCreationService>()
            .AddHostedService<DatabaseInitializationService>();
    }

    public static IServiceCollection AddCassandraIdentity(
        this IServiceCollection services)
    {
        services
            .AddIdentity<ChatifyUser, CassandraIdentityRole>(ConfigureIdentityOptions)
            .AddCassandraErrorDescriber<CassandraErrorDescriber>()
            .UseCassandraStores<ISession>()
            .AddDefaultTokenProviders();

        services
            .ConfigureExternalCookie(opts =>
            {
                opts.Cookie.SameSite = SameSiteMode.None;
                opts.Events.OnRedirectToAccessDenied = ctx => { return Task.CompletedTask; };
                opts.Events.OnValidatePrincipal = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<IAssemblyMarker>>();
                    logger.LogInformation("Validating principal ...");
                    return Task.CompletedTask;
                };
                opts.Cookie.HttpOnly = false;
            })
            .ConfigureApplicationCookie(opts =>
            {
                opts.Cookie.SameSite = SameSiteMode.None;
                opts.Events.OnRedirectToAccessDenied = ctx => { return Task.CompletedTask; };
                opts.Events.OnValidatePrincipal = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<IAssemblyMarker>>();
                    logger.LogInformation("Validating principal ...");
                    return Task.CompletedTask;
                };
                opts.Cookie.HttpOnly = false;
            });

        return services;
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;

        options.User.RequireUniqueEmail = true;
    }
}