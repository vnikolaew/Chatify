using System.Net;
using AspNetCore.Identity.Cassandra.Models;
using AutoMapper;
using Chatify.Application.Common.Mappings;
using Chatify.Domain.Entities;
using Chatify.Domain.ValueObjects;
using Redis.OM.Modeling;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

[Document(
    StorageType = StorageType.Json,
    IndexName = "users",
    Prefixes = new[] { nameof(User) })]
public class ChatifyUser() :
    CassandraIdentityUser(Guid.NewGuid()),
    IMapFrom<Domain.Entities.User>
{
    [RedisIdField]
    [Indexed]
    public string RedisId => Id.ToString();

    [Searchable]
    public string RedisUsername => UserName;
    
    [Searchable]
    public string DisplayName { get; set; }
    
    [Indexed]
    public sbyte Status { get; set; }

    private readonly IList<string> _phoneNumbers = new List<string>();

    [Indexed]
    public HashSet<string> PhoneNumbers
    {
        get => _phoneNumbers.ToHashSet();
        init => _phoneNumbers = value.ToList();
    }

    [Indexed(CascadeDepth = 1)]
    public Media ProfilePicture { get; set; }

    private readonly IList<Media> _bannerPictures = new List<Media>();

    [Indexed]
    public HashSet<Media> BannerPictures
    {
        get => _bannerPictures.ToHashSet();
        init => _bannerPictures = value.ToList();
    }

    [Indexed]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    [Indexed]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Indexed]
    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    // private readonly HashSet<byte[]> _deviceIpsBytes = new();

    [Indexed]
    public HashSet<byte[]> DeviceIpsBytes
    {
        get => _deviceIps.Select(_ => _.GetAddressBytes()).ToHashSet();
        set => _deviceIps = value.Select(_ => new IPAddress(_)).ToHashSet();
    }

    private HashSet<IPAddress> _deviceIps = new();

    public ISet<IPAddress> DeviceIps
    {
        get => _deviceIps;
        init
        {
            DeviceIpsBytes = value.Select(_ => _.GetAddressBytes()).ToHashSet();
            _deviceIps = value.ToHashSet();
        }
    }

    [Indexed]
    public Metadata Metadata { get; init; } = new();

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
            .ForMember(u => u.Status,
                cfg => cfg.MapFrom(u => ( UserStatus )u.Status))
            .ForMember(u => u.PhoneNumbers,
                cfg => cfg.MapFrom(
                    u => u.PhoneNumbers
                        .Select(_ => new PhoneNumber(_))
                        .ToHashSet()))
            .ReverseMap()
            .ForMember(u => u.Status,
                cfg => cfg.MapFrom(u => u.Status))
            .ForMember(u => u.PhoneNumbers,
                cfg => cfg.MapFrom(
                    u => u.PhoneNumbers
                        .Select(_ => _.Value)
                        .ToHashSet()))
            .ForMember(u => u.Email,
                cfg => cfg.MapFrom(u => u.Email.Value));
}