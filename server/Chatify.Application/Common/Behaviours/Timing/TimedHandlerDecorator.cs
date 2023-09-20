using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Common.Behaviours.Timing;

[Decorator]
public sealed class TimedHandlerDecorator<TQuery, TResult>(IQueryHandler<TQuery, TResult> inner,
        ILogger<TimedHandlerDecorator<TQuery, TResult>> logger)
    : IQueryHandler<TQuery, TResult>
    where TQuery : class, IQuery<TResult>
{
    public static readonly ISet<Type> EnabledQueries
        = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                ( t.IsAssignableTo(typeof(IQueryHandler<TQuery, TResult>))
                  && t.GetCustomAttribute<TimedAttribute>() is not null
                  && t.GetCustomAttribute<DecoratorAttribute>() is null )
                || t.IsAssignableTo(typeof(IQuery<>)))
            .Select(t => t.IsAssignableTo(typeof(IQuery<>))
                ? t
                : t.GetInterfaces()[0].GetGenericArguments()[0])
            .ToImmutableHashSet();

    private static bool IsTimingEnabled => EnabledQueries.Contains(typeof(TQuery));

    public async Task<TResult> HandleAsync(
        TQuery query, CancellationToken cancellationToken = default)
    {
        if ( !IsTimingEnabled ) return await inner.HandleAsync(query, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await inner.HandleAsync(query, cancellationToken);

        var elapsedMs = stopwatch.ElapsedMilliseconds;
        logger.LogInformation(
            "Query operation `{Query}` took {Milliseconds} milliseconds",
            typeof(TQuery).Name, elapsedMs);
        return result;
    }
}