namespace Chatify.Shared.Abstractions.Serialization;

public interface ISerializer
{
    string Serialize<T>(T value);
    
    void Serialize<T>(T value, Stream stream);

    T? Deserialize<T>(string payload);
    
    T? Deserialize<T>(Stream stream);
}