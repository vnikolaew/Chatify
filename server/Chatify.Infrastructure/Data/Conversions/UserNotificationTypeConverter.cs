using AutoMapper;
using Chatify.Domain.Entities;
using UserNotification = Chatify.Infrastructure.Data.Models.UserNotification;

namespace Chatify.Infrastructure.Data.Conversions;

public class UserNotificationTypeConverter
    : ITypeConverter<UserNotification, Domain.Entities.UserNotification>
{
    public Domain.Entities.UserNotification Convert(
        UserNotification source,
        Domain.Entities.UserNotification destination,
        ResolutionContext context)
    {
        return ( UserNotificationType )source.Type switch
        {
            UserNotificationType.IncomingFriendInvite =>
                context.Mapper.Map<IncomingFriendInvitationNotification>(source),
            _ => default!
        };
    }
}