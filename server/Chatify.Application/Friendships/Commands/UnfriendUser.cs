using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using UnfriendUserResult = OneOf<UsersAreNotFriendsError, Unit>;

public record UsersAreNotFriendsError(Guid UserId, Guid FriendId);

public record UnfriendUser(
   [Required] Guid UserId
) : ICommand<UnfriendUserResult>;

internal sealed class UnfriendUserHandler
   : ICommandHandler<UnfriendUser, UnfriendUserResult>
{
   private readonly IFriendshipsRepository _friendships;
   private readonly IIdentityContext _identityContext;

   public UnfriendUserHandler(
      IFriendshipsRepository friendships,
      IIdentityContext identityContext)
   {
      _friendships = friendships;
      _identityContext = identityContext;
   }

   public async Task<UnfriendUserResult> HandleAsync(
      UnfriendUser command,
      CancellationToken cancellationToken = default)
   {
      var areFriends = ( await _friendships.AllFriendIdsForUser(_identityContext.Id, cancellationToken) )
         .Any(id =>
            id == command.UserId);
      if ( !areFriends ) return new UsersAreNotFriendsError(_identityContext.Id, command.UserId);
  
      var success = await _friendships.DeleteForUsers(_identityContext.Id, command.UserId, cancellationToken);
      return Unit.Default;
   }
}