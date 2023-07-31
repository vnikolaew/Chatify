namespace Chatify.Domain.Entities;

public class ChatGroup
{
    public Guid Id { get; set; }
    
    public Guid  CreatorId { get; set; }

    public string Name { get; set; } = default!;
    
    public string About { get; set; } = default!;

    public string PictureUrl { get; set; } = default!;

    public ISet<Guid> AdminIds { get; set; } = new HashSet<Guid>();

    public ISet<User> Admins { get; set; } = new HashSet<User>();
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}