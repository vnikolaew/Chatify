using System.Text.Json;
using System.Text.Json.Serialization;
using Chatify.Shared.Abstractions.Queries;
using Humanizer;
using Microsoft.AspNetCore.Http.Features;

namespace Chatify.Web.Middleware;

public sealed class CursorPagedConverter<T>
    : JsonConverter<CursorPaged<T>>
{
    public override CursorPaged<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        string pagingCursor = root.GetProperty(nameof(CursorPaged<T>.PagingCursor)).GetString()!;
        int pageSize = root.GetProperty(nameof(CursorPaged<T>.PageSize)).GetInt32()!;
        List<T> items =
            JsonSerializer.Deserialize<List<T>>(root.GetProperty(nameof(CursorPaged<T>.Items)).GetRawText(), options)!;

        return new CursorPaged<T>(items, pagingCursor, pageSize);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CursorPaged<T> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(CursorPaged<T>.PagingCursor).Camelize());
        writer.WriteStringValue(value.PagingCursor);

        writer.WritePropertyName(nameof(CursorPaged<T>.HasMore).Camelize());
        writer.WriteBooleanValue(value.HasMore);

        writer.WritePropertyName(nameof(CursorPaged<T>.Total).Camelize());
        writer.WriteNumberValue(value.Total);

        writer.WritePropertyName(nameof(CursorPaged<T>.PageSize).Camelize());
        writer.WriteNumberValue(value.PageSize);

        writer.WritePropertyName(nameof(CursorPaged<T>.Items).Camelize());
        writer.WriteStartArray();
        foreach ( var item in value.Items )
        {
            JsonSerializer.Serialize(writer, item, item.GetType(), options);
        }
        writer.WriteEndArray();


        writer.WriteEndObject();
    }
}