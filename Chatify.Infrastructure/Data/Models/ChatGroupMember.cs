using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMember : IMapFrom<Domain.Entities.ChatGroupMember>
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public sbyte MembershipType { get; set; }
}