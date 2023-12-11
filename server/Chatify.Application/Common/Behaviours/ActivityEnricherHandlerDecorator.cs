using System.Diagnostics;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure;

namespace Chatify.Application.Common.Behaviours;

[Decorator]
internal sealed class ActivityEnricherHandlerDecorator<TRequest, TResponse>(
        IQueryHandler<TRequest, TResponse> inner,
        IIdentityContext identityContext)
    : IQueryHandler<TRequest, TResponse>
    where TRequest : class, IQuery<TResponse>
{
    private const string ActivitySourceName = "Chatify";
    
    public async Task<TResponse> HandleAsync(
        TRequest query,
        CancellationToken cancellationToken = default)
    {
        var source = new ActivitySource(ActivitySourceName);
        using var activity = source.StartActivity();

        activity.SetTag("request.type", typeof(TRequest).Name);
        activity.SetTag("user.id", identityContext.Id.ToString());
        
        return await inner.HandleAsync(query, cancellationToken);
    }
}