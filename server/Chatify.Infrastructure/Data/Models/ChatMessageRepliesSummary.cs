using AutoMapper;
using Cassandra;
using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Common.Mappings;
using Humanizer;

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

    private static DateTime MapDateTime(DateTimeOffset? dateTimeOffset)
        => dateTimeOffset?.Date ?? default;

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatMessageRepliesSummary, MessageRepliersInfo>()
            .ForMember(ri => ri.LastUpdatedAt,
                cfg =>
                    cfg.MapFrom(rs => MapDateTime(rs.UpdatedAt)))
            .ReverseMap();
}

public class MessageReplierInfo : IMapFrom<Chatify.Domain.Entities.MessageReplierInfo>
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;

    public string ProfilePictureUrl { get; set; } = default!;

    public void Mapping(Profile profile)
        => profile
            .CreateMap<MessageReplierInfo, Domain.Entities.MessageReplierInfo>()
            .MapRecordMember(i => i.Username, i => i.Username)
            .MapRecordMember(i => i.UserId, i => i.UserId)
            .MapRecordMember(i => i.ProfilePictureUrl, i => i.ProfilePictureUrl)
            .ReverseMap();

    public static readonly UdtMap<MessageReplierInfo> UdtMap = Cassandra.UdtMap
        .For<MessageReplierInfo>(nameof(MessageReplierInfo).Underscore())
        .Map(ri => ri.UserId, nameof(UserId).Underscore())
        .Map(ri => ri.Username, nameof(Username).Underscore())
        .Map(ri => ri.ProfilePictureUrl, nameof(ProfilePictureUrl).Underscore());
};