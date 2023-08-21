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

internal sealed class ChatGroupMemberRemovedEventHandler(IDomainRepository<Domain.Entities.ChatGroup, Guid> groups,
        ILogger<ChatGroupMemberRemovedEventHandler> logger,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext,
        IDatabase cache)
    : IEventHandler<ChatGroupMemberRemovedEvent>
{
    private static RedisKey GetGroupMembersCacheKey(Guid groupId)
      => new($"groups:{groupId.ToString()}:members");

   public async Task HandleAsync(
      ChatGroupMemberRemovedEvent @event,
      CancellationToken cancellationToken = default)
   {
      var group = await groups.GetAsync(@event.GroupId, cancellationToken);
      if ( group is null ) return;

      var membersCount = await membersCounts.Decrement(group.Id, cancellationToken: cancellationToken);
      logger.LogInformation("Decremented Membership count for Chat Group with Id '{Id}' to {Count} ",
         @event.GroupId, membersCount?.MembersCount);

      // Remove new member to cache set as well:
      var groupKey = GetGroupMembersCacheKey(group.Id);
      var memberId = @event.MemberId.ToString();
      var userFeedCacheKey = $"user:{@event.MemberId}:feed";

      var cacheRemoveTasks = new[]
      {
          // Remove user from `group-members` set
         cache.SetRemoveAsync(groupKey, memberId),
         
          // Remove `group entry` user feed sorted set
         cache.SortedSetRemoveAsync(userFeedCacheKey, @event.GroupId.ToString())
      };
      
      var success = await Task.WhenAll(cacheRemoveTasks);
      if ( success.All(_ => _) )
      {
         logger.LogInformation(
            "Successfully pruned cache entries for User {UserId}",
            @event.MemberId);
      }

      await chatifyHubContext
         .Clients
         .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
         .ChatGroupMemberRemoved(new ChatGroupMemberRemoved(
            @event.GroupId,
            @event.RemovedById,
            @event.MemberId,
            identityContext.Username,
            @event.Timestamp));
   }
}