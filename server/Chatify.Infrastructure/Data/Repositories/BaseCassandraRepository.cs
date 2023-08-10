using Cassandra;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using Chatify.Infrastructure.Common.Mappings;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public abstract class BaseCassandraRepository<TEntity, TDataEntity, TId> :
    IDomainRepository<TEntity, TId> where TEntity : class
{
    protected readonly IMapper Mapper;
    protected readonly Mapper DbMapper;

    protected readonly string IdColumn;

    protected BaseCassandraRepository(IMapper mapper, Mapper dbMapper, string? idColumn = default!)
    {
        Mapper = mapper;
        DbMapper = dbMapper;

        var definition = MappingConfiguration.Global.Get<TDataEntity>();
        IdColumn = idColumn ?? definition.PartitionKeys[0];
    }

    public async Task<TEntity> SaveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = entity.To<TDataEntity>(Mapper);

        await DbMapper.InsertAsync(dataEntity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));

        return entity;
    }

    public async Task<TEntity?> UpdateAsync(TId id, Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {IdColumn} = ?", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);
        updateAction(entity);
        Mapper.Map(entity, dataEntity);

        await DbMapper.UpdateAsync(dataEntity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));

        return entity;
    }

    public async Task<TEntity?> UpdateAsync(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {IdColumn} = ?", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);
        await updateAction(entity);
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
            await DbMapper.DeleteAsync<TDataEntity>($"WHERE {IdColumn} = ?", id);
            return true;
        }
        catch ( Exception )
        {
            return false;
        }
    }

    public Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<TDataEntity>($"WHERE {IdColumn} = ?", id)
            .ToAsync<TDataEntity, TEntity>(Mapper);
}