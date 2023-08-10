namespace Chatify.Application.Common.Contracts;

public interface IGuidGenerator
{
    Guid New();
    
    public string NewStringId();
}