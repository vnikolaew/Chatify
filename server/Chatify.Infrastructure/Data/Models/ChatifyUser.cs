using System.Net;
using System.Reflection;
using AspNetCore.Identity.Cassandra.Models;
using AutoMapper;
using Cassandra.Data.Linq;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.ValueObjects;
using Humanizer;
using Newtonsoft.Json;
using Redis.OM.Modeling;
using DateTimeOffset = System.DateTimeOffset;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

[Document(
    StorageType = StorageType.Json,
    IndexName = "users",
    Prefixes = new[] { nameof(Domain.Entities.User) })]
public class ChatifyUser() :
    CassandraIdentityUser(Guid.NewGuid()),
    IMapFrom<Domain.Entities.User>
{
    public const string CookieConsentMetadata = "Cookie-Consent";

    [RedisIdField]
    [Indexed]
    [Ignore]
    public string RedisId
    {
        get => Id.ToString();
        set => GetType()
            .GetProperty(nameof(Id),
                BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(this, Guid.TryParse(value, out var guid) ? guid : default);
    }

    [Searchable] public string RedisUsername => UserName;

    [Searchable] public string DisplayName { get; set; } = default!;

    // A user handle with the following format: username#0000
    [Searchable] public string UserHandle { get; set; } = default!;

    [Indexed] public sbyte Status { get; set; }

    private readonly IList<string> _phoneNumbers = new List<string>();

    [Indexed]
    public HashSet<string> PhoneNumbers
    {
        get => _phoneNumbers.ToHashSet();
        init => _phoneNumbers = value.ToList();
    }

    [Indexed(CascadeDepth = 1)] public Media ProfilePicture { get; set; }

    private readonly IList<Media> _bannerPictures = new List<Media>();

    [Indexed]
    public HashSet<Media> BannerPictures
    {
        get => _bannerPictures.ToHashSet();
        init => _bannerPictures = value.ToList();
    }

    [Indexed] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    [Indexed] public DateTimeOffset? UpdatedAt { get; set; }

    [Indexed] public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    // private readonly HashSet<byte[]> _deviceIpsBytes = new();

    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public HashSet<byte[]> DeviceIpsBytes
    {
        get => _deviceIps.Select(_ => _.GetAddressBytes()).ToHashSet();
        set => _deviceIps = value.Select(_ => new IPAddress(_)).ToHashSet();
    }

    private HashSet<IPAddress> _deviceIps = new();

    [Indexed]
    public string[] DevicesIpAddresses
    {
        get => DeviceIps.Select(ip => ip.ToString()).ToArray();
        set => DeviceIps = value
            .Select(ip => IPAddress.TryParse(ip, out var ipAddress) ? ipAddress : default)
            .Where(_ => _ is not null)
            .ToHashSet();
    }

    [System.Text.Json.Serialization.JsonIgnore]
    [JsonIgnore]
    public HashSet<IPAddress> DeviceIps
    {
        get => _deviceIps;
        set
        {
            DeviceIpsBytes = value.Select(_ => _.GetAddressBytes()).ToHashSet();
            _deviceIps = value.ToHashSet();
        }
    }

    [Indexed] public Metadata Metadata { get; init; } = new();

    public void AddToken(TokenInfo token)
    {
        ArgumentNullException.ThrowIfNull(token);
        if ( Tokens.Any(x =>
                x.LoginProvider == token.LoginProvider && x.Name == token.Name) )
            throw new InvalidOperationException("Token with LoginProvider: '" + token.LoginProvider + "' and Name: " +
                                                token.Name + " already exists.");
        ( Tokens as List<TokenInfo> )?.Add(token);
    }

    public void AddRole(string role)
    {
        if ( string.IsNullOrEmpty(role) )
            throw new ArgumentNullException(nameof(role));
        if ( Roles.Contains(role) ) return;

        ( Roles as List<string> )?.Add(role);
    }

    public void AddLogin(LoginInfo login)
    {
        ArgumentNullException.ThrowIfNull(login);
        if ( Logins.Any(l =>
                l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey) )
            throw new InvalidOperationException("Login with LoginProvider: '" + login.LoginProvider +
                                                "' and ProviderKey: " + login.ProviderKey + " already exists.");
        ( Logins as List<LoginInfo> )?.Add(login);
    }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatifyUser, Domain.Entities.User>()
            .ForMember(u => u.Email,
                cfg => cfg.MapFrom(u => new Email(u.Email)))
            .ForMember(u => u.Roles,
                cfg =>
                    cfg.MapFrom(u => u.Roles.ToList()))
            .ForMember(u => u.PhoneNumbers,
                cfg => cfg.MapFrom(
                    u => u.PhoneNumbers
                        .Select(_ => new PhoneNumber(_))
                        .ToHashSet()))
            .AfterMap((cu, u) =>
            {
                if ( !cu.Metadata.TryGetValue(
                        nameof(Domain.Entities.User.StarredChatGroups).Underscore().ToLower(),
                        out var starredChatGroups) ) return;
                try
                {
                    var starredGroupIds = JsonSerializer.Deserialize<List<Guid>>(starredChatGroups);
                    starredGroupIds?.ForEach(gId => u.StarredChatGroups.Add(gId));
                }
                catch ( Exception )
                {
                }
            })
            .ReverseMap()
            .ForMember(u => u.PhoneNumbers,
                cfg => cfg.MapFrom(
                    u => u.PhoneNumbers
                        .Select(_ => _.Value)
                        .ToHashSet()))
            .ForMember(u => u.Email,
                cfg => cfg.MapFrom(u => u.Email.Value))
            .AfterMap((u, cu) =>
            {
                if ( u.StarredChatGroups.Any() )
                {
                    cu.Metadata.TryAdd(
                        nameof(Domain.Entities.User.StarredChatGroups).Underscore().ToLower(), string.Empty
                    );
                    cu.Metadata[nameof(Domain.Entities.User.StarredChatGroups).Underscore().ToLower()] =
                        JsonSerializer.Serialize(u.StarredChatGroups);
                }
                else
                {
                    cu.Metadata[nameof(Domain.Entities.User.StarredChatGroups).Underscore().ToLower()] =
                        JsonSerializer.Serialize(Array.Empty<Guid>());
                }
            });
}