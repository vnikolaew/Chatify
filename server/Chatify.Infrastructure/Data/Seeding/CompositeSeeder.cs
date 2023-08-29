using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class CompositeSeeder(IServiceScopeFactory scopeFactory) : ISeeder
{
    public int Priority => 1;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CompositeSeeder>>();

        var seeders = scope.ServiceProvider
            .GetServices<ISeeder>()
            .Where(s => s is ChatGroupMessageSeeder
                or ChatMessageReactionsSeeder
                or ChatMessageRepliesSeeder)
            .OrderBy(s => s.Priority)
            .ToList();

        logger.LogInformation("Starting database seeding ... Using {Seeders}",
            string.Join(", ", seeders.Select(s => s.GetType().Name)));
        foreach ( var seeder in seeders )
        {
            await seeder.SeedAsync(cancellationToken);
        }

        logger.LogInformation("Database seeding done");
    }
}