using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Shared.Abstractions.Dispatchers;

public interface IDispatcher
{
    Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
    
    Task<R> SendAsync<T, R>(T command, CancellationToken cancellationToken = default) where T : class, ICommand<R>;

    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;

    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}