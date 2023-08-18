using System.Linq.Expressions;
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Humanizer;

namespace Chatify.Infrastructure.Data.Counters;

public abstract class BaseCounterService<TEntity, TId>(Expression<Func<TEntity, long>> propertyGetter,
        IMapper mapper)
    : ICounterService<TEntity, TId>
{
    protected readonly Expression<Func<TEntity, long>> PropertyGetter = propertyGetter;
    protected readonly IMapper Mapper = mapper;

    public async Task<TEntity?> Increment(TId id, long by = 1, CancellationToken cancellationToken = default)
    {
        var entity = await Mapper.FirstOrDefaultAsync<TEntity>("WHERE id = ?", id);
        if (entity is null) return default;

        var propName = (PropertyGetter.Body as MemberExpression)!
            .Member.Name.Underscore();

        await Mapper.UpdateAsync<TEntity>($"SET {propName} = {propName} + ?", by);
        
        var prop = PropertyGetter.Compile()(entity);
        prop += by;
        
        return entity;
    }

    public async Task<TEntity?> Decrement(TId id, long by = 1, CancellationToken cancellationToken = default)
    {
        var entity = await Mapper.FirstOrDefaultAsync<TEntity>("WHERE id = ?", id);
        if (entity is null) return default;

        var propName = (PropertyGetter.Body as MemberExpression)!
            .Member.Name.Underscore();

        await Mapper.UpdateAsync<TEntity>($"SET {propName} = {propName} - ?", by);
        
        var prop = PropertyGetter.Compile()(entity);
        prop -= by;
        
        return entity;
    }
}