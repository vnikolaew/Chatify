using System.Text.Json;
using AutoMapper;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Conversions;
using Humanizer;

namespace Chatify.Infrastructure.Data.Models;

using Metadata = Dictionary<string, string>;

public class UserNotification
    : IMapFrom<Domain.Entities.UserNotification>,
        IMapFrom<IncomingFriendInvitationNotification>
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
            .ConvertUsing<UserNotificationTypeConverter>();
        profile
            .CreateMap<Domain.Entities.UserNotification, UserNotification>();

        profile
            .CreateMap<Dictionary<string, string>?, UserNotificationMetadata?>()
            .ConvertUsing<UserMediaTypeConverter>();

        profile
            .CreateMap<UserNotificationMetadata?, Dictionary<string, string>?>()
            .ConvertUsing<UserMediaTypeConverter>();
    }

    void IMapFrom<IncomingFriendInvitationNotification>.Mapping(Profile profile)
        => profile
            .CreateMap<UserNotification, IncomingFriendInvitationNotification>()
            .IncludeBase<UserNotification, Domain.Entities.UserNotification>()
            .AfterMap((un, fi) =>
            {
                fi ??= new IncomingFriendInvitationNotification();
                fi.InviteId = Guid.Parse(un.Metadata!["invite_id"]);
            })
            .ReverseMap()
            .IncludeBase<Domain.Entities.UserNotification, UserNotification>()
            .AfterMap((fi, un) =>
            {
                un ??= new UserNotification();
                un.Metadata![nameof(IncomingFriendInvitationNotification.InviteId).Underscore()] =
                    fi.InviteId.ToString();
            });
}