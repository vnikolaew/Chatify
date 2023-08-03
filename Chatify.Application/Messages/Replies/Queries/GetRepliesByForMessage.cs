using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Replies.Queries;

using GetMessagesForChatGroupResult = Either<Error, CursorPaged<ChatMessageReply>>;

public record GetRepliesByForMessage(
    [Required] Guid MessageId,
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
        if (message is null) return Error.New("Chat message does not exist.");

        var isGroupMember = await _members.Exists(
            message.ChatGroupId,
            _identityContext.Id, cancellationToken);
        if (!isGroupMember) return Error.New("");

        var messages = await _messageReplies.GetPaginatedByMessageAsync(
            command.MessageId, command.PageSize, command.PagingCursor, cancellationToken);
        
        return messages;
    }
}