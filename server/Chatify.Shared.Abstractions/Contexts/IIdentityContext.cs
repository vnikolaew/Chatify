namespace Chatify.Shared.Abstractions.Contexts;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }

    public Guid Id { get; }

    public string Username { get; }

    string Role { get; }

    string UserLocale { get; }

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

    public override string ToString() => $"{Latitude};{Longitude}";
};