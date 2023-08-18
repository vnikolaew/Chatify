using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace Chatify.Infrastructure.Authentication.External.Facebook;

public class FacebookOAuthClient(HttpClient httpClient) : IFacebookOAuthClient
{
    public const string Endpoint = "https://graph.facebook.com";

    private static readonly string[] Fields =
        { "id", "name", "email", "picture", "hometown", "first_name", "last_name", "gender" };

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
            RequestUri = new Uri($"{httpClient.BaseAddress}/v17.0/me{qs.ToUriComponent()}"),
        };

        var responseMessage = await httpClient.SendAsync(request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
        var userInfo = await responseMessage.Content
            .ReadFromJsonAsync<FacebookUserInfo>(cancellationToken: cancellationToken);

        return userInfo;
    }
}