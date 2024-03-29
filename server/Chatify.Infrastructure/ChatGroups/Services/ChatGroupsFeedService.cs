﻿using Bogus;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Shared.Infrastructure.Common.Extensions;
using StackExchange.Redis;

namespace Chatify.Infrastructure.ChatGroups.Services;

internal sealed class ChatGroupsFeedService(
    IDatabase cache,
    IChatMessageRepository messages,
    IChatGroupRepository groups,
    IUserRepository users)
    : IChatGroupsFeedService
{
    private static readonly Faker<ChatGroupFeedEntry> FeedEntryFaker
        = new Faker<ChatGroupFeedEntry>()
            .RuleFor(e => e.LatestMessage, f => new ChatMessage
            {
                Id = Guid.NewGuid(),
                Content = f.Lorem.Sentence(8),
                CreatedAt = f.Date.Past(1),
            })
            .RuleFor(e => e.MessageSender, f => new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                CreatedAt = f.Date.Past(1),
                Username = f.Internet.UserName(),
                Status = f.PickRandom<UserStatus>(),
                Email = f.Internet.Email(),
                ProfilePicture = new Media { Id = Guid.NewGuid(), MediaUrl = f.Internet.Avatar() }
            })
            .RuleFor(e => e.ChatGroup, (f,
                e) => new ChatGroup
            {
                Id = Guid.NewGuid(),
                CreatedAt = f.Date.Past(1),
                Name = f.Internet.DomainName(),
                About = f.Lorem.Sentence(10),
                Picture = new Media { Id = Guid.NewGuid(), MediaUrl = f.Internet.Avatar() },
                AdminIds = new HashSet<Guid> { e.MessageSender.Id },
            });

    public async Task<List<ChatGroupFeedEntry>> GetFeedEntriesForUserAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        // Get feed with group ids from cache sorted set:
        var groupIds = await cache.GetUserFeedAsync(
            userId,
            offset, limit);
        
        if ( !groupIds.Any() ) return FeedEntryFaker.Generate(10);
        return await GetFeedByGroups(groupIds.ToList(), cancellationToken);
    }

    public async Task<List<ChatGroupFeedEntry>> GetStarredFeedEntriesForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(userId, cancellationToken);
        if ( user is null ) return [];

        return await GetFeedByGroups(user.StarredChatGroups.ToList(), cancellationToken);
    }

    private async Task<List<ChatGroupFeedEntry>> GetFeedByGroups(List<Guid> groupIds, CancellationToken cancellationToken)
    {
        var (feedGroups, feedMessagesDict) = await (
            groups.GetByIds(groupIds, cancellationToken),
            messages.GetLatestForGroups(groupIds, cancellationToken)
        );

        // Query DB to get last message for each Chat Group:
        var feedMessages = feedMessagesDict.Values.ToList();

        // Query cache for Message Sender info:
        var messageSenderIds = feedMessages
            .Where(m => m is not null)
            .Select(m => m!.UserId)
            .Distinct()
            .ToList();

        var userInfos = await users.GetByIds(messageSenderIds, cancellationToken);

        var entries = feedGroups
            .ZipOn(feedMessages,
                r => r!.Id,
                m => m!.ChatGroupId,
                (group,
                    message) => new ChatGroupFeedEntry { ChatGroup = group!, LatestMessage = message! })
            .Cast<ChatGroupFeedEntry>()
            .ToList();

        return entries.ZipOn(userInfos!,
                e => e!.LatestMessage?.UserId,
                u => u.Id,
                (e, user) => e! with { MessageSender = user })
            .OrderByDescending(e => e!.LatestMessage?.CreatedAt ?? e.ChatGroup.CreatedAt)
            .ToList()!;
    }
}