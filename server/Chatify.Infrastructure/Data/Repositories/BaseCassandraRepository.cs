using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using AspNetCore.Identity.Cassandra.Extensions;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Humanizer;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public abstract class BaseCassandraRepository<TEntity, TDataEntity, TId> :
    IDomainRepository<TEntity, TId>
    where TEntity : class
    where TDataEntity : notnull
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
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ? ALLOW FILTERING;", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);

        var changedProps = _changeTracker.Track(entity, updateAction);
        Mapper.Map(entity, dataEntity);

        var cql = GetUpdateCqlStatement(dataEntity, changedProps);
        await DbMapper.ExecuteAsync(cql);

        return entity;
    }

    public async Task<TEntity?> UpdateAsync(
        TEntity entity,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var changedProps = _changeTracker.Track(entity, updateAction);
        var dataEntity = Mapper.Map<TDataEntity>(entity);

        var cql = GetUpdateCqlStatement(dataEntity, changedProps);
        await DbMapper.ExecuteAsync(cql);

        return entity;
    }

    private static Cql GetUpdateCqlStatement(
        TDataEntity entity,
        Dictionary<string, object?> changedProps)
    {
        if ( changedProps.Count == 0 ) return new Cql("SELECT 1 + 1 ;");

        var tableColumnNames = changedProps
            .Keys
            .Select(prop => typeof(TDataEntity).GetProperty(prop)!)
            .Select(pi => MappingDefinition.GetColumnDefinition(pi).ColumnName ?? pi.Name.Underscore())
            .ToList();

        var primaryKeys = MappingDefinition
            .PartitionKeys
            .ToHashSet();
        var clusteringKeys =  MappingDefinition
            .ClusteringKeys
            .Select(_ => _.Item1)
            .ToHashSet();
        primaryKeys.UnionWith(clusteringKeys);
        
        var partitionKeyValues = entity
            .GetType()
            .GetProperties()
            .OrderBy(pi => MappingDefinition.GetColumnDefinition(pi).ColumnName)
            .Where(pi => primaryKeys.Contains(MappingDefinition.GetColumnDefinition(pi).ColumnName))
            .Select(pi => pi.GetValue(entity))
            .ToList();

        var filterStatement = string.Join(" AND ", primaryKeys.OrderBy(_ => _).Select(key => $"{key} = ?"));
        var cql = new Cql($" UPDATE {MappingDefinition.TableName} SET {string.Join(
            ", ",
            tableColumnNames
                .Select(c => $"{c} = ?").ToList())} WHERE {filterStatement};")
            .WithOptions(opts =>
                opts.SetConsistencyLevel(ConsistencyLevel.Quorum)
                    .SetRetryPolicy(new DefaultRetryPolicy()));

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, changedProps
                .Select(_ => _.Value)
                .Append(partitionKeyValues)
                .ToArray());

        return cql;
    }

    public async Task<TEntity?> UpdateAsync(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = await DbMapper.FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ? ALLOW FILTERING;", id);
        if ( dataEntity is null ) return default;

        var entity = dataEntity.To<TEntity>(Mapper);

        var changedProps = await _changeTracker.TrackAsync(entity, updateAction);
        Mapper.Map(entity, dataEntity);

        var cql = GetUpdateCqlStatement(dataEntity, changedProps);
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

    public virtual Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<TDataEntity>($"WHERE {_idColumn} = ? ALLOW FILTERING;", id)
            .ToAsync<TDataEntity, TEntity>(Mapper)!;
}