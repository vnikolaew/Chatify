using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Chatify.Infrastructure.Authentication.External.Google;

public class GoogleOAuthClient : IGoogleOAuthClient
{
    private readonly HttpClient _httpClient;

    public GoogleOAuthClient(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var responseMessage = await _httpClient.SendAsync(request, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
        var userInfo = await responseMessage.Content
            .ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: cancellationToken);

        return userInfo;
    }
}