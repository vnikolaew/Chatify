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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CompositeSeeder>>();

        var purgeCache = scope.ServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection("Redis")
            .GetValue<bool>("PurgeCache");
        if ( purgeCache )
        {
            var server = scope.ServiceProvider.GetRequiredService<IServer>();
            var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();
            await PurgeCacheEntries(cache, server, false);
        }

        var purgeDb = scope.ServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection("Cassandra")
            .GetValue<bool>("PurgeDb");
        if ( purgeDb )
        {
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            await PurgeDbTables(mapper, TablesToBeTruncated);
        }

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
        bool deleteJsonDocuments = false)
    {
        await cache.DeleteAllKeysByPattern(server, "user:*");
        await cache.DeleteAllKeysByPattern(server, "message:*");
        await cache.DeleteAllKeysByPattern(server, "groups:*");

        if ( deleteJsonDocuments )
        {
            await cache.DeleteAllKeysByPattern(server, "User:*");
            await cache.DeleteAllKeysByPattern(server, "ChatGroup:*");
        }
    }
}