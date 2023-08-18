using System.Reflection;

namespace Chatify.Infrastructure.Data.Services;

public interface IEntityChangeTracker
{
    public Dictionary<string, object?> Track<TEntity>(
        TEntity entity,
        Action<TEntity> updateAction);
    
    public Task<Dictionary<string, object?>> TrackAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task> updateAction);
}

internal sealed class EntityChangeTracker : IEntityChangeTracker
{
    public Dictionary<string, object?> Track<TEntity>(
        TEntity entity,
        Action<TEntity> updateAction)
    {
        var currentProps = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name,
                p => p.GetValue(entity));

        updateAction(entity);
        var newProps = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name,
                p => p.GetValue(entity));

        var changedProps = TrackChangedProperties(
            currentProps, newProps);

        return changedProps;
    }

    public async Task<Dictionary<string, object?>> TrackAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task> updateAction)
    {
        var currentProps = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name,
                p => p.GetValue(entity));

        await updateAction(entity);
        var newProps = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name,
                p => p.GetValue(entity));

        var changedProps = TrackChangedProperties(
            currentProps, newProps);

        return changedProps;
    }

    private Dictionary<string, object?> TrackChangedProperties(
        Dictionary<string, object?> oldProps,
        Dictionary<string, object?> newProps)
    {
        var changes = new Dictionary<string, object?>();
        foreach ( var (propsName, propValue) in oldProps )
        {
            var newProp = newProps[propsName];
            if ( propValue is null && newProp is null ) continue;
            if ( propValue is null && newProp is not null )
            {
                changes.Add(propsName, newProp);
            }

            if ( !propValue?.Equals(newProp) ?? false )
            {
                changes.Add(propsName, newProp);
            }
        }

        return changes;
    }
}