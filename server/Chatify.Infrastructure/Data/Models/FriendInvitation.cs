using AutoMapper;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class FriendInvitation : IMapFrom<Domain.Entities.FriendInvitation>
{
    public Guid Id { get; set; }
    
    public Guid InviterId { get; set; }
    
    public Guid InviteeId { get; set; }

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public sbyte Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public bool Updated { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<FriendInvitation, Domain.Entities.FriendInvitation>()
            .ForMember(i => i.Status,
                cfg =>
                    cfg.MapFrom(i => ( FriendInvitationStatus )i.Status));
}