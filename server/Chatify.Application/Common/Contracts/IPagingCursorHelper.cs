namespace Chatify.Application.Common.Contracts;

public interface IPagingCursorHelper
{
    string CombineCursors(string pagingCursorOne, string pagingCursorTwo);

    public IEnumerable<string> ToPagingCursors(string pagingCursor);

    public byte[] ToPagingState(string pagingCursor);
    
    public string ToPagingCursor(byte[] pagingState);

    public string CombineCursors(params string[] pagingCursors);
}