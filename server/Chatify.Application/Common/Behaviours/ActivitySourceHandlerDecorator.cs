using System.Diagnostics;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure;

namespace Chatify.Application.Common.Behaviours;

[Decorator]
public sealed class ActivitySourceHandlerDecorator<TRequest, TResponse>(
    IQueryHandler<TRequest, TResponse> inner,
    IIdentityContext identityContext
) : IQueryHandler<TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest query, CancellationToken cancellationToken = default)
    {
        var source = new ActivitySource("Chatify");
        using var activity = source.StartActivity(typeof(TRequest).Name, ActivityKind.Server);
        
        activity.SetTag("query.name", typeof(TRequest).Name);
        activity.SetTag("user.id", identityContext.Id);

        return await inner.HandleAsync(query, cancellationToken);
    }
}