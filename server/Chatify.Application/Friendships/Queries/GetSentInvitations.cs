using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Friendships.Queries;

using GetSentInvitationsResult = OneOf<Error, List<FriendInvitation>>;

public record GetSentInvitations : IQuery<GetSentInvitationsResult>;

internal sealed class GetSentInvitationsHandler : IQueryHandler<GetSentInvitations, GetSentInvitationsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IFriendInvitationRepository _friendInvitations;

    public GetSentInvitationsHandler(
        IIdentityContext identityContext,
        IFriendInvitationRepository friendInvitations)
    {
        _identityContext = identityContext;
        _friendInvitations = friendInvitations;
    }

    public async Task<GetSentInvitationsResult> HandleAsync(
        GetSentInvitations query,
        CancellationToken cancellationToken = default)
    {
        var invites = await _friendInvitations.AllSentByUserAsync(_identityContext.Id, cancellationToken);
        return invites.Any() ? invites : Error.New("No invites found.");
    }
}