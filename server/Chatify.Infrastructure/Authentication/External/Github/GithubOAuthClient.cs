using AutoMapper;
using Chatify.Infrastructure.Common.Mappings;
using LanguageExt;
using Octokit;

namespace Chatify.Infrastructure.Authentication.External.Github;

internal sealed class GithubOAuthClient : IGithubOAuthClient
{
    private readonly GitHubClient _gitHubClient
        = new(new ProductHeaderValue("Chatify"));

    private readonly GithubOptions _githubOptions;
    private readonly IMapper _mapper;

    public GithubOAuthClient(GithubOptions githubOptions, IMapper mapper)
    {
        _githubOptions = githubOptions;
        _mapper = mapper;
    }

    public async Task<GithubUserInfo?> GetUserInfoAsync(
        string code, CancellationToken cancellationToken = default)
        => ( await new TryAsync<GithubUserInfo>(async () =>
            {
                var request = new OauthTokenRequest(_githubOptions.ClientId, _githubOptions.ClientSecret, code);
                var token = await _gitHubClient.Oauth.CreateAccessToken(request);

                _gitHubClient.Credentials = new Credentials(token.AccessToken);
                var res = await _gitHubClient.User.Current();
                return res.To<GithubUserInfo>(_mapper);
            }).Invoke() )
            .Match(u => u, _ => default);
}