using System.Net;
using Bogus;
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class UserSeeder : ISeeder
{
    public int Priority => 0;

    private readonly Faker<ChatifyUser> _userFaker;
    private readonly IServiceScopeFactory _scopeFactory;

    public UserSeeder(IServiceScopeFactory scopeFactory, IPasswordHasher hasher)
    {
        _scopeFactory = scopeFactory;
        _userFaker = new Faker<ChatifyUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.PasswordHash, (_, u) => hasher.Secure($"{u.UserName}123"))
            .RuleFor(u => u.Status, f => f.PickRandom(Enum.GetValues<UserStatus>().Select(_ => (sbyte)_)))
            .RuleFor(u => u.ProfilePictureUrl, f => f.Internet.Avatar())
            .RuleFor(u => u.EmailConfirmed, _ => true)
            .RuleFor(u => u.EmailConfirmationTime, _ => DateTimeOffset.Now)
            .RuleFor(u => u.DeviceIps, f => new HashSet<IPAddress> { f.Internet.IpAddress() })
            .RuleFor(u => u.DisplayName, (f, u) => u.UserName.Humanize())
            .RuleFor(u => u.Email, f => f.Internet.Email());
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var users = _userFaker.Generate(20);
        
        foreach (var user in users)
        {
            await mapper.InsertAsync(user, insertNulls: true);
        }
    }
}