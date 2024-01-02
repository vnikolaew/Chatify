using Cassandra;
using Cassandra.Serialization;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Data.Mappings.Serialization;

public sealed class FriendInvitationStatusSerializer
    : TypeSerializer<FriendInvitationStatus>
{
    public override FriendInvitationStatus Deserialize(
        ushort protocolVersion, byte[] buffer,
        int offset, int length, IColumnInfo typeInfo)
        => ( FriendInvitationStatus )( sbyte )buffer[offset];

    public override ColumnTypeCode CqlType => ColumnTypeCode.TinyInt;

    public override byte[] Serialize(
        ushort protocolVersion,
        FriendInvitationStatus value)
        => [( byte )( sbyte )value];
}