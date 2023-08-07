using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberRemovedEventHandler
   : IEventHandler<ChatGroupMemberRemovedEvent>
{
   private readonly IDomainRepository<Domain.Entities.ChatGroup, Guid> _groups;
   private readonly ILogger<ChatGroupMemberRemovedEventHandler> _logger;
   private readonly ICounterService<ChatGroupMembersCount, Guid> _membersCounts;
   private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
   private readonly IIdentityContext _identityContext;
   private readonly IDatabase _cache;

   public ChatGroupMemberRemovedEventHandler(
      IDomainRepository<Domain.Entities.ChatGroup, Guid> groups,
      ILogger<ChatGroupMemberRemovedEventHandler> logger,
      ICounterService<ChatGroupMembersCount, Guid> membersCounts,
      IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
      IIdentityContext identityContext, IDatabase cache)
   {
      _groups = groups;
      _logger = logger;
      _membersCounts = membersCounts;
      _chatifyHubContext = chatifyHubContext;
      _identityContext = identityContext;
      _cache = cache;
   }

   private static RedisKey GetGroupMembersCacheKey(Guid groupId)
      => new($"groups:{groupId.ToString()}:members");

   public async Task HandleAsync(
      ChatGroupMemberRemovedEvent @event,
      CancellationToken cancellationToken = default)
   {
      var group = await _groups.GetAsync(@event.GroupId, cancellationToken);
      if ( group is null ) return;

      var membersCount = await _membersCounts.Decrement(group.Id, cancellationToken: cancellationToken);
      _logger.LogInformation("Decremented Membership count for Chat Group with Id '{Id}' to {Count} ",
         @event.GroupId, membersCount?.MembersCount);

      // Remove new member to cache set as well:
      var groupKey = GetGroupMembersCacheKey(group.Id);
      var userKey = new RedisValue(@event.MemberId.ToString());
      var userFeedCacheKey = new RedisKey($"user:{@event.MemberId}:feed");

      var cacheRemoveTasks = new[]
      {
         _cache.SetRemoveAsync(groupKey, userKey),
         _cache.KeyDeleteAsync(userFeedCacheKey)
      };
      var success = await Task.WhenAll(cacheRemoveTasks);
      if ( success.All(_ => _) )
      {
         _logger.LogInformation(
            "Successfully pruned cache entries for User {UserId}",
            @event.MemberId);
      }

      var groupId = $"chat-groups:{@event.GroupId}";
      await _chatifyHubContext
         .Clients
         .Group(groupId)
         .ChatGroupMemberRemoved(new ChatGroupMemberRemoved(
            @event.GroupId,
            @event.RemovedById,
            @event.MemberId,
            _identityContext.Username,
            @event.Timestamp));
   }
}