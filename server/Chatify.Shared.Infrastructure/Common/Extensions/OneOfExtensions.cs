using OneOf;

namespace Chatify.Shared.Infrastructure.Common.Extensions;

public static class OneOfExtensions
{
    public static async Task<TResult> MatchAsync<T1, T2, TResult>(
        this Task<OneOf<T1, T2>> task,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2
    )
    {
        var value = await task;
        return value.Match(f1, f2);
    }
    
    public static async Task<TResult> MatchAsync<T1, T2, T3, TResult>(
        this Task<OneOf<T1, T2, T3>> task,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2,
        Func<T3, TResult> f3
    )
    {
        var value = await task;
        return value.Match(f1, f2, f3);
    }
    
    public static async Task<TResult> MatchAsync<T1, T2, T3, T4, TResult>(
        this Task<OneOf<T1, T2, T3, T4>> task,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2,
        Func<T3, TResult> f3,
        Func<T4, TResult> f4
    )
    {
        var value = await task;
        return value.Match(f1, f2, f3, f4);
    }
    
    public static async Task<TResult> MatchAsync<T1, T2, T3, T4, T5, TResult>(
        this Task<OneOf<T1, T2, T3, T4, T5>> task,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2,
        Func<T3, TResult> f3,
        Func<T4, TResult> f4,
        Func<T5, TResult> f5
    )
    {
        var value = await task;
        return value.Match(f1, f2, f3, f4, f5);
    }
}