using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Friendships.Commands;

using SendFriendInvitationResult = Either<Error, Guid>;

public record SendFriendInvitation([Required] Guid InviteeId) : ICommand<SendFriendInvitationResult>;

internal sealed class SendFriendInvitationHandler : ICommandHandler<SendFriendInvitation, SendFriendInvitationResult>
{
    private readonly IDomainRepository<FriendInvitation, Guid> _friendInvites;
    private readonly IDomainRepository<User, Guid> _users;
    private readonly IIdentityContext _identityContext;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;

    public SendFriendInvitationHandler(
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IIdentityContext identityContext,
        IDomainRepository<User, Guid> users,
        IClock clock, IEventDispatcher eventDispatcher)
    {
        _friendInvites = friendInvites;
        _identityContext = identityContext;
        _users = users;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<SendFriendInvitationResult> HandleAsync(
        SendFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        var invitee = await _users.GetAsync(command.InviteeId, cancellationToken);
        if (invitee is null) return Error.New("Invitee not found.");

        var friendInvite = new FriendInvitation
        {
            InviteeId = command.InviteeId,
            Id = Guid.NewGuid(),
            Status = (sbyte)FriendInvitationStatus.Pending,
            CreatedAt = _clock.Now,
            InviterId = _identityContext.Id,
        };
        
        var invite = await _friendInvites.SaveAsync(friendInvite, cancellationToken);
        await _eventDispatcher.PublishAsync(new FriendInvitationSentEvent
        {
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            Timestamp = _clock.Now,
        }, cancellationToken);
        return invite.Id;
    }
}