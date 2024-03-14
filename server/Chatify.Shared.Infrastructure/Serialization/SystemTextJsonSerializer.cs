using System.Text.Json;
using Chatify.Shared.Abstractions.Serialization;

namespace Chatify.Shared.Infrastructure.Serialization;

public class SystemTextJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);

    public void Serialize<T>(T value,Stream stream)
        =>JsonSerializer.Serialize(stream, value);

    public T? Deserialize<T>(
        string payload)
        => JsonSerializer.Deserialize<T>(payload, Options);

    public T? Deserialize<T>(Stream stream)
        => JsonSerializer.Deserialize<T>(stream);
}