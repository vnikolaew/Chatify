using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Friendships.Queries;

using GetSentInvitationsResult = OneOf<BaseError, List<FriendInvitation>>;

public record GetSentInvitations : IQuery<GetSentInvitationsResult>;

internal sealed class GetSentInvitationsHandler(IIdentityContext identityContext,
        IFriendInvitationRepository friendInvitations)
    : IQueryHandler<GetSentInvitations, GetSentInvitationsResult>
{
    public async Task<GetSentInvitationsResult> HandleAsync(GetSentInvitations query,
        CancellationToken cancellationToken = default)
    {
        var invites = await friendInvitations.AllSentByUserAsync(identityContext.Id, cancellationToken);
        return invites;
    }
}