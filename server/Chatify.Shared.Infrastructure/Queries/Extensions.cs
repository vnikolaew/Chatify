using System.Reflection;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Queries.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Queries;

public static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        foreach ( var assembly in assemblies )
        {
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                            t.GetCustomAttribute<DecoratorAttribute>() is null &&
                            t.GetInterfaces()
                                .Where(i => i.IsGenericType)
                                .Any(i => i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                                )
                )
                .ToDictionary(t => t, t => t.GetInterfaces());

            foreach ( var (type, interfaces) in types )
            {
                foreach ( var @interface in interfaces )
                {
                    services.AddScoped(@interface, type);
                }
            }
        }

        return services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
    }

    public static IServiceCollection AddPagedQueryDecorator(this IServiceCollection services)
    {
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(PagedQueryHandlerDecorator<,>));
        return services;
    }
}