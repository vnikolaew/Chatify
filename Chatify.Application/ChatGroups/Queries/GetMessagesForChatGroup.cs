using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.ChatGroups.Queries;

using GetMessagesForChatGroupResult = Either<Error, CursorPaged<ChatMessage>>;

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
        if (!isGroupMember) return Error.New("");

        var messages = await _messages.GetPaginatedByGroupAsync(
            command.GroupId, command.PageSize, command.PagingCursor, cancellationToken);
        return messages;
    }
}