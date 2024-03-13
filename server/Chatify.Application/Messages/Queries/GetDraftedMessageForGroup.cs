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

using GetDraftedMessageForGroupResult = OneOf<MessageNotFoundError, ChatMessageDraft?>;

[CachedByUser("drafts", 30)]
public record GetDraftedMessageForGroup(
    [Required] [property: CacheKey] Guid GroupId
) : IQuery<GetDraftedMessageForGroupResult>;

[Timed]
internal sealed class
    GetDraftedMessageForGroupHandler(
        IIdentityContext identityContext,
        IChatMessageDraftRepository drafts
    ) : IQueryHandler<GetDraftedMessageForGroup, GetDraftedMessageForGroupResult>
{
    public async Task<GetDraftedMessageForGroupResult> HandleAsync(
        GetDraftedMessageForGroup query,
        CancellationToken cancellationToken = default)
    {
        var draftMessage = await drafts
            .ForUserAndGroup(identityContext.Id, query.GroupId, cancellationToken);
        
        return draftMessage is null
            ? new MessageNotFoundError(default!)
            : draftMessage;
    }
}