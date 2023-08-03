using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class CompositeSeeder : ISeeder
{
    public int Priority => 1;

    private readonly IServiceScopeFactory _scopeFactory;

    public CompositeSeeder(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CompositeSeeder>>();

        var seeders = scope.ServiceProvider
            .GetServices<ISeeder>()
            .Where(s => s is not CompositeSeeder)
            .OrderBy(s => s.Priority)
            .ToList();

        logger.LogInformation("Starting database seeding ... Using {Seeders}",
            string.Join(", ", seeders.Select(s => s.GetType().Name)));
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(cancellationToken);
        }

        logger.LogInformation("Database seeding done");
    }
}