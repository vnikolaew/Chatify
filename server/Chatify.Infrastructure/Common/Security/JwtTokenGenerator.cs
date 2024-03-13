using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chatify.Infrastructure.Common.Settings;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Time;
using Microsoft.IdentityModel.Tokens;

namespace Chatify.Infrastructure.Common.Security;

public sealed class JwtTokenGenerator(
    JwtSettings settings,
    IClock clock) : IJwtTokenGenerator
{
    public string Generate(ClaimsPrincipal principal)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("Id", principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
                new Claim(JwtRegisteredClaimNames.Sub, principal.FindFirstValue(ClaimTypes.Name)!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                ..principal.Claims
            ]),
            Expires = clock.Now.AddMinutes(10),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(settings.KeyBytes),
                SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}