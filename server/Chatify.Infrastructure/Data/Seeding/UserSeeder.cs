using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AspNetCore.Identity.Cassandra.Models;
using Bogus;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM.Contracts;
using static Chatify.Infrastructure.Authentication.External.Constants.AuthProviders;
using Media = Chatify.Infrastructure.Data.Models.Media;

namespace Chatify.Infrastructure.Data.Seeding;

public sealed partial class UserSeeder(IServiceScopeFactory factory)
    : BaseSeeder<ChatifyUser>(factory)
{
    public override int Priority => 0;

    private readonly Faker<ChatifyUser> _userFaker = new Faker<ChatifyUser>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.UserName, f => $"{f.Person.FirstName}_${f.Person.LastName}".ToLower())
        .RuleFor(u => u.Status, f => f.PickRandom(Enum.GetValues<UserStatus>()))
        .RuleFor(u => u.ProfilePicture, f => new Media { MediaUrl = f.Internet.Avatar(), Id = Guid.NewGuid() })
        .RuleFor(u => u.PhoneNumbers,
            f => new System.Collections.Generic.HashSet<string> { f.Phone.PhoneNumber(), f.Phone.PhoneNumber() })
        .RuleFor(u => u.EmailConfirmed, _ => true)
        .RuleFor(u => u.Logins,
            (_,
                u) => new LoginInfo[]
            {
                new(RegularLogin, u.Id.ToString(), nameof(RegularLogin))
            })
        .RuleFor(u => u.NormalizedUserName, (_,
            u) => u.UserName.ToUpper())
        .RuleFor(u => u.Email, (f,
            u) =>
        {
            var parts = u.UserName.Split("_", StringSplitOptions.RemoveEmptyEntries);
            return f.Internet.Email(parts[0], parts[1]);
        })
        .RuleFor(u => u.NormalizedEmail, (_,
            u) => u.Email.ToUpper())
        .RuleFor(u => u.EmailConfirmationTime, _ => DateTimeOffset.Now)
        .RuleFor(u => u.DeviceIps,
            f => new HashSet<IPAddress> { f.Internet.IpAddress() })
        .RuleFor(u => u.DisplayName, (_,
            u) => u.UserName.Humanize());

    protected override async Task SeedCoreAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ChatifyUser>>();
        var provider = scope.ServiceProvider
            .GetRequiredService<IRedisConnectionProvider>();

        var pattern = SpecialCharsRegex();

        var users = _userFaker
            .Generate(50)
            .Select(u =>
            {
                u.UserName = pattern.Replace(u.UserName,
                    match => string.Empty);
                u.DisplayName = u.UserName.Humanize();
                return u;
            })
            .ToList();

        foreach ( var user in users )
        {
            _ = await userManager
                .CreateAsync(user, $"{user.UserName.Titleize()}123!");

            // Insert user in RedisJSON cache:
            try
            {
                await provider.RedisCollection<ChatifyUser>().InsertAsync(user);
            }
            catch ( Exception e )
            {
                Console.WriteLine(e);
            }

            _ = await userManager.AddClaimsAsync(
                user, new List<Claim>
                {
                    new(Authentication.External.Constants.ClaimNames.Picture, user.ProfilePicture.MediaUrl),
                });
        }
    }

    [GeneratedRegex(@"[!@#$%^&*()+\-=\[\]{}|;':"",.<>/?~]")]
    private static partial Regex SpecialCharsRegex();
}