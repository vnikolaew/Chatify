using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;

namespace Chatify.Application.Common;

internal abstract class BaseCommandHandler<TRequest, TResponse>
    : ICommandHandler<TRequest, TResponse> where TRequest : class, ICommand<TResponse>
{
    protected readonly IEventDispatcher _eventDispatcher;
    protected readonly IIdentityContext _identityContext;
    protected readonly IClock _clock;

    protected BaseCommandHandler(
        IEventDispatcher eventDispatcher,
        IIdentityContext identityContext,
        IClock clock)
    {
        _eventDispatcher = eventDispatcher;
        _identityContext = identityContext;
        _clock = clock;
    }

    public abstract Task<TResponse> HandleAsync(TRequest command,
        CancellationToken cancellationToken = default);
}