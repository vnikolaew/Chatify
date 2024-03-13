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

    /// <summary>
    /// Combine the elements of 2 collections using a zipping function.
    /// </summary>
    /// <param name="enumerable">The first enumerable.</param>
    /// <param name="enumerableTwo">The second enumerable.</param>
    /// <param name="enumerableKeySelector">A key selector for the first enumerable.</param>
    /// <param name="enumerableTwoKeySelector">A key selector for the second enumerable.</param>
    /// <param name="zipper"></param>
    /// <returns>The resulting combined list.</returns>
    public static List<TResult?> ZipOn<T1, T2, TKey, TResult>(
        this IEnumerable<T1> enumerable,
        IEnumerable<T2> enumerableTwo,
        Func<T1, TKey?> enumerableKeySelector,
        Func<T2, TKey?> enumerableTwoKeySelector,
        Func<T1, T2, TResult> zipper)
        where TKey : notnull
    {
        var listOne = enumerable.ToList();

        var dictTwo = enumerableTwo
            .Where(_ => _ is not null && enumerableTwoKeySelector(_) is not null)
            .ToDictionary(enumerableTwoKeySelector);

        return listOne.Select(
                item =>
                {
                    if ( item is null ) return zipper(default, default);
                    var key = enumerableKeySelector(item);
                    return key is not null && dictTwo.TryGetValue(key, out var value)
                        ? zipper(item, value!)
                        : zipper(item, default!);
                })
            .ToList();
    }
}