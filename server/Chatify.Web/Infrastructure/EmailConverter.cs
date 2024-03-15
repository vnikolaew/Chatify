using System.Text.Json;
using System.Text.Json.Serialization;
using Chatify.Domain.ValueObjects;

namespace Chatify.Web.Infrastructure;

public sealed class EmailConverter : JsonConverter<Email>
{
    public override Email? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}