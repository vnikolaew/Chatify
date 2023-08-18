using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Common.Behaviours.Timing;

internal sealed class TimedHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : class, IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly ILogger<TimedHandlerDecorator<TQuery, TResult>> _logger;

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

    public TimedHandlerDecorator(
        IQueryHandler<TQuery, TResult> inner,
        ILogger<TimedHandlerDecorator<TQuery, TResult>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(
        TQuery query, CancellationToken cancellationToken = default)
    {
        if ( !IsTimingEnabled ) return await _inner.HandleAsync(query, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await _inner.HandleAsync(query, cancellationToken);

        var elapsedMs = stopwatch.ElapsedMilliseconds;
        _logger.LogInformation(
            "Query operation `{Query}` took {Milliseconds} milliseconds",
            typeof(TQuery).Name, elapsedMs);
        return result;
    }
}