using System.Security.Claims;
using Chatify.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace Chatify.Shared.Infrastructure.Contexts;

public class IdentityContext : IIdentityContext
{
    private const string UserLocaleHeader = "X-User-Locale";

    private const string UserLocationHeader = "X-User-Location";

    public bool IsAuthenticated { get; }
    public Guid Id { get; }
    public string Username { get; set; }
    public string Role { get; }

    public string? UserLocale { get; }

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
        UserLocale = context.Request.Headers.TryGetValue(UserLocaleHeader, out var header)
                     && header.Count == 1
            ? header[0]!
            : default;

        UserLocation = context
                           .Request
                           .Headers
                           .TryGetValue(UserLocationHeader, out var location)
                       && location.Count >= 1
                       && GeoLocation.TryParse(location[0]!, out var geoLocation)
            ? geoLocation
            : default;
    }

    public IdentityContext(ClaimsPrincipal principal)
    {
        if ( principal?.Identity is null
             || string.IsNullOrWhiteSpace(principal?.Identity?.Name) )
        {
            return;
        }

        IsAuthenticated = principal.Identity?.IsAuthenticated is true;
        Id = IsAuthenticated
             && Guid.TryParse(principal.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "Id")?.Value,
                 out var id)
            ? id
            : Guid.Empty;

        Username = IsAuthenticated ? principal.FindFirstValue(ClaimTypes.Name)! : default!;
        Role = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        Claims = principal.Claims.GroupBy(x => x.Type)?
                     .ToDictionary(x => x.Key, x => x.Select(c => c.Value.ToString()))
                 ?? new Dictionary<string, IEnumerable<string>>();
    }

    public static IIdentityContext Empty => new IdentityContext();
}