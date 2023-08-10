using System.Net.Sockets;
using System.Reflection;
using AspNetCore.Identity.Cassandra;
using AspNetCore.Identity.Cassandra.Extensions;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Serialization;
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
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if ( !configuration.GetValue<bool>("UseSeeding") ) return services;

        return services
            .AddHostedService<DatabaseSeedingService>()
            .Scan(s => s.FromExecutingAssembly()
                .AddClasses(c => c.AssignableTo<ISeeder>()
                    .Where(t => t is { IsAbstract: false, IsInterface: false }))
                .AsImplementedInterfaces()
                .WithScopedLifetime())
            .AddScoped<CompositeSeeder>();
    }

    public static IServiceCollection AddCassandra(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
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
                        .WithQueryOptions(options).Build();

                    ISession session = null!;
                    RetryPolicy(requiredService.RetryCount, logger)
                        .Execute(() => session = cluster.Connect());
                    if ( session is null )
                        throw new ApplicationException("FATAL ERROR: Cassandra session could not be created");

                    logger.LogInformation("Cassandra session created");
                    return session;
                });

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

        services
            .AddCassandra(configuration)
            .AddTransient<Mapper>(sp => ( sp.GetRequiredService<IMapper>() as Mapper )!)
            .AddHostedService<DatabaseInitializationService>()
            .AddIdentity<ChatifyUser, CassandraIdentityRole>(ConfigureIdentityOptions)
            .AddCassandraErrorDescriber<CassandraErrorDescriber>()
            .UseCassandraStores<ISession>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(opts =>
        {
            opts.Cookie.SameSite = SameSiteMode.None;
            opts.Cookie.HttpOnly = false;
        });

        return services;
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedEmail = false;

        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;

        options.User.RequireUniqueEmail = true;
    }
}