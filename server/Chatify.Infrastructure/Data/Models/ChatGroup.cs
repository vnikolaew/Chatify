using AutoMapper;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Extensions;
using Humanizer;
using Redis.OM.Modeling;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

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

    [Indexed] public ISet<Guid> AdminIds { get; set; } = new HashSet<Guid>();

    [Indexed] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();

    [FrozenValue]
    public IDictionary<Guid, MessagePin> PinnedMessages { get; set; } = new Dictionary<Guid, MessagePin>();

    public DateTimeOffset? UpdatedAt { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatGroup, Domain.Entities.ChatGroup>()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()))
            .ForMember(g => g.PinnedMessages,
                cfg => cfg.MapFrom(
                    cg => cg.PinnedMessages.Values
                        .Select(m => new PinnedMessage(m.MessageId, m.CreatedAt.DateTime, m.PinnerId))
                        .ToHashSet()
                ))
            .ReverseMap()
            .ForMember(g => g.AdminIds,
                cfg => cfg.MapFrom(g => g.AdminIds.ToHashSet()))
            .ForMember(g => g.PinnedMessages,
                cfg => cfg.MapFrom(
                    cg => cg.PinnedMessages.ToDictionary(m => m.MessageId)));
}

public class MessagePin : IMapFrom<PinnedMessage>
{
    public Guid MessageId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid PinnerId { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<MessagePin, PinnedMessage>()
            .ReverseMap();

    public static readonly UdtMap<MessagePin> UdtMap = Cassandra.UdtMap
        .For<MessagePin>(nameof(MessagePin).Underscore())
        .UnderscoreColumn(p => p.MessageId)
        .UnderscoreColumn(p => p.CreatedAt)
        .UnderscoreColumn(p => p.PinnerId);
}