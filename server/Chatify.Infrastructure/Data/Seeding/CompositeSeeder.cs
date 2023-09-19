using Cassandra.Mapping;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class CompositeSeeder(IServiceScopeFactory scopeFactory) : ISeeder
{
    public int Priority => 1;

    private static readonly string[] TablesToBeTruncated =
    {
        "chat_group_members",
        "chat_group_members_count",
        "chat_message_replies_summaries",
        "chat_group_join_requests",
        "chat_messages_reply_count",
        "chat_groups",
        "friends",
        "chat_messages",
        "chat_message_replies",
        "chat_message_reactions",
        "chat_group_attachments",
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        await PurgeCache(scope.ServiceProvider);
        await PurgeDatabaseTables(scope.ServiceProvider);
        await SeedCoreAsync(scope, cancellationToken);
    }

    private static async Task SeedCoreAsync(
        AsyncServiceScope scope,
        CancellationToken cancellationToken
    )
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CompositeSeeder>>();

        var seeders = scope.ServiceProvider
            .GetServices<ISeeder>()
            .Where(s => s is not CompositeSeeder)
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

    private static async Task PurgeDatabaseTables(IServiceProvider serviceProvider)
    {
        var purgeDb = serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection("Cassandra")
            .GetValue<bool>("PurgeDb");
        if ( purgeDb )
        {
            var mapper = serviceProvider.GetRequiredService<IMapper>();
            await PurgeDbTables(mapper, TablesToBeTruncated);
        }
    }

    private static async Task PurgeCache(IServiceProvider serviceProvider)
    {
        var cacheConfig = serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection("Redis");

        var purgeCache = cacheConfig.GetValue<bool>("PurgeCache");
        if ( purgeCache )
        {
            var server = serviceProvider.GetRequiredService<IServer>();
            var cache = serviceProvider.GetRequiredService<IDatabase>();

            var keysSection = cacheConfig
                .GetSection("PurgeKeyPatterns");
            var keyPatterns = keysSection.Get<string[]>();
            await PurgeCacheEntries(cache, server, keyPatterns ?? new string[] { }, false);
        }
    }

    private static async Task PurgeDbTables(
        IMapper mapper,
        string[] tables)
    {
        await mapper.ExecuteAsync("USE chatify;");
        foreach ( var table in tables )
        {
            await mapper.ExecuteAsync($"TRUNCATE {table};");
        }
    }

    private static async Task PurgeCacheEntries(
        IDatabase cache,
        IServer server,
        IEnumerable<string> keyPatterns,
        bool deleteJsonDocuments = false)
    {
        foreach ( var keyPattern in keyPatterns )
        {
            await cache.DeleteAllKeysByPattern(server, keyPattern);
        }

        if ( deleteJsonDocuments )
        {
            await cache.DeleteAllKeysByPattern(server, "User:*");
            await cache.DeleteAllKeysByPattern(server, "ChatGroup:*");
        }
    }
}