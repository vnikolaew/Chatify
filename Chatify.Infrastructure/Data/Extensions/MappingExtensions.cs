using System.Linq.Expressions;
using Cassandra.Mapping;
using Humanizer;

namespace Chatify.Infrastructure.Data.Extensions;

public static class MappingExtensions
{
    public static Map<T> UnderscoreColumn<T, TProp>(
        this Map<T> map,
        Expression<Func<T, TProp>> expression,
        Action<ColumnMap>? configure = default)
    {
        var underscoreColumnName = (expression.Body as MemberExpression)?.Member.Name ??
                                   throw new ArgumentException("Expression must be a member expression.",
                                       nameof(expression));

        return map.Column(expression, m =>
        {
            m.WithName(underscoreColumnName.Underscore().ToLower());
            configure?.Invoke(m);
        });
    }

    public static Map<T> SetColumn<T, TProp, TValue>(
        this Map<T> map,
        Expression<Func<T, TProp>> expression)
    {
        var lowerColumnName = (expression.Body as MemberExpression)?.Member?.Name?.ToLower() ??
                              throw new ArgumentException("Expression must be a member expression.",
                                  nameof(expression));
        return map.Column(expression,
            m => m.WithDbType<HashSet<TValue>>().WithName(lowerColumnName));
    }

    public static Map<T> ListColumn<T, TProp, TValue>(
        this Map<T> map,
        Expression<Func<T, TProp>> expression)
    {
        var lowerColumnName = (expression.Body as MemberExpression)?.Member?.Name?.ToLower() ??
                              throw new ArgumentException("Expression must be a member expression.",
                                  nameof(expression));

        return map.Column(expression,
            m => m.WithDbType<List<TValue>>().WithName(lowerColumnName));
    }
}