using System.Reflection;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Events;

public static class Extensions
{
    public enum EventDispatcherType
    {
        TaskWhenAll,
        FireAndForget
    }

    public static IServiceCollection AddEvents(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        EventDispatcherType dispatcherType = EventDispatcherType.TaskWhenAll)
    {
        if (dispatcherType is EventDispatcherType.FireAndForget)
        {
            services.AddSingleton<IEventDispatcher, FireAndForgetEventDispatcher>();
        }
        else services.AddSingleton<IEventDispatcher, EventDispatcher>();

        return services
            .Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>))
                    .WithoutAttribute<DecoratorAttribute>(), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }
}