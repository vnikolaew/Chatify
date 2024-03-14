using Chatify.Shared.Abstractions.Common;
using IdGen;

namespace Chatify.Infrastructure.Common;

internal sealed class SnowflakeUuidGenerator : IGuidGenerator
{
    private static readonly IdGenerator IdGenerator = new(0);
    
    public Guid New()
    {
        var id = new IdGenerator(0).CreateId();
        const long additionalData = 9876543210;

        var combinedData = id ^ additionalData;

        Span<byte> buffer = stackalloc byte[16];
        
        BitConverter.GetBytes(id).CopyTo(buffer);
        BitConverter.GetBytes(combinedData).CopyTo(buffer[8..]);
        
        return new Guid(buffer);
    }

    public string NewStringId() => New().ToString();
}