using Chatify.Application.Common.Contracts;

namespace Chatify.Infrastructure.Data.Repositories;

public class CassandraPagingCursorHelper : IPagingCursorHelper
{
    public string CombineCursors(params string[] pagingCursors)
    {
        var pagingStates = pagingCursors
            .Select(ToPagingState)
            .ToList();

        var buffer = new byte[4 * pagingStates.Count + pagingStates.Sum(s => s.Length)];
        var currIdx = 0;
        foreach ( var pagingState in pagingStates )
        {
            int psLength = pagingState.Length;
            var psLengthBytes = BitConverter.GetBytes(psLength);

            Array.Copy(psLengthBytes, 0, buffer, currIdx, 4);
            currIdx += 4;
            Array.Copy(pagingState, 0, buffer, currIdx, psLength);
            currIdx += psLength;
        }

        return ToPagingCursor(buffer)!;
    }

    public string CombineCursors(string pagingCursorOne,
        string pagingCursorTwo)
    {
        if ( string.IsNullOrEmpty(pagingCursorOne)
             || string.IsNullOrEmpty(pagingCursorTwo) )
        {
            return default;
        }
        
        var pagingStateOne = ToPagingState(pagingCursorOne)!;
        var pagingStateTwo = ToPagingState(pagingCursorTwo)!;

        int psOneLength = pagingStateOne.Length;
        var psOneLengthBytes = BitConverter.GetBytes(psOneLength);

        int psTwoLength = pagingStateTwo.Length;
        var psTwoLengthBytes = BitConverter.GetBytes(psTwoLength);

        var buffer = new byte[
            psOneLengthBytes.Length
            + psTwoLengthBytes.Length
            + psOneLength
            + psTwoLength];
        int currIdx = 0;

        Array.Copy(psOneLengthBytes, 0, buffer, currIdx, psOneLengthBytes.Length);
        currIdx += psOneLengthBytes.Length;

        Array.Copy(pagingStateOne, 0, buffer, currIdx, psOneLength);
        currIdx += pagingStateOne.Length;

        Array.Copy(psTwoLengthBytes, 0, buffer, currIdx, psTwoLengthBytes.Length);
        currIdx += psTwoLengthBytes.Length;

        Array.Copy(pagingStateTwo, 0, buffer, currIdx, psTwoLength);
        currIdx += psTwoLengthBytes.Length;

        return ToPagingCursor(buffer)!;
    }

    public string? ToPagingCursor(byte[]? pagingState)
        => pagingState is null ? null : Convert.ToBase64String(pagingState);

    public byte[]? ToPagingState(string? pagingCursor)
        => string.IsNullOrEmpty(pagingCursor) ? null : Convert.FromBase64String(pagingCursor);

    public IEnumerable<string> ToPagingCursors(string pagingCursor)
    {
        var pagingStatesSpan = Convert.FromBase64String(pagingCursor).AsSpan();

        int currIdx = 0;
        List<string> cursors = new();
        while ( currIdx < pagingStatesSpan.Length )
        {
            var psLength = BitConverter.ToInt32(pagingStatesSpan[currIdx..( currIdx + 4 )]);
            currIdx += 4;
            var pagingState = pagingStatesSpan[currIdx..( currIdx + psLength )];
            currIdx += psLength;

            cursors.Add(ToPagingCursor(pagingState.ToArray())!);
        }

        return cursors;
    }
}