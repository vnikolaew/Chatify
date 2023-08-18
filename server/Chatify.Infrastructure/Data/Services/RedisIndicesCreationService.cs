using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.OM;

namespace Chatify.Infrastructure.Data.Services;

internal sealed class RedisIndicesCreationService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var connectionProvider = scope.ServiceProvider
            .GetRequiredService<RedisConnectionProvider>();
        
        await connectionProvider.Connection.CreateIndexAsync(typeof(ChatifyUser));
    }
}