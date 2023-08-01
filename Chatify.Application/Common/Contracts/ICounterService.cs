namespace Chatify.Application.Common.Contracts;

public interface ICounterService<TEntity, in TId>
{
    Task<TEntity?> Increment(TId id, long by = 1, CancellationToken cancellationToken = default);
    
    Task<TEntity?> Decrement(TId id, long by = 1, CancellationToken cancellationToken = default);
}