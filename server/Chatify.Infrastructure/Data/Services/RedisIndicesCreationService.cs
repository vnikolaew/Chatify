using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Redis.OM.Modeling;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class RedisIndicesCreationService(
    IServiceScopeFactory scopeFactory,
    ILogger<RedisIndicesCreationService> logger)
    : DelayedBackgroundService
{
    private static List<Type> IndexTypes => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.GetCustomAttribute<DocumentAttribute>() is not null
                    && t is { IsInterface: false, IsAbstract: false })
        .ToList();

    protected override async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        const int millisDelay = 5_000;
        await using var scope = scopeFactory.CreateAsyncScope();

        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                var connectionMultiplexer = scope.ServiceProvider
                    .GetRequiredService<IConnectionMultiplexer>();

                if ( !connectionMultiplexer.IsConnected )
                {
                    var db = connectionMultiplexer.GetDatabase();
                    _ = await db.PingAsync();
                }

                break;
            }
            catch ( Exception e )
            {
                logger.LogInformation("Redis server is not up. Retrying again in 5000ms");
                await Task.Delay(millisDelay, cancellationToken);
            }
        }
    }

    protected override async Task ExecuteCoreAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var connectionProvider = scope.ServiceProvider
            .GetRequiredService<RedisConnectionProvider>();

        foreach ( var indexType in IndexTypes )
        {
            await connectionProvider.Connection.CreateIndexAsync(indexType);
        }
    }
}