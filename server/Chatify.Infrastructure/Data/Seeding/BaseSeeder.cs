using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

public abstract class BaseSeeder<TEntity>(IServiceScopeFactory scopeFactory)
    : ISeeder
{
    public abstract int Priority { get; }

    protected readonly IServiceScopeFactory ScopeFactory = scopeFactory;

    public virtual async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();
        var dbMapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        if ( await dbMapper.AnyAsync<TEntity>() ) return;
        await SeedCoreAsync(cancellationToken);
    }

    protected abstract Task SeedCoreAsync(CancellationToken cancellationToken = default);
}