namespace Chatify.Shared.Abstractions.Events;

public interface IEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent;
    
    Task PublishAsync<TEvent>(IEnumerable<TEvent> @events, CancellationToken cancellationToken = default) where TEvent : class, IEvent;
}
