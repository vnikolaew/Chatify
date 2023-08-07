using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetMessagesForChatGroupResult = OneOf<UserIsNotMemberError, CursorPaged<ChatGroupMessageResponseModel>>;

public record GetMessagesForChatGroup(
    [Required] Guid GroupId,
    [Required] int PageSize,
    [Required] string PagingCursor
) : IQuery<GetMessagesForChatGroupResult>;

internal sealed class GetMessagesByChatGroupHandler
    : IQueryHandler<GetMessagesForChatGroup, GetMessagesForChatGroupResult>
{
    private readonly IChatMessageRepository _messages;
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupMemberRepository _members;

    public GetMessagesByChatGroupHandler(
        IChatMessageRepository messages,
        IIdentityContext identityContext,
        IChatGroupMemberRepository members)
    {
        _messages = messages;
        _identityContext = identityContext;
        _members = members;
    }

    public async Task<GetMessagesForChatGroupResult> HandleAsync(
        GetMessagesForChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        var messageReplySummaries = await _messages.GetPaginatedReplierInfosByGroupAsync(
            command.GroupId,
            command.PageSize,
            command.PagingCursor, cancellationToken);
        var messages = await _messages.GetPaginatedByGroupAsync(
            command.GroupId,
            command.PageSize,
            command.PagingCursor,
            cancellationToken);

        return messages
            .Zip(messageReplySummaries, (message, replySummary) => new ChatGroupMessageResponseModel(
                message,
                new MessageRepliersInfoResponseModel(
                    replySummary.Total,
                    replySummary.LastUpdatedAt,
                    replySummary.ReplierInfos.Select(ri =>
                        new MessageReplierInfoResponseModel(ri.UserId, ri.Username, ri.ProfilePictureUrl)).ToList()))
            )
            .ToCursorPaged(messages.PagingCursor);
    }
}