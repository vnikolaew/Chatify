using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Common.Contracts;
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
    [Required] string PagingCursor
) : IQuery<GetMessagesForChatGroupResult>;

[Timed]
internal sealed class GetMessagesByChatGroupHandler
    : IQueryHandler<GetMessagesForChatGroup, GetMessagesForChatGroupResult>
{
    private readonly IChatMessageRepository _messages;
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupMemberRepository _members;
    private readonly IUserRepository _users;
    private readonly IPagingCursorHelper _pagingCursorHelper;

    public GetMessagesByChatGroupHandler(
        IChatMessageRepository messages,
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IPagingCursorHelper pagingCursorHelper,
        IUserRepository users)
    {
        _messages = messages;
        _identityContext = identityContext;
        _members = members;
        _pagingCursorHelper = pagingCursorHelper;
        _users = users;
    }

    public async Task<GetMessagesForChatGroupResult> HandleAsync(
        GetMessagesForChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        var pagingCursors = _pagingCursorHelper
            .ToPagingCursors(command.PagingCursor)
            .ToList();

        var messagesTask = _messages.GetPaginatedByGroupAsync(
            command.GroupId,
            command.PageSize,
            pagingCursors[0],
            cancellationToken);
        var messageReplierInfosTask = _messages.GetPaginatedReplierInfosByGroupAsync(
            command.GroupId,
            command.PageSize,
            pagingCursors[1],
            cancellationToken);

        var (messages, messageReplySummaries) =
            await ( messagesTask, messageReplierInfosTask ).WhenAll();

        var userInfos = ( await _users
                .GetByIds(messages.Select(m => m.UserId).ToHashSet(), cancellationToken) )!
            .ToImmutableDictionary(u => u.Id);

        var combinedCursor = _pagingCursorHelper.CombineCursors(
            messages.PagingCursor,
            messageReplySummaries.PagingCursor);

        return messages
            .Zip(messageReplySummaries,
                (message, repliersSummary) =>
                {
                    var user = userInfos[message.UserId];
                    return new ChatGroupMessageEntry
                    {
                        Message = message,
                        RepliersInfo = new MessageRepliersInfoEntry(
                            repliersSummary.Total,
                            repliersSummary.LastUpdatedAt,
                            repliersSummary.ReplierInfos.Select(
                                    ri => new MessageReplierInfoEntry(ri.UserId, ri.Username, ri.ProfilePictureUrl))
                                .ToList()),
                        SenderInfo = new MessageReplierInfoEntry(
                            user.Id,
                            user.Username,
                            user.ProfilePicture.MediaUrl)
                    };
                })
            .ToCursorPaged(combinedCursor);
    }
}