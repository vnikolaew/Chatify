﻿using AutoMapper;
using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageRepliesSummary : IMapFrom<MessageRepliersInfo>
{
    public Guid ChatGroupId { get; set; }

    public Guid Id { get; set; }

    [SecondaryIndex] public Guid MessageId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool Updated => UpdatedAt.HasValue;

    public long Total { get; set; }

    private readonly ISet<Guid> _replierIds = new HashSet<Guid>();

    public HashSet<Guid> ReplierIds
    {
        get => _replierIds.ToHashSet();
        init => _replierIds = value;
    }


    private readonly ISet<MessageReplierInfo> _replierInfos = new HashSet<MessageReplierInfo>();

    public HashSet<MessageReplierInfo> ReplierInfos
    {
        get => _replierInfos.ToHashSet();
        init => _replierInfos = value;
    }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessageRepliesSummary, MessageRepliersInfo>()
            .ForMember(ri => ri.LastUpdatedAt,
                cfg =>
                    cfg.MapFrom(rs => rs.UpdatedAt));

}

public class MessageReplierInfo : IMapFrom<Chatify.Domain.Entities.MessageReplierInfo>
{
    public Guid UserId { get; set; }

    public string Username { get; set; }

    public string ProfilePictureUrl { get; set; }
};