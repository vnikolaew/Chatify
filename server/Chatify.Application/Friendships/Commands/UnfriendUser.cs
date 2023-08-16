using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using UnfriendUserResult = OneOf<UsersAreNotFriendsError, Error, Unit>;

public record UsersAreNotFriendsError(Guid UserId, Guid FriendId);

public record UnfriendUser(
    [Required] Guid UserId
) : ICommand<UnfriendUserResult>;

internal sealed class UnfriendUserHandler
    : ICommandHandler<UnfriendUser, UnfriendUserResult>
{
    private readonly IFriendshipsRepository _friendships;
    private readonly IChatGroupRepository _groups;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IIdentityContext _identityContext;

    public UnfriendUserHandler(
        IFriendshipsRepository friendships,
        IIdentityContext identityContext, IEventDispatcher eventDispatcher, IClock clock, IChatGroupRepository groups)
    {
        _friendships = friendships;
        _identityContext = identityContext;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _groups = groups;
    }

    public async Task<UnfriendUserResult> HandleAsync(
        UnfriendUser command,
        CancellationToken cancellationToken = default)
    {
        var userFriendships = await _friendships.AllFriendshipsForUser(_identityContext.Id, cancellationToken);
        var friendship = userFriendships
            .FirstOrDefault(fr =>
                fr.FriendTwoId == command.UserId);
        
        if (friendship is null) return new UsersAreNotFriendsError(_identityContext.Id, command.UserId);

        var success = await _friendships.DeleteForUsers(_identityContext.Id, command.UserId, cancellationToken);
        if ( !success ) return Error.New("");

        success = await _groups.DeleteAsync(friendship.GroupId, cancellationToken);
        if ( !success ) return Error.New("");

        await _eventDispatcher.PublishAsync(new UserUnfriendedEvent
        {
            UserId = command.UserId,
            Timestamp = _clock.Now,
            UnfriendedById = _identityContext.Id
        }, cancellationToken);

        return Unit.Default;
    }
}