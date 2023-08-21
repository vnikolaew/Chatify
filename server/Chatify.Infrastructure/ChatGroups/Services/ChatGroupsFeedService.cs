using Bogus;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
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
            .RuleFor(e => e.ChatMessage, f => new ChatMessage
            {
                Id = Guid.NewGuid(),
                Content = f.Lorem.Sentence(8),
                CreatedAt = f.Date.Past(1),
            })
            .RuleFor(e => e.User, f => new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                CreatedAt = f.Date.Past(1),
                Username = f.Internet.UserName(),
                Status = f.PickRandom<UserStatus>(),
                Email = f.Internet.Email(),
                ProfilePicture = new Media { Id = Guid.NewGuid(), MediaUrl = f.Internet.Avatar() }
            })
            .RuleFor(e => e.ChatGroup, (f, e) => new ChatGroup
            {
                Id = Guid.NewGuid(),
                CreatedAt = f.Date.Past(1),
                Name = f.Internet.DomainName(),
                About = f.Lorem.Sentence(10),
                Picture = new Media { Id = Guid.NewGuid(), MediaUrl = f.Internet.Avatar() },
                AdminIds = new HashSet<Guid> { e.User.Id },
            });

    private static RedisKey GetUserFeedCacheKey(Guid userId)
        => new($"user:{userId}:feed");

    public async Task<List<ChatGroupFeedEntry>> GetFeedEntriesForUserAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        // Get feed with group ids from cache sorted set:
        var userFeedCacheKey = GetUserFeedCacheKey(userId);
        var values = await cache.SortedSetRangeByRankAsync(
            userFeedCacheKey,
            offset, limit + offset, Order.Descending);
        if ( !values.Any() ) return FeedEntryFaker.Generate(10);

        var groupIds = values
            .Select(v => Guid.Parse(v.ToString()))
            .ToList();

        var feedGroups = await groups
            .GetByIds(groupIds, cancellationToken);

        // Query DB to get last message for each Chat Group:
        var feedMessages = ( await messages
                .GetLatestForGroups(groupIds, cancellationToken) )
            .Values
            .OrderByDescending(_ => _.CreatedAt)
            .ToList();

        // Query cache for Message Sender info:
        var messageSenderIds = feedMessages
            .Select(m => m.UserId)
            .Distinct()
            .ToList();
        var userInfos = await users.GetByIds(messageSenderIds, cancellationToken);

        return feedMessages
            .ZipOn(
                feedGroups,
                m => m.ChatGroupId,
                g => g.Id,
                (message, group) => new ChatGroupFeedEntry
                {
                    ChatMessage = message,
                    ChatGroup = group
                })
            .ZipOn(
                userInfos!,
                e => e.ChatMessage.UserId,
                u => u.Id,
                (e, user) => e with { User = user })
            .ToList();
    }
}