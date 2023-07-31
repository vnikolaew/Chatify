using AutoMapper;
using Cassandra;
using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroup : IMapFrom<Domain.Entities.ChatGroup>
{
    public Guid Id { get; set; }

    public Guid CreatorId { get; set; }

    public string Name { get; set; } = default!;

    public string About { get; set; } = default!;

    public string PictureUrl { get; set; } = default!;

    public ISet<Guid> AdminIds { get; set; } = new HashSet<Guid>();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatGroup, Domain.Entities.ChatGroup>()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()))
            .ReverseMap()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()));
}