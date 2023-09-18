using AutoMapper;
using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Humanizer;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<long, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessage : IMapFrom<Domain.Entities.ChatMessage>
{
    [SecondaryIndex] public Guid Id { get; set; }

    public Guid ChatGroupId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = default!;

    protected HashSet<Media> _attachments = new();

    [FrozenKey]
    public IEnumerable<Media> Attachments
    {
        get => _attachments;
        set => _attachments = value.ToHashSet();
    }

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<long, long>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool Updated => UpdatedAt.HasValue;

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessage, Domain.Entities.ChatMessage>()
            .AfterMap((message,
                domainMessage) =>
            {
                domainMessage.ForwardedMessageId =
                    message.Metadata.TryGetValue(nameof(Domain.Entities.ChatMessage.ForwardedMessageId).Underscore(),
                        out var id) && Guid.TryParse(id, out var messageId)
                        ? messageId
                        : default;
            })
            .ReverseMap()
            .AfterMap((domainMessage,
                message) =>
            {
                if ( domainMessage.ForwardedMessageId.HasValue )
                {
                    message.Metadata[nameof(Domain.Entities.ChatMessage.ForwardedMessageId).Underscore()] =
                        domainMessage.ForwardedMessageId.Value.ToString();
                }
            });
}