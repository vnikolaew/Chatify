using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

public class FriendsRelation : IMapFrom<Domain.Entities.FriendsRelation>
{
    public Guid Id { get; set; }
    
    public Guid FriendOneId { get; set; }
    
    public Guid FriendTwoId { get; set; }
    
    public Guid GroupId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}