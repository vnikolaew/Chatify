using System.Net;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;

    public string Email { get; set; } = default!;

    public UserStatus Status { get; set; }

    public List<string> Roles { get; set; } = new();

    public string ProfilePictureUrl { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    public HashSet<IPAddress> DeviceIps { get; init; } = new();

    public Metadata Metadata { get; init; } = new Dictionary<string, string>();
    
    public string DisplayName { get; set; }
}

public enum UserStatus : sbyte
{
    Online,
    Away,
    Offline
}