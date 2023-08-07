namespace Chatify.Infrastructure.Data.Repositories;

public static class CassandraPagingHelper
{
    public static string ToPagingCursor(byte[] pagingState)
        => Convert.ToBase64String(pagingState);

    public static byte[] ToPagingState(string pagingCursor)
        => Convert.FromBase64String(pagingCursor);
}