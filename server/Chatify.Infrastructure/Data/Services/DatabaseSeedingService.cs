using Chatify.Infrastructure.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class DatabaseSeedingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseSeedingService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<CompositeSeeder>();
        
        await seeder.SeedAsync(stoppingToken);
    }
}