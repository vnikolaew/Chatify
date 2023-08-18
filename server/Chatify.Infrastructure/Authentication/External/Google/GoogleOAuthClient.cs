using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Chatify.Infrastructure.Authentication.External.Google;

public class GoogleOAuthClient(HttpClient httpClient) : IGoogleOAuthClient
{
    public const string Endpoint = "https://www.googleapis.com/oauth2/v2/userinfo?alt=json";

    public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var responseMessage = await httpClient.SendAsync(request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
        var userInfo = await responseMessage.Content
            .ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: cancellationToken);

        return userInfo;
    }
}