using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupJoinRequest : IMapFrom<Domain.Entities.ChatGroupJoinRequest>
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public sbyte Status { get; set; }
}