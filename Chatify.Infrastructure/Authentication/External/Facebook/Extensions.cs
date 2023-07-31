using System.Security.Claims;
using static Chatify.Infrastructure.Authentication.External.Constants;

namespace Chatify.Infrastructure.Authentication.External.Facebook;

public static class Extensions
{
    public static ClaimsPrincipal ToClaimsPrincipal(this FacebookUserInfo userInfo)
        => new(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Name),
                    new(ClaimTypes.NameIdentifier, userInfo.Id),
                    new(ClaimTypes.Email, userInfo.Email ?? string.Empty),
                    new(ClaimNames.Picture, userInfo.Picture.Data.Url),
                }));
}