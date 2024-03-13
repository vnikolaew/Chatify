using Microsoft.Extensions.Hosting;

namespace Chatify.Infrastructure.Data.Services;

public abstract class DelayedBackgroundService : BackgroundService
{
    protected abstract Task WaitAsync(CancellationToken cancellationToken = default);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitAsync(stoppingToken);
        await ExecuteCoreAsync(stoppingToken);
    }

    protected abstract Task ExecuteCoreAsync(CancellationToken stoppingToken);
}