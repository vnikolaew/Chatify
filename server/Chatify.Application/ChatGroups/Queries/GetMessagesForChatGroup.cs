using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetMessagesForChatGroupResult = OneOf<UserIsNotMemberError, CursorPaged<ChatGroupMessageEntry>>;

public record GetMessagesForChatGroup(
    [Required] Guid GroupId,
    [Required] int PageSize,
    string? PagingCursor
) : IQuery<GetMessagesForChatGroupResult>;

[Timed]
internal sealed class GetMessagesByChatGroupHandler(
        IChatMessageRepository messages,
        IChatMessageReactionRepository reactions,
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IPagingCursorHelper pagingCursorHelper,
        IUserRepository users)
    : IQueryHandler<GetMessagesForChatGroup, GetMessagesForChatGroupResult>
{
    public async Task<GetMessagesForChatGroupResult> HandleAsync(
        GetMessagesForChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

        var pagingCursors = string.IsNullOrEmpty(command.PagingCursor)
            ? new List<string> { default!, default! }
            : pagingCursorHelper
                .ToPagingCursors(command.PagingCursor)
                .ToList();

        var messagesTask = messages.GetPaginatedByGroupAsync(
            command.GroupId,
            command.PageSize,
            pagingCursors[0],
            cancellationToken);
        var messageReplierInfosTask = messages.GetPaginatedReplierInfosByGroupAsync(
            command.GroupId,
            command.PageSize,
            pagingCursors[1],
            cancellationToken);


        var (groupMessages, messageReplySummaries) =
            await ( messagesTask, messageReplierInfosTask ).WhenAll();

        // Determine if user has reacted to each message:
        var userMessageReactions = await reactions.AllByUserAndMessageIds(
            identityContext.Id,
            groupMessages.Select(_ => _.Id),
            cancellationToken);

        var forwardedMessages = groupMessages
            .Where(m => m.ForwardedMessageId.HasValue)
            .ToList();

        var originMessages = new Dictionary<Guid, ChatMessage>();
        if ( forwardedMessages.Any() )
        {
            var forwardedMessagesIds = forwardedMessages
                .Select(m => m.ForwardedMessageId!.Value)
                .ToList();

            originMessages = ( await messages.GetByIds(forwardedMessagesIds, cancellationToken) )
                !.ToDictionary(m => m.Id);
        }

        var userInfos = ( await users
                .GetByIds(groupMessages.Select(m => m.UserId).ToHashSet(), cancellationToken) )!
            .ToImmutableDictionary(u => u.Id);

        var combinedCursor = pagingCursorHelper.CombineCursors(
            groupMessages.PagingCursor,
            messageReplySummaries.PagingCursor);

        return groupMessages.ZipOn(messageReplySummaries,
            m => m.Id,
            ri => ri.MessageId,
            (message,
                repliersSummary) =>
            {
                var user = userInfos[message.UserId];
                return new ChatGroupMessageEntry
                {
                    Message = message,
                    UserReaction = userMessageReactions.TryGetValue(message.Id, out var code) && code is not null
                        ? new UserMessageReaction(code.Value)
                        : null,
                    ForwardedMessage =
                        message.ForwardedMessageId.HasValue
                            ? originMessages[message.ForwardedMessageId.Value]
                            : default,
                    RepliersInfo = repliersSummary?.ToEntry() ??
                                   new MessageRepliersInfoEntry(0, null!, new List<MessageReplierInfoEntry>()),
                    SenderInfo = new MessageSenderInfoEntry(
                        user.Id,
                        user.Username,
                        user.ProfilePicture.MediaUrl)
                };
            }
        ).ToCursorPaged(combinedCursor, groupMessages.HasMore, groupMessages.Total)!;
    }
}