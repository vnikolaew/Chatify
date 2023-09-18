using System.Security.Claims;
using Chatify.Domain.Events.Users;
using Chatify.Infrastructure.Authentication.External;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserDetailsEditedEventHandler(
        IServiceScopeFactory scopeFactory,
        IRedisConnectionProvider connectionProvider,
        IMapper dbMapper
    )
    : IEventHandler<UserDetailsEditedEvent>
{
    private readonly IRedisCollection<ChatifyUser> _cacheUsers = connectionProvider.RedisCollection<ChatifyUser>();

    public async Task HandleAsync(
        UserDetailsEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await dbMapper.FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?;", @event.UserId);
        if ( user is null ) return;

        // Invalidate stale User cache info:
        await _cacheUsers.UpdateAsync(user);

        // Update user claims as well:
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ChatifyUser>>();
        
        var claims = await userManager.GetClaimsAsync(user);
        var pictureClaim = claims.FirstOrDefault(c => c.Type == Constants.ClaimNames.Picture);
        if ( pictureClaim is not null && @event.ProfilePicture is not null )
        {
            await userManager.RemoveClaimAsync(user, pictureClaim);
            await userManager.AddClaimAsync(user,
                new Claim(Constants.ClaimNames.Picture, @event.ProfilePicture!.MediaUrl));
        }
    }
}