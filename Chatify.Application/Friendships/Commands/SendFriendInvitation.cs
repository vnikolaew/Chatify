using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
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
    private readonly IFriendInvitationRepository _friendInvites;
    private readonly IDomainRepository<Domain.Entities.User, Guid> _users;
    private readonly IIdentityContext _identityContext;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;

    public SendFriendInvitationHandler(
        IFriendInvitationRepository friendInvites,
        IIdentityContext identityContext,
        IDomainRepository<Domain.Entities.User, Guid> users,
        IClock clock, IEventDispatcher eventDispatcher,
        IGuidGenerator guidGenerator)
    {
        _friendInvites = friendInvites;
        _identityContext = identityContext;
        _users = users;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
        _guidGenerator = guidGenerator;
    }

    public async Task<SendFriendInvitationResult> HandleAsync(
        SendFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        var invitee = await _users.GetAsync(command.InviteeId, cancellationToken);
        if (invitee is null) return Error.New("Invitee not found.");

        var existingInvites = await _friendInvites
            .AllSentByUserAsync(_identityContext.Id, cancellationToken);
        if (existingInvites.Any(i => i.InviteeId == command.InviteeId))
        {
            return Error.New($"An invite to user with Id '{command.InviteeId}' was already sent.");
        }

        var friendInviteId = _guidGenerator.New();
        var friendInvite = new FriendInvitation
        {
            InviteeId = command.InviteeId,
            Id = friendInviteId,
            Status = (sbyte)FriendInvitationStatus.Pending,
            CreatedAt = _clock.Now,
            InviterId = _identityContext.Id,
        };

        var invite = await _friendInvites.SaveAsync(friendInvite, cancellationToken);
        await _eventDispatcher.PublishAsync(new FriendInvitationSentEvent
        {
            Id = friendInviteId,
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            Timestamp = _clock.Now,
        }, cancellationToken);
        return invite.Id;
    }
}