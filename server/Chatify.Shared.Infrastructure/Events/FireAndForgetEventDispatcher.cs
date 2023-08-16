﻿using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Events;

public sealed class FireAndForgetEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public FireAndForgetEventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;
    
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        var tasks = handlers.Select(handler => handler.HandleAsync(@event, cancellationToken));
        
        Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch ( Exception)
            {
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