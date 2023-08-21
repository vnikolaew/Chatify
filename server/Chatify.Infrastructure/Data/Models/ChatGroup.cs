using AutoMapper;
using Chatify.Application.Common.Mappings;
using Redis.OM.Modeling;

namespace Chatify.Infrastructure.Data.Models;

[Document(
    StorageType = StorageType.Json,
    IndexName = "chat_groups",
    Prefixes = new[] { nameof(ChatGroup) })]
public class ChatGroup : IMapFrom<Domain.Entities.ChatGroup>
{
    [RedisIdField] [Indexed] public Guid Id { get; set; }

    [Indexed] public Guid CreatorId { get; set; }

    [Searchable] public string Name { get; set; } = default!;

    [Indexed] public string About { get; set; } = default!;

    [Indexed(CascadeDepth = 1)] public Media Picture { get; set; } = default!;

    [Indexed]
    public ISet<Guid> AdminIds { get; set; } = new HashSet<Guid>();

    [Indexed] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatGroup, Domain.Entities.ChatGroup>()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()))
            .ReverseMap()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()));
}