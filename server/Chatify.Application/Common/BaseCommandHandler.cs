using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;

namespace Chatify.Application.Common;

internal abstract class BaseCommandHandler<TRequest, TResponse>(
    IEventDispatcher eventDispatcher,
    IIdentityContext identityContext,
    IClock clock)
    : ICommandHandler<TRequest, TResponse>
    where TRequest : class, ICommand<TResponse>
{
    public abstract Task<TResponse> HandleAsync(TRequest command,
        CancellationToken cancellationToken = default);
}