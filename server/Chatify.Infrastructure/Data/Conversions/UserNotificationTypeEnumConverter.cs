using AutoMapper;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Data.Conversions;

public class UserNotificationTypeEnumConverter
    : ITypeConverter<UserNotificationType, sbyte>,
        ITypeConverter<sbyte, UserNotificationType>
{
    public sbyte Convert(
        UserNotificationType source,
        sbyte destination,
        ResolutionContext context)
        => ( sbyte )source;

    public UserNotificationType Convert(sbyte source, UserNotificationType destination, ResolutionContext context)
        => ( UserNotificationType )source;
}