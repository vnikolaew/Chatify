using Chatify.Domain.Repositories;
using Chatify.Services.Shared.Models;
using Chatify.Services.Shared.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Chatify.UsersService.Services;

[Authorize]
internal class UsersServicer : Chatify.Services.Shared.Users.UsersServicer.UsersServicerBase
{
    private readonly IUserRepository _users;

    public UsersServicer(IUserRepository users) => _users = users;

    public override async Task<GetUsersByIdsResponse> GetUsersByIds(
        GetUsersByIdsRequest request, ServerCallContext context)
    {
        var userIds = request.UserIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var users = await _users.GetByIds(userIds, context.CancellationToken);
        var response = new GetUsersByIdsResponse
        {
            Count = users!.Count,
            Users =
            {
                users.Select(user => new UserModel
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Status = ( UserStatus )( user.Status + 1 ),
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    UserHandle = user.UserHandle,
                    PhoneNumbers = { user.PhoneNumbers.Select(_ => _.Value) },
                    ProfilePicture = new Media
                    {
                        Id = user.ProfilePicture.Id.ToString(),
                        Type = user.ProfilePicture.Type ?? string.Empty,
                        FileName = user.ProfilePicture.FileName ?? string.Empty,
                        MediaUrl = user.ProfilePicture.MediaUrl
                    }
                })
            }
        };

        return response;
    }
}