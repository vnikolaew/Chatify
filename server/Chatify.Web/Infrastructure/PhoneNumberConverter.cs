using System.Text.Json;
using System.Text.Json.Serialization;
using Chatify.Domain.ValueObjects;

namespace Chatify.Web.Infrastructure;

public sealed class PhoneNumberConverter : JsonConverter<PhoneNumber>
{
    public override PhoneNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}