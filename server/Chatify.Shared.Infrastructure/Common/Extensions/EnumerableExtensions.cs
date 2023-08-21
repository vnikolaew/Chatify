namespace Chatify.Shared.Infrastructure.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>(
        this IEnumerable<T1> enumerable,
        IEnumerable<T2> enumerableTwo,
        IEnumerable<T3> enumerableThree,
        Func<T1, T2, T3, TResult> zipper)
    {
        using var enumeratorOne = enumerable.GetEnumerator();
        using var enumeratorTwo = enumerableTwo.GetEnumerator();
        using var enumeratorThree = enumerableThree.GetEnumerator();

        while ( enumeratorOne.MoveNext() && enumeratorTwo.MoveNext() && enumeratorThree.MoveNext() )
        {
            yield return zipper(
                enumeratorOne.Current,
                enumeratorTwo.Current,
                enumeratorThree.Current
            );
        }
    }

    public static IEnumerable<TResult> ZipOn<T1, T2, TKey, TResult>(
        this IEnumerable<T1> enumerable,
        IEnumerable<T2> enumerableTwo,
        Func<T1, TKey> enumerableKeySelector,
        Func<T2, TKey> enumerableTwoKeySelector,
        Func<T1, T2, TResult> zipper)
        where TKey : notnull
    {
        var dictOne = enumerable
            .GroupBy(enumerableKeySelector)
            .ToDictionary(gr => gr.Key, gr => gr.ToList());

        var dictTwo = enumerableTwo.ToDictionary(
            enumerableTwoKeySelector);

        var kvs = dictOne
            .Select(kv => kv.Value
                .Select(_ => new KeyValuePair<TKey, T1>(kv.Key, _)))
            .SelectMany(_ => _)
            .ToList();

        return kvs
            .Select(kv =>
            {
                var itemOne = kv.Value!;
                var itemTwo = dictTwo.TryGetValue(kv.Key, out var value)
                    ? value
                    : default;
                return zipper(itemOne, itemTwo!);
            })
            .ToList();
    }
}