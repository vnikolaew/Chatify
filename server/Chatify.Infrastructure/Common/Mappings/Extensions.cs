using System.Collections;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Chatify.Infrastructure.Common.Mappings;

public static class Extensions
{
    public static IQueryable<T> To<T>(
        this IQueryable queryable,
        IMapper mapper)
        => queryable.ProjectTo<T>(mapper.ConfigurationProvider);
    
    public static IQueryable<T> To<T>(
        this IEnumerable enumerable,
        IMapper mapper)
        => enumerable
            .AsQueryable()
            .ProjectTo<T>(mapper.ConfigurationProvider);
    
    public static async Task<IQueryable<T>> ToAsync<T>(
        this Task<IEnumerable> task,
        IMapper mapper)
        => (await task)
            .AsQueryable()
            .ProjectTo<T>(mapper.ConfigurationProvider);

    public static T To<T>(this object value, IMapper mapper)
        => mapper.Map<T>(value);

    public static async Task<T?> ToAsyncNullable<TFrom, T>(this Task<TFrom?> value, IMapper mapper)
        where TFrom : notnull
        => await value switch
        {
            { } item => item.To<T>(mapper),
            null => default
        };

    public static async Task<T> ToAsync<TFrom, T>(this Task<TFrom> value, IMapper mapper)
        where TFrom : notnull
        => ( await value ).To<T>(mapper);
}