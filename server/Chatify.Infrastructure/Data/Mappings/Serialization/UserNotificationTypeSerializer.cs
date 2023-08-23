using Cassandra;
using Cassandra.Serialization;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Data.Mappings.Serialization;

public sealed class UserNotificationTypeSerializer
    : TypeSerializer<UserNotificationType>
{
    public override UserNotificationType Deserialize(
        ushort protocolVersion,
        byte[] buffer, int offset, int length, IColumnInfo typeInfo)
    {
        if ( buffer is null ) return UserNotificationType.Unspecified;
        return ( UserNotificationType )( sbyte )buffer[offset];
    }

    public override ColumnTypeCode CqlType => ColumnTypeCode.TinyInt;

    public override byte[] Serialize(ushort protocolVersion, UserNotificationType value)
        => new[] { ( byte )( sbyte )value };
}