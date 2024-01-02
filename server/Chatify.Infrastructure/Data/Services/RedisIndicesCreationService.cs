using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.OM;
using Redis.OM.Modeling;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class RedisIndicesCreationService(IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private static List<Type> indexTypes => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.GetCustomAttribute<DocumentAttribute>() is not null
                    && t is { IsInterface: false, IsAbstract: false })
        .ToList();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var connectionProvider = scope.ServiceProvider
            .GetRequiredService<RedisConnectionProvider>();

        foreach ( var indexType in indexTypes )
        {
            await connectionProvider.Connection.CreateIndexAsync(indexType);
        }
    }
}