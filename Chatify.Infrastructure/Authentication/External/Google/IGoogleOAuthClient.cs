namespace Chatify.Infrastructure.Authentication.External.Google;

public interface IGoogleOAuthClient
{
    Task<GoogleUserInfo?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}