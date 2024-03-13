namespace Chatify.Shared.Abstractions.Queries;

public static class Extensions
{
    public static CursorPaged<T> ToCursorPaged<T>(this IEnumerable<T> items, string? pagingCursor, long total = default)
        => new(items, pagingCursor, items.Count(), total, pagingCursor is not null);
    
    public static CursorPaged<T> ToCursorPaged<T>(this IEnumerable<T> items, string pagingCursor, bool hasMore = true, long total = default)
        => new(items, pagingCursor, items.Count(), total, hasMore);
}