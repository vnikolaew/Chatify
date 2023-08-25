namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMemberByUser
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ChatGroupId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static ChatGroupMemberByUser From(Domain.Entities.ChatGroupMember member)
        => new()
        {
            Id = member.Id,
            UserId = member.UserId,
            CreatedAt = member.CreatedAt,
            ChatGroupId = member.ChatGroupId
        };
}