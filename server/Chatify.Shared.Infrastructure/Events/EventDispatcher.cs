using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Events;

public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        var tasks = handlers.Select(handler => handler.HandleAsync(@event, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task PublishAsync<TEvent>(
        IEnumerable<TEvent> @events,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var tasks = @events
            .Select(e => PublishAsync(e, cancellationToken))
            .ToList();
        await Task.WhenAll(tasks);
    }
}