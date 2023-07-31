using System.Linq.Expressions;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;

namespace Chatify.Infrastructure.Data.Counters;

public class BaseCounterService<TEntity, TId> : ICounterService<TEntity, TId>
{
    protected readonly Expression<Func<TEntity, long>> PropertyGetter;
    protected readonly IMapper Mapper;

    public BaseCounterService(Expression<Func<TEntity, long>> propertyGetter, IMapper mapper)
    {
        PropertyGetter = propertyGetter;
        Mapper = mapper;
    }

    public async Task<TEntity?> Increment(TId id, long by = 1, CancellationToken cancellationToken = default)
    {
        var entity = await Mapper.FirstOrDefaultAsync<TEntity>("WHERE id = ?", id);
        if (entity is null) return default;

        var newCount = PropertyGetter.Compile()(entity);
        ++newCount;
            
        await Mapper.UpdateAsync(
            entity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));
        
        return entity;
    }

    public async Task<TEntity?> Decrement(TId id, long by = 1, CancellationToken cancellationToken = default)
    {
        var entity = await Mapper.FirstOrDefaultAsync<TEntity>("WHERE id = ?", id);
        if (entity is null) return default;
        
        var newCount = PropertyGetter.Compile()(entity);
        --newCount;
            
        await Mapper.UpdateAsync(
            entity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));
        
        return entity;
    }
}