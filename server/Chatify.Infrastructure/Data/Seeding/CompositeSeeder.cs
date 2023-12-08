using Cassandra.Mapping;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ChatGroupMember = Chatify.Domain.Entities.ChatGroupMember;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class CompositeSeeder(IServiceScopeFactory scopeFactory) : ISeeder
{
    public int Priority => 1;
    
    private const string CassandraConfigSectionName = "Cassandra";
    private const string RedisConfigSectionName = "Redis";
    
    private const string PurgeCacheConfigName = "PurgeCache";
    private const string PurgeKeyPatternsConfigName = "PurgeKeyPatterns";
    
    private static readonly string[] TablesToBeTruncated =
    {
        nameof(ChatGroupMember).Pluralize().Underscore(),
        nameof(ChatGroupMembersCount).Pluralize().Underscore(),
        nameof(ChatGroupJoinRequest).Pluralize().Underscore(),
        nameof(ChatGroup).Pluralize().Underscore(),
        nameof(ChatMessage).Pluralize().Underscore(),
        nameof(ChatMessageReply).Pluralize().Underscore(),
        nameof(ChatMessageReplyCount).Pluralize().Underscore(),
        nameof(ChatMessageReaction).Pluralize().Underscore(),
        nameof(ChatGroupAttachment).Pluralize().Underscore(),
        nameof(ChatMessageRepliesSummary).Pluralize().Underscore(),
        "friends",
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
            .GetSection(CassandraConfigSectionName)
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
            .GetSection(RedisConfigSectionName);

        var purgeCache = cacheConfig.GetValue<bool>(PurgeCacheConfigName);
        if ( purgeCache )
        {
            var server = serviceProvider.GetRequiredService<IServer>();
            var cache = serviceProvider.GetRequiredService<IDatabase>();

            var keysSection = cacheConfig
                .GetSection(PurgeKeyPatternsConfigName);
            var keyPatterns = keysSection.Get<string[]>();
            await PurgeCacheEntries(
                cache, server, keyPatterns ?? Array.Empty<string>(),
                false);
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

        var jsonPatterns = new[] { "User:*", "ChatGroup:*" };
        if ( deleteJsonDocuments )
        {
            foreach ( var jsonPattern in jsonPatterns )
            {
                await cache.DeleteAllKeysByPattern(server, jsonPattern);
            }
        }
    }
}