using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Seeding;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class DatabaseSeedingService(IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    public const string UseSeedingConfigKey = "UseSeeding";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<CompositeSeeder>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if ( configuration.GetValue<bool>(UseSeedingConfigKey) )
        {
            await seeder.SeedAsync(stoppingToken);
        }

        await PrintTestUser(scope);
    }

    private static async Task PrintTestUser(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeedingService>>();

        var userId = ( await mapper.FetchListAsync<ChatGroupMember>() )
            .GroupBy(m => m.UserId)
            .OrderByDescending(gr => gr.Count())
            .First()
            .Key;

        var user = await mapper.FirstOrDefaultAsync<ChatifyUser>("SELECT username, email FROM users WHERE id = ?", userId);

        logger.LogInformation("Test user credentials: Username - {Username}, Email - {Email} Password - {Password}",
            user.UserName,
            user.Email,
            $"{user.UserName.Titleize()}123!"
        );
    }
}