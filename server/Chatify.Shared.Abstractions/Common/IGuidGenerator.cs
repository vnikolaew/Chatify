namespace Chatify.Shared.Abstractions.Common;

public interface IGuidGenerator
{
    Guid New();
    
    public string NewStringId();
}