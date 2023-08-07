using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Users;

public class UserSignedInEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    
    public DateTime Timestamp { get; set; }

    public string AuthenticationProvider { get; set; } = default!;
}