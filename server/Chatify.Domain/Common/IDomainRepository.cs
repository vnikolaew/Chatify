namespace Chatify.Domain.Common;

public interface IDomainRepository<TEntity, in TId>
    where TEntity : class
{
    Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    Task<TEntity?> UpdateAsync(TId id, Action<TEntity> updateAction, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken = default);
}