namespace Chatify.Infrastructure.Authentication.External.Github;

public interface IGithubOAuthClient
{
    Task<GithubUserInfo?> GetUserInfoAsync(
        string code,
        CancellationToken cancellationToken = default);
}