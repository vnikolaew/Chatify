using System.Text.Json;
using Cassandra;
using Cassandra.Mapping.TypeConversion;
using Cassandra.Serialization;
using Chatify.Domain.Entities;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings.Serialization;

public sealed class UserNotificationMetadataTypeSerializer :
    TypeSerializer<UserNotificationMetadata>
{
    public override UserNotificationMetadata Deserialize(
        ushort protocolVersion, byte[] buffer, int offset, int length,
        IColumnInfo typeInfo)
    {
        // First deserialize to a dictionary: 
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(
            buffer.AsSpan()[offset..( offset + length )]);

        return dict is not null
            ? new UserNotificationMetadata
            {
                UserMedia = JsonSerializer.Deserialize<Media>(
                    dict[nameof(UserNotificationMetadata.UserMedia).Underscore()])
            }
            : default!;
    }

    public override ColumnTypeCode CqlType => ColumnTypeCode.Map;

    public override byte[] Serialize(
        ushort protocolVersion,
        UserNotificationMetadata value)
        => JsonSerializer
            .SerializeToUtf8Bytes(new Dictionary<string, object?>
            {
                { "user_media", value.UserMedia }
            });
}

public sealed class UserStatusTypeSerializer :
    TypeSerializer<UserStatus>
{
    public override UserStatus Deserialize(
        ushort protocolVersion,
        byte[] buffer,
        int offset,
        int length,
        IColumnInfo typeInfo)
    {
        if ( buffer is null ) return UserStatus.Online;
        return ( UserStatus )( sbyte )buffer[offset];
    }

    public override ColumnTypeCode CqlType => ColumnTypeCode.TinyInt;

    public override byte[] Serialize(ushort protocolVersion, UserStatus value)
        => new[] { ( byte )( sbyte )value };
}

public class CustomTypeConverter : TypeConverter
{
    protected override Func<TDatabase, TPoco>? GetUserDefinedFromDbConverter<TDatabase, TPoco>()
    {
        if ( typeof(TDatabase).IsAssignableTo(typeof(IDictionary<string, string>))
             && typeof(TPoco) == typeof(UserNotificationMetadata) )
        {
            return ( Func<IDictionary<string, string>, UserNotificationMetadata> )(
                database => new UserNotificationMetadata
                {
                    UserMedia = JsonSerializer.Deserialize<Domain.Entities.Media?>(
                        database.TryGetValue("user_media", out var media)
                            ? media
                            : default!)
                } ) as Func<TDatabase, TPoco>;
        }

        return default;
    }

    protected override Func<TPoco, TDatabase> GetUserDefinedToDbConverter<TPoco, TDatabase>()
    {
        if ( typeof(TDatabase).IsAssignableTo(typeof(IDictionary<string, string>))
             && typeof(TPoco) == typeof(UserNotificationMetadata) )
        {
            return ( ( Func<UserNotificationMetadata, IDictionary<string, string>> )(
                userNotification => new Dictionary<string, string>
                {
                    {
                        "user_media", JsonSerializer.Serialize(userNotification, new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                            PropertyNameCaseInsensitive = true
                        })
                    }
                } ) as Func<TPoco, TDatabase> )!;
        }

        return default!;
    }
}