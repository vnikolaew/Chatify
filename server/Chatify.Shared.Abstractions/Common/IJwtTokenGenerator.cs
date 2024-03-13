using System.Security.Claims;

namespace Chatify.Shared.Abstractions.Common;

public interface IJwtTokenGenerator
{
    string Generate(ClaimsPrincipal principal);
}