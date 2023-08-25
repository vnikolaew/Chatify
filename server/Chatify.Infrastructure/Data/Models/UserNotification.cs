using AutoMapper;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Conversions;
using Chatify.Infrastructure.Data.Extensions;
using Humanizer;

namespace Chatify.Infrastructure.Data.Models;

using Metadata = Dictionary<string, string>;

public class UserNotification
    : IMapFrom<Domain.Entities.UserNotification>,
        IMapFrom<IncomingFriendInvitationNotification>,
        IMapFrom<AcceptedFriendInvitationNotification>,
        IMapFrom<DeclinedFriendInvitationNotification>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public sbyte Type { get; set; }

    public Metadata? Metadata { get; set; }

    public string? Summary { get; set; }

    public bool Read { get; set; }

    void IMapFrom<Domain.Entities.UserNotification>.Mapping(Profile profile)
    {
        profile
            .CreateMap<UserNotification, Domain.Entities.UserNotification>()
            .ConvertUsing(new UserNotificationTypeConverter());
        profile
            .CreateMap<Domain.Entities.UserNotification, UserNotification>();

        profile
            .CreateFor<UserNotificationType, sbyte, UserNotificationTypeEnumConverter>()
            .CreateFor<Dictionary<string, string>?, UserNotificationMetadata?, UserMediaTypeConverter>();
    }

    void IMapFrom<IncomingFriendInvitationNotification>.Mapping(Profile profile)
        => profile
            .CreateMap<UserNotification, IncomingFriendInvitationNotification>()
            .IncludeBase<UserNotification, Domain.Entities.UserNotification>()
            .AfterMap((un, fi) =>
            {
                fi.InviteId = un.Metadata!.TryGetValue("invite_id", out var inviteId)
                    ? Guid.Parse(inviteId)
                    : default;
                fi.Type = UserNotificationType.IncomingFriendInvite;
            })
            .ReverseMap()
            .IncludeBase<Domain.Entities.UserNotification, UserNotification>()
            .AfterMap((fi, un) =>
            {
                un ??= new UserNotification();
                un.Metadata![nameof(IncomingFriendInvitationNotification.InviteId).Underscore()] =
                    fi.InviteId.ToString();
            });

    void IMapFrom<AcceptedFriendInvitationNotification>.Mapping(Profile profile)
        => profile
            .CreateMap<UserNotification, AcceptedFriendInvitationNotification>()
            .IncludeBase<UserNotification, Domain.Entities.UserNotification>()
            .AfterMap((un, fi) =>
            {
                fi.InviteId = un.Metadata!.TryGetValue(nameof(AcceptedFriendInvitationNotification.InviteId).Underscore(), out var inviteId)
                    ? Guid.Parse(inviteId)
                    : default;
                fi.InviterId = un.Metadata!.TryGetValue(nameof(AcceptedFriendInvitationNotification.InviterId).Underscore(), out var inviterId)
                    ? Guid.Parse(inviterId)
                    : default;
                fi.ChatGroupId = un.Metadata!.TryGetValue(nameof(AcceptedFriendInvitationNotification.ChatGroupId).Underscore(), out var groupId)
                    ? Guid.Parse(groupId)
                    : default;
            })
            .ReverseMap()
            .IncludeBase<Domain.Entities.UserNotification, UserNotification>()
            .AfterMap((fi, un) =>
            {
                un ??= new UserNotification();
                un.Metadata![nameof(AcceptedFriendInvitationNotification.InviteId).Underscore()] =
                    fi.InviteId.ToString();
                un.Metadata![nameof(AcceptedFriendInvitationNotification.InviterId).Underscore()] =
                    fi.InviterId.ToString();
                un.Metadata![nameof(AcceptedFriendInvitationNotification.ChatGroupId).Underscore()] =
                    fi.ChatGroupId.ToString();
            });

    void IMapFrom<DeclinedFriendInvitationNotification>.Mapping(Profile profile)
        => profile
            .CreateMap<UserNotification, DeclinedFriendInvitationNotification>()
            .IncludeBase<UserNotification, AcceptedFriendInvitationNotification>()
            .ReverseMap()
            .IncludeBase<AcceptedFriendInvitationNotification, UserNotification>();
}