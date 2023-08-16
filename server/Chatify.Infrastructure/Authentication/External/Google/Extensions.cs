using System.Security.Claims;
using Chatify.Infrastructure.Authentication.External.Github;
using static Chatify.Infrastructure.Authentication.External.Constants;

namespace Chatify.Infrastructure.Authentication.External.Google;

public static class Extensions
{
    public static ClaimsPrincipal ToClaimsPrincipal(this GoogleUserInfo userInfo)
        => new(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Name),
                    new(ClaimTypes.NameIdentifier, userInfo.Id),
                    new(ClaimTypes.Email, userInfo.Email),
                    new(ClaimNames.Picture, userInfo.Picture),
                    new(ClaimNames.Locale, userInfo.Locale),
                }));
    
    public static ClaimsPrincipal ToClaimsPrincipal(this GithubUserInfo userInfo)
        => new(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Name),
                    new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()!),
                    new(ClaimTypes.Email, userInfo.Email),
                    new(ClaimNames.Picture, userInfo.AvatarUrl),
                    new(ClaimNames.Locale, userInfo.Location)
                }));
}