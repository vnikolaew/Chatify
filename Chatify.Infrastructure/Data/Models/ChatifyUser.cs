using System.Net;
using AspNetCore.Identity.Cassandra.Models;
using AutoMapper;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatifyUser : CassandraIdentityUser, IMapFrom<Domain.Entities.User>
{
    public ChatifyUser() : base(Guid.NewGuid())
    {
    }

    public string DisplayName { get; set; }

    public sbyte Status { get; set; }

    private readonly IList<string> _phoneNumbers = new List<string>();

    public HashSet<string> PhoneNumbers
    {
        get => _phoneNumbers.ToHashSet();
        init => _phoneNumbers = value.ToList();
    }

    private readonly IList<string> _profilePictures = new List<string>();

    public HashSet<string> ProfilePictures
    {
        get => _profilePictures.ToHashSet();
        init => _profilePictures = value.ToList();
    }

    private readonly IList<string> _bannerPictures = new List<string>();

    public HashSet<string> BannerPictures
    {
        get => _bannerPictures.ToHashSet();
        init => _bannerPictures = value.ToList();
    }

    [ClusteringKey(0, SortOrder.Descending)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.Now;

    public HashSet<IPAddress> DeviceIps { get; init; } = new();

    public Metadata Metadata { get; init; } = new();

    public void AddToken(TokenInfo token)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (Tokens.Any(x =>
                x.LoginProvider == token.LoginProvider && x.Name == token.Name))
            throw new InvalidOperationException("Token with LoginProvider: '" + token.LoginProvider + "' and Name: " +
                                                token.Name + " already exists.");
        (Tokens as List<TokenInfo>)?.Add(token);
    }

    public void AddRole(string role)
    {
        if (string.IsNullOrEmpty(role))
            throw new ArgumentNullException(nameof(role));
        if (Roles.Contains(role)) return;

        (Roles as List<string>)?.Add(role);
    }

    public void AddLogin(LoginInfo login)
    {
        ArgumentNullException.ThrowIfNull(login);
        if (Logins.Any(l =>
                l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey))
            throw new InvalidOperationException("Login with LoginProvider: '" + login.LoginProvider +
                                                "' and ProviderKey: " + login.ProviderKey + " already exists.");
        (Logins as List<LoginInfo>)?.Add(login);
    }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<ChatifyUser, Domain.Entities.User>()
            .ForMember(u => u.Roles,
                cfg =>
                    cfg.MapFrom(u => u.Roles.ToList()))
            .ReverseMap();
}