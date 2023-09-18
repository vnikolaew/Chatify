using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Messages.Queries;

using GetDraftedMessagesResult = OneOf<Error, List<ChatMessageDraft>>;

[CachedByUser("drafts", 30)]
public record GetDraftedMessages : IQuery<GetDraftedMessagesResult>;

[Timed]
internal sealed class GetDraftedMessagesHandler(
    IIdentityContext identityContext,
    IChatMessageDraftRepository drafts
    ) : IQueryHandler<GetDraftedMessages, GetDraftedMessagesResult>
{
    public async Task<GetDraftedMessagesResult> HandleAsync(GetDraftedMessages query,
        CancellationToken cancellationToken = default)
        => await drafts.AllForUser(identityContext.Id, cancellationToken);
}