using System.Text.Json;
using AutoMapper;
using Chatify.Domain.Entities;
using Humanizer;

namespace Chatify.Infrastructure.Data.Conversions;

public class UserMediaTypeConverter
    : ITypeConverter<UserNotificationMetadata?, Dictionary<string, string>?>,
        ITypeConverter<Dictionary<string, string>?, UserNotificationMetadata?>
{
    public Dictionary<string, string> Convert(
        UserNotificationMetadata? source,
        Dictionary<string, string>? destination,
        ResolutionContext context)
    {
        destination ??= new Dictionary<string, string>();
        source ??= new UserNotificationMetadata();

        destination[nameof(UserNotificationMetadata.UserMedia).Underscore()] =
            JsonSerializer.Serialize(source.UserMedia);
        return destination;
    }

    public UserNotificationMetadata? Convert(
        Dictionary<string, string>? source,
        UserNotificationMetadata? destination,
        ResolutionContext context)
    {
        destination ??= new UserNotificationMetadata();
        destination.UserMedia = JsonSerializer.Deserialize<Media>(
                ( source ??= new Dictionary<string, string>() )
                [nameof(UserNotificationMetadata.UserMedia).Underscore()]);
        return destination;
    }
}