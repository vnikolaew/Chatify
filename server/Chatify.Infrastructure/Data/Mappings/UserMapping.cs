using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class UserMapping : Cassandra.Mapping.Mappings
{
    private const string UsersTableName = "users";

    public UserMapping()
        => For<ChatifyUser>()
            .TableName(UsersTableName)
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(u => u.Id)
            .Column(u => u.Metadata, cm => cm.WithDbType<Dictionary<string, string>>())
            .UnderscoreColumn(u => u.DisplayName)
            .UnderscoreColumn(u => u.Metadata)
            .UnderscoreColumn(u => u.Id)
            .UnderscoreColumn(u => u.Status, cm => cm.WithDbType<sbyte>())
            .UnderscoreColumn(u => u.PhoneNumbers)
            .UnderscoreColumn(u => u.ProfilePicture)
            .UnderscoreColumn(u => u.BannerPictures)
            .UnderscoreColumn(u => u.CreatedAt)
            .UnderscoreColumn(u => u.UpdatedAt)
            .UnderscoreColumn(u => u.LastLogin)
            .UnderscoreColumn(u => u.UserHandle, cm => cm.WithSecondaryIndex())
            .Column(u => u.DeviceIps, cm => cm.Ignore())
            .Column(u => u.RedisId, cm => cm.Ignore())
            .Column(u => u.RedisUsername, cm => cm.Ignore())
            .Column(u => u.DevicesIpAddresses, cm => cm.Ignore())
            .Column(u => u.UserName, m => m.WithName(nameof(ChatifyUser.UserName).ToLower()))
            .UnderscoreColumn(u => u.Email)
            .Column(u => u.DeviceIpsBytes,
                cm => cm.WithName(nameof(ChatifyUser.DeviceIps).Underscore()));
}