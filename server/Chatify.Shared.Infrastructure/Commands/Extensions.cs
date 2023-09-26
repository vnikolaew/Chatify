using System.Reflection;
using Chatify.Shared.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Commands;

public static class Extensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        foreach ( var assembly in assemblies )
        {
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                            t.GetCustomAttribute<DecoratorAttribute>() is null &&
                            t.GetInterfaces()
                                .Where(i => i.IsGenericType)
                                .Any(i => i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                                          || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
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

        return services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
    }
}