using AutoMapper;
using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<long, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReaction : IMapFrom<Domain.Entities.ChatMessageReaction>
{
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;
    
    public long ReactionCode { get; set; }
    
    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<long, long>();
    public bool Updated { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessageReaction, Domain.Entities.ChatMessageReaction>()
            .ReverseMap();
}