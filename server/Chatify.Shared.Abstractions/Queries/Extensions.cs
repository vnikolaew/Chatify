namespace Chatify.Shared.Abstractions.Queries;

public static class Extensions
{
    public static CursorPaged<T> ToCursorPaged<T>(this IEnumerable<T> items, string pagingCursor)
        => new CursorPaged<T>(items, pagingCursor);

}