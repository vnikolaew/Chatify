using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Messages.Queries;

using GetChatGroupPinnedMessagesResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, List<ChatMessage>>;

[Cached("pinned-messages", 60 * 10)]
public record GetChatGroupPinnedMessages(
    [Required][property: CacheKey] Guid GroupId
) : IQuery<GetChatGroupPinnedMessagesResult>;

[Timed]
internal sealed class GetChatGroupPinnedMessagesHandler
    : IQueryHandler<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>
    
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IChatGroupRepository _groups;
    private readonly IChatMessageRepository _messages;
    private readonly IIdentityContext _identityContext;

    public GetChatGroupPinnedMessagesHandler(IChatGroupMemberRepository members, IChatGroupRepository groups,
        IIdentityContext identityContext, IChatMessageRepository messages)
    {
        _members = members;
        _groups = groups;
        _identityContext = identityContext;
        _messages = messages;
    }

    public async Task<GetChatGroupPinnedMessagesResult> HandleAsync(
        GetChatGroupPinnedMessages query,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(query.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isMember = await _members.Exists(group.Id, _identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(_identityContext.Id, group.Id);

        var pinnedMessageIds = JsonSerializer.Deserialize<HashSet<Guid>>(group.Metadata["pinned_message_ids"]);
        if ( pinnedMessageIds is null || !pinnedMessageIds.Any() ) return new List<ChatMessage>();

        return await _messages.GetByIds(pinnedMessageIds, cancellationToken) ?? new List<ChatMessage>();
    }
}