using Chatify.Infrastructure.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class DatabaseSeedingService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<CompositeSeeder>();
        
        await seeder.SeedAsync(stoppingToken);
    }
}