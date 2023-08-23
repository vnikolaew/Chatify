using System.Collections;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Internal;
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

    public static IMappingExpression<TSource, TDestination> MapRecordMember<TSource, TDestination, TMember>(
        this IMappingExpression<TSource, TDestination> mappingExpression,
        Expression<Func<TDestination, TMember>> destinationMember, Expression<Func<TSource, TMember>> sourceMember)
    {
        var memberInfo = ReflectionHelper.FindProperty(destinationMember);
        string memberName = memberInfo.Name;
        return mappingExpression
            .ForMember(destinationMember, opt => opt.MapFrom(sourceMember))
            .ForCtorParam(memberName, opt => opt.MapFrom(sourceMember));
    }

    public static List<T> ToList<T>(
        this IEnumerable enumerable,
        IMapper mapper)
        => enumerable
            .To<T>(mapper)
            .ToList();

    public static async Task<IQueryable<T>> ToAsync<T>(
        this Task<IEnumerable> task,
        IMapper mapper)
        => ( await task )
            .AsQueryable()
            .ProjectTo<T>(mapper.ConfigurationProvider);

    public static async Task<IQueryable<R>> ToAsync<T, R>(
        this Task<IEnumerable<T>> task,
        IMapper mapper)
        => ( await task )
            .AsQueryable()
            .ProjectTo<R>(mapper.ConfigurationProvider);

    public static async Task<IQueryable<R>> ToAsync<T, R>(
        this Task<List<T>> task,
        IMapper mapper)
        => ( await task )
            .AsQueryable()
            .ProjectTo<R>(mapper.ConfigurationProvider);

    public static async Task<List<R>> ToAsyncList<T, R>(
        this Task<List<T>> task,
        IMapper mapper)
        => ( await task )
            .AsQueryable()
            .ProjectTo<R>(mapper.ConfigurationProvider)
            .ToList();

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