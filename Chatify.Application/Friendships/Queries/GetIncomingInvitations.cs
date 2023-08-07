using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Friendships.Queries;

using GetIncomingInvitationsResult = OneOf<Error, List<FriendInvitation>>;

public record GetIncomingInvitations : IQuery<GetIncomingInvitationsResult>;

internal sealed class GetIncomingInvitationsHandler
    : IQueryHandler<GetIncomingInvitations, GetIncomingInvitationsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IFriendInvitationRepository _friendInvitations;

    public GetIncomingInvitationsHandler(IIdentityContext identityContext,
        IFriendInvitationRepository friendInvitations)
    {
        _identityContext = identityContext;
        _friendInvitations = friendInvitations;
    }

    public async Task<GetIncomingInvitationsResult> HandleAsync(GetIncomingInvitations query,
        CancellationToken cancellationToken = default)
    {
        var invites = await _friendInvitations.AllSentToUserAsync(_identityContext.Id, cancellationToken);
        return invites.Any() ? invites : Error.New("No invites found.");
    }
}