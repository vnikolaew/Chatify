using Cassandra;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public abstract class BaseCassandraRepository<TEntity, TDataEntity, TId> :
    IDomainRepository<TEntity, TId> where TEntity : class
{
    protected readonly IMapper Mapper;
    protected readonly Mapper DbMapper;

    protected BaseCassandraRepository(IMapper mapper, Mapper dbMapper)
    {
        Mapper = mapper;
        DbMapper = dbMapper;
    }

    public async Task<TEntity> SaveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = Mapper.Map<TDataEntity>(entity);
        await DbMapper.InsertAsync(dataEntity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));

        return entity;
    }

    public async Task<TEntity?> UpdateAsync(TId id, Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>("WHERE id = ?", id);
        if (dataEntity is null) return default;

        var entity = Mapper.Map<TEntity>(dataEntity);
        updateAction(entity);
        Mapper.Map(entity, dataEntity);
        
        await DbMapper.UpdateAsync(dataEntity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));
        
        return entity;
    }

    public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<TDataEntity>("WHERE id = ?", id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await DbMapper.FirstOrDefaultAsync<TDataEntity>("WHERE id = ?", id);
        return entity is null ? default : Mapper.Map<TEntity>(entity);
    }
}