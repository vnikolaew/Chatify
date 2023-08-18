using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chatify.Shared.Infrastructure.Events;

public sealed class FireAndForgetEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FireAndForgetEventDispatcher> _logger;

    public FireAndForgetEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<FireAndForgetEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider
            .GetServices(typeof(IEventHandler<>)
                .MakeGenericType(@event.GetType()));

        var tasks = handlers
            .Select(handler =>
            {
                var methodInfo = handler!.GetType()
                    .GetMethod(nameof(IEventHandler<TEvent>.HandleAsync))!;

                return (methodInfo.Invoke(handler, new object?[] { @event, cancellationToken }) as Task)!;
            });

        Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch ( Exception e)
            {
                _logger.LogError(e, "An exception occurred during event handling of {EventType}",
                    typeof(TEvent).Name);
            }
        }, cancellationToken);
    }

    public async Task PublishAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var tasks = @events
            .Select(e => PublishAsync(e, cancellationToken))
            .ToList();
        await Task.WhenAll(tasks);
    }
}