using System.Globalization;
using System.Security.Claims;
using Chatify.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace Chatify.Shared.Infrastructure.Contexts;

public class IdentityContext : IIdentityContext
{
    public bool IsAuthenticated { get; }
    public Guid Id { get; }
    public string Username { get; set; }
    public string Role { get; }

    public string UserLocale { get; }

    public GeoLocation? UserLocation { get; }
    public Dictionary<string, IEnumerable<string>> Claims { get; }

    private IdentityContext()
    {
    }

    public IdentityContext(Guid? id)
    {
        Id = id ?? Guid.Empty;
        IsAuthenticated = id.HasValue;
    }

    public IdentityContext(HttpContext context) : this(context.User)
    {
        UserLocale = context.Request.Headers["X-User-Locale"][0] ??
                     CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
        UserLocation = context.Request.Headers.TryGetValue("X-User-Location", out var location)
                       && location.Count >= 1
            ? GeoLocation.FromString(location[0]!)
            : default;
    }

    public IdentityContext(ClaimsPrincipal principal)
    {
        if ( principal?.Identity is null || string.IsNullOrWhiteSpace(principal?.Identity?.Name) )
        {
            return;
        }

        IsAuthenticated = principal.Identity?.IsAuthenticated is true;
        Id = IsAuthenticated
             && Guid.TryParse(principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                 out var id)
            ? id
            : Guid.Empty;

        Username = IsAuthenticated ? principal.FindFirstValue(ClaimTypes.Name)! : default!;
        Role = principal?.Claims?.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        Claims = principal?.Claims?.GroupBy(x => x.Type)?
            .ToDictionary(x => x.Key, x => x.Select(c => c.Value.ToString()));
    }

    public static IIdentityContext Empty => new IdentityContext();
}