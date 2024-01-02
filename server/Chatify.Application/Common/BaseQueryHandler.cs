using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Application.Common;

internal abstract class BaseQueryHandler<TRequest, TResponse>(IIdentityContext identityContext)
    : IQueryHandler<TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>
{
    protected readonly IIdentityContext _identityContext = identityContext;

    public abstract Task<TResponse> HandleAsync(TRequest command,
        CancellationToken cancellationToken = default);
}