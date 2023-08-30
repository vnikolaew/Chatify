using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Messages.Reactions.Queries;

using GetAllForMessageResult = OneOf<Error, UserIsNotMemberError, List<ChatMessageReaction>>;

public record GetAllReactionsForMessage(
    [Required] Guid MessageId
) : IQuery<GetAllForMessageResult>;

internal sealed class GetAllForMessageHandler(
        IChatMessageRepository messages,
        IChatMessageReactionRepository reactions,
        IChatGroupMemberRepository members,
        IIdentityContext identityContext)
    : IQueryHandler<GetAllReactionsForMessage, GetAllForMessageResult>
{
    public async Task<GetAllForMessageResult> HandleAsync(GetAllReactionsForMessage query,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(query.MessageId, cancellationToken);
        if ( message is null ) return Error.New("");

        var isMember = await members.Exists(message.ChatGroupId, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, message.ChatGroupId);

        var messageReactions = await reactions.AllForMessage(query.MessageId, cancellationToken);
        return messageReactions ?? new List<ChatMessageReaction>();
    }
}