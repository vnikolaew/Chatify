using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Messages.Replies.Queries;

using GetRepliesForMessageResult = OneOf<MessageNotFoundError, UserIsNotMemberError, CursorPaged<ChatMessageReply>>;


[Cached("message-replies", 10)]
public record GetRepliesForMessage(
    [Required] [property: CacheKey] Guid MessageId,
    [Required] int PageSize,
    [Required] string PagingCursor
) : IQuery<GetRepliesForMessageResult>;

internal sealed class GetRepliesByForMessageHandler(IChatMessageReplyRepository messageReplies,
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IChatMessageRepository messages)
    : IQueryHandler<GetRepliesForMessage, GetRepliesForMessageResult>
{
    public async Task<GetRepliesForMessageResult> HandleAsync(GetRepliesForMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(
            command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var isGroupMember = await members.Exists(
            message.ChatGroupId,
            identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(identityContext.Id, message.ChatGroupId);

        var replies = await messageReplies.GetPaginatedByMessageAsync(
            command.MessageId, command.PageSize, command.PagingCursor, cancellationToken);
        
        return replies;
    }
}