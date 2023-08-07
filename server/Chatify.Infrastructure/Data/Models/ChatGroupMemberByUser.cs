namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMemberByUser
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ChatGroupId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}