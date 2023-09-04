using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Chatify.Infrastructure.Common.Mappings;
using FastDeepCloner;
using Microsoft.AspNetCore.Hosting;

namespace Chatify.Infrastructure.Data.Services;

public interface IEntityChangeTracker
{
    public Dictionary<string, object?> Track<TEntity>(
        TEntity entity,
        Action<TEntity> updateAction) where TEntity : class;

    public Task<Dictionary<string, object?>> TrackAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task> updateAction);

    Dictionary<string, object?> GetChangedProperties<TEntity>(
        TEntity entity,
        TEntity newEntity);
}

internal sealed class EntityChangeTracker : IEntityChangeTracker
{
    public Dictionary<string, object?> Track<TEntity>(
        TEntity entity,
        Action<TEntity> updateAction) where TEntity : class
    {
        var currentProps = GetProps(entity);
        var settings = new FastDeepClonerSettings
        {
            FieldType = FieldType.PropertyInfo,
            OnCreateInstance = FormatterServices.GetUninitializedObject
        };
        var cloneEntity = entity.Clone(settings);

        updateAction(cloneEntity);
        var newProps = GetProps(cloneEntity);

        var changedProps = TrackChangedProperties(
            currentProps, newProps);

        return changedProps;
    }

    public Dictionary<string, object?> GetChangedProperties<TEntity>(TEntity entity,
        TEntity newEntity)
        => TrackChangedProperties(GetProps(entity), GetProps(newEntity));

    public Dictionary<string, object?> GetProps<TEntity>(TEntity entity)
    {
        var props = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        return props.ToDictionary(p => p.Name,
            p => p.GetValue(entity));
    }

    public async Task<Dictionary<string, object?>> TrackAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task> updateAction)
    {
        var currentProps = GetProps(entity);
        await updateAction(entity);
        var newProps = GetProps(entity);

        var changedProps = TrackChangedProperties(
            currentProps, newProps);

        return changedProps;
    }

    public Dictionary<string, object?> TrackChangedProperties(
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

            // TODO: Modify equality comparison algorithm
            if ( propValue is IEnumerable enumerable and not string
                 && newProp is IEnumerable enumerableTwo and not string )
            {
                if ( enumerableTwo is Dictionary<string, string> _ )
                {
                    changes.Add(propsName, enumerableTwo);
                    continue;
                }

                var objectEnumerable = enumerable.Cast<object>().ToList();
                var objectEnumerableTwo = enumerableTwo.Cast<object>().ToList();

                if ( objectEnumerable.Count != objectEnumerableTwo.Count )
                {
                    changes.Add(propsName, newProp);
                    continue;
                }

                if ( !objectEnumerable.SequenceEqual(objectEnumerableTwo) )
                {
                    changes.Add(propsName, newProp);
                }

                continue;
            }

            if ( !propValue?.Equals(newProp) ?? false )
            {
                changes.Add(propsName, newProp);
            }
        }

        return changes;
    }
}