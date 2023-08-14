namespace Chatify.Domain.Entities;
using Metadata = IDictionary<string, string>;

public class ChatGroup
{
    public Guid Id { get; set; }
    
    public Guid  CreatorId { get; set; }

    public string Name { get; set; } = default!;
    
    public string About { get; set; } = default!;

    public Media Picture { get; set; } = default!;

    public ISet<Guid> AdminIds { get; set; } = new HashSet<Guid>();

    public ISet<User> Admins { get; set; } = new HashSet<User>();

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}