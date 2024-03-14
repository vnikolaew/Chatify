using Chatify.Application.User.Contracts;
using Chatify.Domain.ValueObjects;
using GetUsersByIdsRequest = Chatify.Services.Shared.Users.GetUsersByIdsRequest;
using Media = Chatify.Domain.Entities.Media;
using UsersServicer = Chatify.Services.Shared.Users.UsersServicer;
using UserStatus = Chatify.Domain.Entities.UserStatus;

namespace Chatify.Infrastructure.Services.External.Users;

public sealed class UsersService(UsersServicer.UsersServicerClient client) : IUsersService
{
    public async Task<List<Domain.Entities.User>> GetByIds(IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var request = new GetUsersByIdsRequest
        {
            UserIds = { userIds.Select(_ => _.ToString()) }
        };
        var response = await client.GetUsersByIdsAsync(request, cancellationToken: cancellationToken);

        return response.Users.Select(user => new Domain.Entities.User
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email,
            DisplayName = user.DisplayName,
            UserHandle = user.UserHandle,
            Status = ( UserStatus )( user.Status - 1 ),
            Username = user.Username,
            PhoneNumbers = new HashSet<PhoneNumber>(user.PhoneNumbers.Select(pn => new PhoneNumber(pn))),
            ProfilePicture = new Media
            {
                Id = Guid.Parse(user.ProfilePicture.Id),
                Type = user.ProfilePicture.Type,
                FileName = user.ProfilePicture.FileName,
                MediaUrl = user.ProfilePicture.MediaUrl
            },
        }).ToList();
    }

    public async Task<Domain.Entities.User?> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var response = await client.GetUsersByIdsAsync(
            new GetUsersByIdsRequest { UserIds = { new[] { userId.ToString() } } },
            cancellationToken: cancellationToken);
        if ( response.Count == 0 ) return default;

        var responseUser = response.Users[0];
        return new Domain.Entities.User
        {
            Id = Guid.Parse(responseUser.Id),
            PhoneNumbers = responseUser.PhoneNumbers.Select(p => new PhoneNumber(p)).ToHashSet(),

            Email = responseUser.Email,
            Username = responseUser.Username,
            DisplayName = responseUser.DisplayName,
            UserHandle = responseUser.UserHandle,
            Status = ( UserStatus )( responseUser.Status - 1 ),
            ProfilePicture = new Media
            {
                Id = Guid.Parse(responseUser.ProfilePicture.Id),
                FileName = responseUser.ProfilePicture.FileName,
                MediaUrl = responseUser.ProfilePicture.MediaUrl,
                Type = responseUser.ProfilePicture.Type
            },
        };
    }
}