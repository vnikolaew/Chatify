namespace Chatify.Infrastructure.Authentication.External.Facebook;

public interface IFacebookOAuthClient
{
    Task<FacebookUserInfo?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}