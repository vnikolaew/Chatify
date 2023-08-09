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

        return ToPagingCursor(buffer);
    }

    public string CombineCursors(string pagingCursorOne, string pagingCursorTwo)
    {
        var pagingStateOne = ToPagingState(pagingCursorOne);
        var pagingStateTwo = ToPagingState(pagingCursorTwo);

        int psOneLength = pagingStateOne.Length;
        var psOneLengthBytes = BitConverter.GetBytes(psOneLength);

        int psTwoLength = pagingStateTwo.Length;
        var psTwoLengthBytes = BitConverter.GetBytes(psTwoLength);


        var buffer = new byte[8 + psOneLength + psTwoLength];
        Array.Copy(psOneLengthBytes, 0, buffer, 0, 4);
        Array.Copy(pagingStateOne, 0, buffer, 4, psOneLength);

        Array.Copy(psTwoLengthBytes, 0, buffer, psOneLength + 4, 4);
        Array.Copy(pagingStateTwo, 0, buffer, 4, psOneLength + 8);

        return ToPagingCursor(buffer);
    }

    public string ToPagingCursor(byte[] pagingState)
        => Convert.ToBase64String(pagingState);

    public byte[] ToPagingState(string pagingCursor)
        => Convert.FromBase64String(pagingCursor);

    public IEnumerable<string> ToPagingCursors(string pagingCursor)
    {
        var pagingStates = Convert.FromBase64String(pagingCursor);

        int currIdx = 0;
        while ( currIdx < pagingStates.Length )
        {
            var psLength = BitConverter.ToInt32(pagingStates.AsSpan()[currIdx..(currIdx + 3)]);
            currIdx += 4;
            var pagingState = pagingStates[currIdx..( currIdx + psLength )];
            
            yield return ToPagingCursor(pagingState);
        }
    }
}