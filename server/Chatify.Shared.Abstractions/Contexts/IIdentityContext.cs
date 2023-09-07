using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Chatify.Shared.Abstractions.Contexts;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }

    public Guid Id { get; }

    public string Username { get; }

    string Role { get; }

    string? UserLocale { get; }
    
    string? WebSocketConnectionId { get; set; }

    GeoLocation? UserLocation { get; }

    Dictionary<string, IEnumerable<string>> Claims { get; }
}

public record GeoLocation(double Latitude, double Longitude)
{
    public static GeoLocation FromString(string input)
    {
        var parts = input.Split(";", StringSplitOptions.RemoveEmptyEntries);
        return new GeoLocation(
            double.TryParse(parts[0], out var lat) ? lat : default,
            double.TryParse(parts[1], out var @long) ? @long : default
        );
    }

    public static bool TryParse([NotNull] string input, out GeoLocation? geoLocation)
    {
        var parts = input.Split(";", StringSplitOptions.RemoveEmptyEntries);
        double lat = 0, @long = 0;
        var success = parts.Length == 2
                      && double.TryParse(parts[0], out lat)
                      && double.TryParse(parts[1], out @long);
        
        geoLocation = success
            ? new GeoLocation(lat, @long)
            : default;
        
        return success;
    }

    public override string ToString() => $"{Latitude};{Longitude}";
};