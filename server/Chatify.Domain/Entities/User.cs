using System.Net;
using Chatify.Domain.Common;
using Chatify.Domain.ValueObjects;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Domain.Entities;

public class User : IDomainEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;

    public Email Email { get; set; } = default!;

    public UserStatus Status { get; set; }

    public List<string> Roles { get; set; } = new();

    public ISet<PhoneNumber> PhoneNumbers { get; set; } = new HashSet<PhoneNumber>();
    
    public Media ProfilePicture { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    public HashSet<IPAddress> DeviceIps { get; init; } = new();

    public Metadata Metadata { get; init; } = new Dictionary<string, string>();
    
    public string DisplayName { get; set; }
    
    public string UserHandle { get; set; }
    
    public void AddDeviceIp(IPAddress ipAddress)
        => DeviceIps.Add(ipAddress);
}

public enum UserStatus : sbyte
{
    Online,
    Away,
    Offline
}