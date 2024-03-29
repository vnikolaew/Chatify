﻿using AutoMapper;
using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupAttachment : IMapFrom<Domain.Entities.ChatGroupAttachment>
{
    public Guid ChatGroupId { get; set; }

    public Guid AttachmentId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;

    public Media MediaInfo { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatGroupAttachment, Domain.Entities.ChatGroupAttachment>()
            .ReverseMap();
}