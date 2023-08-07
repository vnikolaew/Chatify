﻿using Cassandra.Mapping;
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
            .ClusteringKey(
                new Tuple<string, SortOrder>(
                    nameof(ChatifyUser.CreatedAt).Underscore(), SortOrder.Descending),
                new Tuple<string, SortOrder>(
                    nameof(ChatifyUser.UserName), SortOrder.Ascending))
            .Column(u => u.Metadata, cm => cm.WithDbType<Dictionary<string, string>>())
            .UnderscoreColumn(u => u.DisplayName)
            .UnderscoreColumn(u => u.Metadata)
            .UnderscoreColumn(u => u.Status)
            .UnderscoreColumn(u => u.PhoneNumbers)
            .UnderscoreColumn(u => u.ProfilePictureUrl)
            .UnderscoreColumn(u => u.BannerPictures)
            .UnderscoreColumn(u => u.CreatedAt)
            .UnderscoreColumn(u => u.UpdatedAt)
            .UnderscoreColumn(u => u.LastLogin)
            .Column(u => u.UserName, m => m.WithName(nameof(ChatifyUser.UserName).ToLower()))
            .UnderscoreColumn(u => u.Email)
            .UnderscoreColumn(u => u.DeviceIps);
}