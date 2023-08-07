using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetMessagesForChatGroupResult = OneOf<UserIsNotMemberError, CursorPaged<ChatMessage>>;

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
        var s = DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-UK"));
            
        var isGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        var messages = await _messages.GetPaginatedByGroupAsync(
            command.GroupId,
            command.PageSize,
            command.PagingCursor,
            cancellationToken);
        return messages;
    }
}