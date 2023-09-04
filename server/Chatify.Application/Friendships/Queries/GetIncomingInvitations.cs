using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Friendships.Queries;

using GetIncomingInvitationsResult = OneOf<BaseError, List<FriendInvitation>>;

public record GetIncomingInvitations : IQuery<GetIncomingInvitationsResult>;

internal sealed class GetIncomingInvitationsHandler(IIdentityContext identityContext,
        IFriendInvitationRepository friendInvitations)
    : IQueryHandler<GetIncomingInvitations, GetIncomingInvitationsResult>
{
    public async Task<GetIncomingInvitationsResult> HandleAsync(GetIncomingInvitations query,
        CancellationToken cancellationToken = default)
    {
        var invites = await friendInvitations.AllSentToUserAsync(identityContext.Id, cancellationToken);
        return invites;
    }
}