using System.Net.Sockets;
using System.Reflection;
using AspNetCore.Identity.Cassandra;
using AspNetCore.Identity.Cassandra.Extensions;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Serialization;
using Chatify.Infrastructure.Common.Security;
using Chatify.Infrastructure.Common.Settings;
using Chatify.Infrastructure.Data.Mappings.Serialization;
using Chatify.Infrastructure.Data.Seeding;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using ChatifyUser = Chatify.Infrastructure.Data.Models.ChatifyUser;
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
        var definitions = GetTypeSerializerDefinitions();

        CassandraOptions cassandraOptions = new CassandraOptions();
        configuration.GetSection("Cassandra").Bind(cassandraOptions, opts => { opts.BindNonPublicProperties = true; });
        services.Configure<CassandraOptions>(configuration.GetSection("Cassandra"));

        var options = new QueryOptions();

        if ( cassandraOptions.Query is { ConsistencyLevel: not null } &&
             Enum.TryParse(cassandraOptions.Query.ConsistencyLevel.ToString(), true,
                 out ConsistencyLevel result) )
            options.SetConsistencyLevel(result);

        var cluster = Cluster.Builder()
            .AddContactPoints(cassandraOptions.ContactPoints)
            .WithPort(cassandraOptions.Port)
            .WithCredentials(
                cassandraOptions.Credentials.UserName,
                cassandraOptions.Credentials.Password)
            .WithTypeSerializers(definitions)
            .WithQueryOptions(options)
            .Build();

        var session = RetryPolicy(cassandraOptions.RetryCount)
            .Execute(cluster.Connect);

        return services
            .AddSingleton(_ => cassandraOptions)
            .AddSingleton(_ => session)
            .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<ISession>()))
            .AddTransient<Mapper>(sp => ( sp.GetRequiredService<IMapper>() as Mapper )!);
    }

    public static TypeSerializerDefinitions? GetTypeSerializerDefinitions()
    {
        var typeSerializers = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericType: false } &&
                        ( t.BaseType?.IsGenericType ?? false ) &&
                        t.IsAssignableTo(typeof(TypeSerializer)))
            .Select(t => ( Activator.CreateInstance(t) as TypeSerializer, t.BaseType!.GetGenericArguments()[0] ))
            .Where(t => t.Item1 is not null)
            .ToList();

        var defs = new TypeSerializerDefinitions();
        foreach ( var (typeSerializer, type) in typeSerializers.Where(_ =>
                     _.Item1 is not UserNotificationMetadataTypeSerializer) )
        {
            defs = ( TypeSerializerDefinitions )defs.GetType()
                .GetMethod(nameof(TypeSerializerDefinitions.Define))!
                .MakeGenericMethod(type)
                .Invoke(defs, new[] { typeSerializer });
        }

        return defs;
    }

    private static ResiliencePipeline<ISession> RetryPolicy(
        int retryCount
    )
        => new ResiliencePipelineBuilder<ISession>()
            .AddRetry(new RetryStrategyOptions<ISession>
            {
                ShouldHandle = args =>
                    ValueTask.FromResult(
                        args.Outcome.Exception is NoHostAvailableException or SocketException or Exception),
                OnRetry = _ => ValueTask.CompletedTask,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(8),
                MaxRetryAttempts = retryCount,
                DelayGenerator = arguments =>
                    ValueTask.FromResult(( TimeSpan? )TimeSpan.FromSeconds(Math.Pow(2, arguments.AttemptNumber)))
            }).Build();

    public static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddCassandra(configuration)
            .AddHostedService<DatabaseInitializationService>();

    public static IServiceCollection AddCassandraIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>()
            .AddSettings<JwtSettings>(configuration);

        services
            .AddIdentity<ChatifyUser, CassandraIdentityRole>(ConfigureIdentityOptions)
            .AddCassandraErrorDescriber<CassandraErrorDescriber>()
            .UseCassandraStores<ISession>()
            .AddDefaultTokenProviders();

        services
            .AddAuthorization()
            .ConfigureExternalCookie(opts =>
            {
                opts.Cookie.SameSite = SameSiteMode.None;
                opts.Events.OnRedirectToAccessDenied = ctx => Task.CompletedTask;
                opts.Events.OnValidatePrincipal = ctx => Task.CompletedTask;
                opts.Cookie.HttpOnly = false;
            })
            .ConfigureApplicationCookie(opts =>
            {
                opts.Cookie.SameSite = SameSiteMode.None;
                opts.Events.OnRedirectToAccessDenied = ctx => Task.CompletedTask;
                opts.Events.OnValidatePrincipal = ctx => Task.CompletedTask;
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