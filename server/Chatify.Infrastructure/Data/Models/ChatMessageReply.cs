using AutoMapper;
using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReply : ChatMessage, IMapFrom<Domain.Entities.ChatMessageReply>
{
    public Guid ReplyToId { get; set; }
    
    public long RepliesCount { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessageReply, Domain.Entities.ChatMessageReply>()
            .ReverseMap();
}