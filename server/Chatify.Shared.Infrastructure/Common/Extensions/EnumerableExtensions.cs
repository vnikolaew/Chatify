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
    
}