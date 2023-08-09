using System.Text.RegularExpressions;
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed partial class ChatMessageReplyDeletedEventHandler
    : IEventHandler<ChatMessageReplyDeletedEvent>
{
    private readonly ICounterService<ChatMessageReplyCount, Guid> _replyCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyContext;
    private readonly IMapper _mapper;
    private readonly IIdentityContext _identityContext;

    public ChatMessageReplyDeletedEventHandler(
        ICounterService<ChatMessageReplyCount, Guid> replyCounts,
        IIdentityContext identityContext,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext, IMapper mapper)
    {
        _replyCounts = replyCounts;
        _identityContext = identityContext;
        _chatifyContext = chatifyContext;
        _mapper = mapper;
    }

    public async Task HandleAsync(ChatMessageReplyDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        await _replyCounts.Decrement(
            @event.ReplyToId,
            cancellationToken: cancellationToken);

        // Update Message Reply Summaries "View" table:
        var messageReplierIds = await _mapper.FetchAsync<Guid>(
            "SELECT user_id FROM chat_message_replies WHERE reply_to = ?;", @event.ReplyToId);

        // We need to remove replier Id since there's no more replies by him:
        if ( messageReplierIds.Count(id => id == @event.UserId) <= 1 )
        {
            // Delete UserInfo object from HashSet
            var userInfo = await _mapper.FirstOrDefaultAsync<ChatifyUser>(
                "SELECT id, username, profile_picture_url FROM users WHERE id = ?;", @event.UserId);

            var userInfoDict = new Dictionary<string, string>
            {
                { "user_id", userInfo.Id.ToString() },
                { "username", userInfo.UserName },
                { "profile_picture_url", userInfo.ProfilePicture.MediaUrl }
            };

            var userInfoDictString = GuidRegex()
                .Replace(
                    JsonSerializer.Serialize(userInfoDict),
                    match => match.Groups[0].Value.Replace("\"", ""));

            await _mapper.UpdateAsync<ChatMessageRepliesSummary>(
                $" SET replier_ids = replier_ids - ?, total = total - 1, user_infos = user_infos - {userInfoDictString} WHERE message_id = ? ALLOW FILTERING;",
                @event.UserId,
                @event.MessageId);
        }
        else
        {
            await _mapper.UpdateAsync<ChatMessageRepliesSummary>(
                " SET total = total - 1 WHERE message_id = ? ALLOW FILTERING;",
                @event.MessageId);
        }

        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageRemoved(
                new ChatGroupMessageRemoved(
                    @event.GroupId,
                    @event.MessageId,
                    @event.UserId,
                    _identityContext.Username,
                    @event.Timestamp));
    }

    [GeneratedRegex("\"\\b[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}\\b\"")]
    private static partial Regex GuidRegex();
}