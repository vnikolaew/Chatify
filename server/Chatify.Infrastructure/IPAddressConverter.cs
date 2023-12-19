using System.Net;
using System.Text.Json;

namespace Chatify.Infrastructure;

public sealed class IPAddressConverter
    : System.Text.Json.Serialization.JsonConverter<IPAddress>
{
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string ipAddressString = reader.GetString();
        return IPAddress.TryParse(ipAddressString, out var ipAddress)
            ? ipAddress
            : default;
    }

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}