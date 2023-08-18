using AutoMapper;
using Chatify.Infrastructure.Common.Mappings;
using LanguageExt;
using Octokit;

namespace Chatify.Infrastructure.Authentication.External.Github;

internal sealed class GithubOAuthClient(GithubOptions githubOptions, IMapper mapper) : IGithubOAuthClient
{
    private readonly GitHubClient _gitHubClient
        = new(new ProductHeaderValue("Chatify"));

    public async Task<GithubUserInfo?> GetUserInfoAsync(
        string code, CancellationToken cancellationToken = default)
        => ( await new TryAsync<GithubUserInfo>(async () =>
            {
                var request = new OauthTokenRequest(githubOptions.ClientId, githubOptions.ClientSecret, code);
                var token = await _gitHubClient.Oauth.CreateAccessToken(request);

                _gitHubClient.Credentials = new Credentials(token.AccessToken);
                var res = await _gitHubClient.User.Current();
                return res.To<GithubUserInfo>(mapper);
            }).Invoke() )
            .Match(u => u, _ => default);
}