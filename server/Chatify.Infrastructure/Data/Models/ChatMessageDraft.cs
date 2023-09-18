using AutoMapper;
using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageDraft : ChatMessage, IMapFrom<Domain.Entities.ChatMessageDraft>
{
    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessageDraft, Domain.Entities.ChatMessageDraft>()
            .IncludeBase<ChatMessage, Domain.Entities.ChatMessage>()
            .ReverseMap()
            .IncludeBase<Domain.Entities.ChatMessage, ChatMessage>();
}