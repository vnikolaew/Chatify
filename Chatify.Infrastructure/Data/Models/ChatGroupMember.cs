using AutoMapper;
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

    public void Mapping(Profile profile)
        => profile.CreateMap<ChatGroupMember, Domain.Entities.ChatGroupMember>()
            .ReverseMap()
            .ForMember(m => m.UserId,
                cfg => cfg.MapFrom(m => m.User == null ? m.UserId : m.User.Id))
            .ForMember(m => m.ChatGroupId,
                cfg => cfg.MapFrom(m => m.ChatGroup == null ? m.ChatGroupId : m.ChatGroup.Id));
}