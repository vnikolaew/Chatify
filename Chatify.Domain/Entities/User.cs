using System.Net;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public sbyte Status { get; set; }

    public List<string> Roles { get; set; } = new();

    public HashSet<string> ProfilePictures { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    public HashSet<IPAddress> DeviceIps { get; init; } = new();

    public Metadata Metadata { get; init; } = new Dictionary<string, string>();
}