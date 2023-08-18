using System.Reflection;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public abstract class BaseCassandraRepository<TEntity, TDataEntity, TId> :
    IDomainRepository<TEntity, TId> where TEntity : class
{
    protected readonly IMapper Mapper;
    protected readonly Mapper DbMapper;

    private readonly string _idColumn;
    private readonly IEntityChangeTracker _changeTracker;

    protected static readonly ITypeDefinition MappingDefinition
        = MappingConfiguration.Global.Get<TDataEntity>();
    
    protected BaseCassandraRepository(
        IMapper mapper,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker,
        string? idColumn = default!)
    {
        Mapper = mapper;
        DbMapper = dbMapper;
        _changeTracker = changeTracker;

        var definition = MappingConfiguration.Global.Get<TDataEntity>();
        _idColumn = idColumn ?? definition.PartitionKeys[0];
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

    public async Task<TEntity?> UpdateAsync(
        TId id, Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ?", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);

        var changedProps = _changeTracker.Track(entity, updateAction);
        Mapper.Map(entity, dataEntity);

        var cql = GetUpdateCqlStatement(id, changedProps);
        await DbMapper.ExecuteAsync(cql);
        
        return entity;
    }

    private static Cql GetUpdateCqlStatement(
        TId id,
        Dictionary<string, object?> changedProps)
    {
        var tableColumnNames = changedProps
            .Keys
            .Select(prop => typeof(TEntity).GetProperty(prop)!)
            .Select(pi => MappingDefinition.GetColumnDefinition(pi).ColumnName)
            .ToList();

        var cql = new Cql($" UPDATE {MappingDefinition.TableName} SET {string.Join(
            ", ",
            tableColumnNames
                .Select(c => $"{c} = ?").ToList())} WHERE {MappingDefinition.PartitionKeys[0].ToLower()} = ?;")
            .WithOptions(opts =>
                opts.SetConsistencyLevel(ConsistencyLevel.Quorum)
                    .SetRetryPolicy(new DefaultRetryPolicy()));

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, changedProps
                .Select(_ => _.Value)
                .Append(id).ToArray());
        
        return cql;
    }

    public async Task<TEntity?> UpdateAsync(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ?", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);

        var changedProps = await _changeTracker.TrackAsync(entity, updateAction);
        Mapper.Map(entity, dataEntity);

        var cql = GetUpdateCqlStatement(id, changedProps);
        await DbMapper.ExecuteAsync(cql);
        
        return entity;
    }

    public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<TDataEntity>($"WHERE {_idColumn} = ?", id);
            return true;
        }
        catch ( Exception )
        {
            return false;
        }
    }

    public Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ?", id)
            .ToAsync<TDataEntity, TEntity>(Mapper);
}