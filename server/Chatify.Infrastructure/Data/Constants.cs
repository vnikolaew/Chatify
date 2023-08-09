using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data;

public static class Constants
{
    public const string KeyspaceName = "chatify";
    
    public static readonly Media DefaultUserProfilePicture = new()
    {
        MediaUrl = "default-user-profile-picture.png",
        Id = Guid.NewGuid(),
        FileName = "default-user-profile-picture.png",
        Type = "png"
    };
}