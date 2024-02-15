using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Messages.Queries;

using GetChatGroupPinnedMessagesResult =
    OneOf<ChatGroupNotFoundError, ChatGroups.Commands.UserIsNotMemberError, List<ChatMessage>>;

[Cached("pinned-messages", 60 * 10)]
public record GetChatGroupPinnedMessages(
    [Required] [property: CacheKey] Guid GroupId
) : IQuery<GetChatGroupPinnedMessagesResult>;

[Timed]
internal sealed class GetChatGroupPinnedMessagesHandler(
    IChatGroupMemberRepository members,
    IChatGroupRepository groups,
    IIdentityContext identityContext,
    IChatMessageRepository messages)
    : IQueryHandler<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>
{
    public async Task<GetChatGroupPinnedMessagesResult> HandleAsync(GetChatGroupPinnedMessages query,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(query.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(group.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new ChatGroups.Commands.UserIsNotMemberError(identityContext.Id, group.Id);

        if ( !group.PinnedMessages.Any() ) return new List<ChatMessage>();
        var pinnedMessages = await messages
                                 .GetByIds(group.PinnedMessages.Select(_ => _.MessageId),
                                     cancellationToken)
                             ?? [];

        return pinnedMessages;
    }
}