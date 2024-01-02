using System.Net;
using Chatify.Domain.Common;
using Chatify.Domain.ValueObjects;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Domain.Entities;

public class User : IDomainEntity
{
    private string _username = default!;
    
    public Guid Id { get; set; }

    public string Username
    {
        get => _username;
        set
        {
            UserHandle = UserHandle?.Replace(_username, value) ?? string.Empty;
            _username = value;
        }
    }

    public Email Email { get; set; } = default!;

    public UserStatus Status { get; set; }

    public List<string> Roles { get; set; } = [];

    public ISet<PhoneNumber> PhoneNumbers { get; set; } = new HashSet<PhoneNumber>();

    public Media ProfilePicture { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    public HashSet<IPAddress> DeviceIps { get; init; } = [];

    public Metadata Metadata { get; init; } = new Dictionary<string, string>();

    public HashSet<Guid> StarredChatGroups { get; init; } = [];

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