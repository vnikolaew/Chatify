using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Events.Users;

public class UserDetailsEditedEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    
    public string? DisplayName { get; set; }
    
    public Media? ProfilePicture { get; set; }
    
    public ISet<string>? PhoneNumbers { get; set; }
}