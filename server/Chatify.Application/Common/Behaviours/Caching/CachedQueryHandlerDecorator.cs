﻿using System.Collections.Immutable;
using System.Reflection;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure;
using Humanizer;

namespace Chatify.Application.Common.Behaviours.Caching;

[Decorator]
internal sealed class CachedQueryHandlerDecorator<TQuery, TResult>
    : IQueryHandler<TQuery, TResult> where TQuery : class, IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly IIdentityContext _identityContext;
    private readonly ICacheService _cache;

    private string UserId => _identityContext.Id.ToString();

    private static readonly List<PropertyInfo> PropertyCacheKeys =
        typeof(TQuery)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .Where(p => p.GetCustomAttribute<CacheKeyAttribute>() is not null)!
            .OrderBy(p => p.Name)
            .ToList();

    private static readonly ImmutableDictionary<Type, CachedAttribute> CachedQueryOptions
        = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsAssignableTo(typeof(IQueryHandler<TQuery, TResult>))
                && t.GetCustomAttribute<CachedAttribute>() is not null
                && t.GetCustomAttribute<DecoratorAttribute>() is null)
            .ToImmutableDictionary(t => t.GetGenericArguments()[0],
                t => t.GetCustomAttribute<CachedAttribute>()!);

    private static bool IsCachingEnabled => CachedQueryOptions.ContainsKey(typeof(TQuery));

    public CachedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResult> inner,
        ICacheService cache,
        IIdentityContext identityContext
    )
    {
        _inner = inner;
        _cache = cache;
        _identityContext = identityContext;
    }

    public async Task<TResult> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default)
    {
        if ( !IsCachingEnabled )
        {
            return await _inner.HandleAsync(query, cancellationToken);
        }

        var cacheAttribute = CachedQueryOptions[typeof(TQuery)];
        var keyPropertyValues = cacheAttribute switch
        {
            CachedByUserAttribute => new[] { UserId },
            _ => PropertyCacheKeys
                .Select(p => p.GetValue(query)!.ToString()!)
                .ToArray()
        };

        var cacheKey = $"{cacheAttribute.QueryCacheKeyPrefix}:{string.Join(":", keyPropertyValues)}";

        var item = await _cache.GetAsync<TResult>(cacheKey, cancellationToken);
        if ( item is not null ) return item;

        var result = await _inner.HandleAsync(query, cancellationToken);
        await _cache.SetAsync(cacheKey, result, cacheAttribute.TtlInSeconds?.Seconds(), cancellationToken);

        return result;
    }
}