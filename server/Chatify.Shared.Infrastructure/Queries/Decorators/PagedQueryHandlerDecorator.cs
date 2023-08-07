using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Shared.Infrastructure.Queries.Decorators;

[Decorator]
public sealed class PagedQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : class, IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _handler;

    public PagedQueryHandlerDecorator(IQueryHandler<TQuery, TResult> handler)
        => _handler = handler;

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        const int maxResults = 100;
        const int defaultResults = 10;
            
        if (query is IPagedQuery pagedQuery)
        {
            pagedQuery.Page = Math.Max(1, pagedQuery.Page);
            pagedQuery.Results = Math.Min(maxResults, pagedQuery.Results);

            if (pagedQuery.Results <= 0)
            {
                pagedQuery.Results = defaultResults;
            }
        }

        return await _handler.HandleAsync(query, cancellationToken);
    }
}