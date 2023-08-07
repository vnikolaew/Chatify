using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Messages.Replies.Queries;

using GetMessagesForChatGroupResult = OneOf<MessageNotFoundError, UserIsNotMemberError, CursorPaged<ChatMessageReply>>;

public record MessageNotFoundError(Guid MessageId);

[Cached("message-replies", 10)]
public record GetRepliesByForMessage(
    [Required] [property: CacheKey] Guid MessageId,
    [Required] int PageSize,
    [Required] string PagingCursor
) : IQuery<GetMessagesForChatGroupResult>;

internal sealed class GetRepliesByForMessageHandler
    : IQueryHandler<GetRepliesByForMessage, GetMessagesForChatGroupResult>
{
    private readonly IChatMessageReplyRepository _messageReplies;
    private readonly IChatMessageRepository _messages;
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupMemberRepository _members;

    public GetRepliesByForMessageHandler(
        IChatMessageReplyRepository messageReplies,
        IIdentityContext identityContext,
        IChatGroupMemberRepository members, IChatMessageRepository messages)
    {
        _messageReplies = messageReplies;
        _identityContext = identityContext;
        _members = members;
        _messages = messages;
    }

    public async Task<GetMessagesForChatGroupResult> HandleAsync(
        GetRepliesByForMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(
            command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var isGroupMember = await _members.Exists(
            message.ChatGroupId,
            _identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(_identityContext.Id, message.ChatGroupId);

        var messages = await _messageReplies.GetPaginatedByMessageAsync(
            command.MessageId, command.PageSize, command.PagingCursor, cancellationToken);
        
        return messages;
    }
}