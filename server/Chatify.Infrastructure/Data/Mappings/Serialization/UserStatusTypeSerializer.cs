using Cassandra;
using Cassandra.Serialization;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Data.Mappings.Serialization;

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