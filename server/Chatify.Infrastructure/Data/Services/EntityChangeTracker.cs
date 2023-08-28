﻿using System.Collections;
using System.Reflection;
using Chatify.Infrastructure.Common.Mappings;
using Microsoft.AspNetCore.Hosting;

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
        var currentProps = GetProps(entity);
        updateAction(entity);
        var newProps = GetProps(entity);

        var changedProps = TrackChangedProperties(
            currentProps, newProps);

        return changedProps;
    }

    private static Dictionary<string, object?> GetProps<TEntity>(TEntity entity)
        => typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name,
                p =>
                {
                    var value = p.GetValue(entity);

                    // Handle collections separately:
                    if ( value is IEnumerable enumerable and not string )
                    {
                        if ( value is List<object> list ) return new List<object>(list);
                        if ( value is Dictionary<string, string> dictionary )
                        {
                            return new Dictionary<string, string>(dictionary);
                        }

                        if ( value is HashSet<object> set ) return new HashSet<object>(set);

                        if ( value is object[] array )
                        {
                            var newArray = new object[array.Length];
                            
                            array.CopyTo(newArray, 0);
                            return newArray;
                        }
                    }
                    
                    return value;
                });

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

    private static Dictionary<string, object?> TrackChangedProperties(
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