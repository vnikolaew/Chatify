using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace Chatify.Infrastructure.Authentication.External.Facebook;

public class FacebookOAuthClient : IFacebookOAuthClient
{
    private readonly HttpClient _httpClient;

    private static readonly string[] Fields =
        { "id", "name", "email", "picture", "hometown", "first_name", "last_name", "gender" };

    public FacebookOAuthClient(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<FacebookUserInfo?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var qs = QueryString
            .Create("access_token", accessToken)
            .Add("fields", string.Join(",", Fields));
        
        var request = new HttpRequestMessage
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
            RequestUri = new Uri($"{_httpClient.BaseAddress}/v17.0/me{qs.ToUriComponent()}"),
        };

        var responseMessage = await _httpClient.SendAsync(request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
        var userInfo = await responseMessage.Content
            .ReadFromJsonAsync<FacebookUserInfo>(cancellationToken: cancellationToken);

        return userInfo;
    }
}