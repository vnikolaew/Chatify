using System.Reflection;
using AspNetCore.Identity.Cassandra;
using AspNetCore.Identity.Cassandra.Extensions;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Seeding;
using Chatify.Infrastructure.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddSeeding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("UseSeeding")) return services;

        return services
            .AddHostedService<DatabaseSeedingService>()
            .Scan(s => s.FromExecutingAssembly()
                .AddClasses(c => c.AssignableTo<ISeeder>()
                    .Where(t => t is { IsAbstract: false, IsInterface: false }))
                .AsImplementedInterfaces()
                .WithScopedLifetime())
            .AddScoped<CompositeSeeder>();
    }

    public static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mappingsList = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsSubclassOf(typeof(Cassandra.Mapping.Mappings)))
            .Select(Activator.CreateInstance)
            .Cast<Cassandra.Mapping.Mappings>()
            .Where(m => m is not null)
            .ToList();

        foreach (var mapping in mappingsList)
        {
            MappingConfiguration.Global.Define(mapping);
        }

        services
            .AddCassandra(configuration)
            .AddTransient<Mapper>(sp => (sp.GetRequiredService<IMapper>() as Mapper)!)
            .AddHostedService<DatabaseInitializationService>()
            .AddIdentity<ChatifyUser, CassandraIdentityRole>(opts =>
            {
                opts.SignIn.RequireConfirmedEmail = false;

                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequireDigit = true;

                opts.User.RequireUniqueEmail = true;
            })
            .AddCassandraErrorDescriber<CassandraErrorDescriber>()
            .UseCassandraStores<ISession>()
            .AddDefaultTokenProviders();

        // services.TryDecorate<ISession>((session, sp) =>
        //     new LoggedSession(session, sp.GetRequiredService<ILogger<LoggedSession>>()));
        return services;
    }
}