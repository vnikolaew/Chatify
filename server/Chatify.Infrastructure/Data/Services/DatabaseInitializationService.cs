using System.Reflection;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Mappings.Serialization;
using Chatify.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace Chatify.Infrastructure.Data.Services;

public class DatabaseInitializationService(
    ISession session,
    IMapper mapper,
    ILogger<DatabaseInitializationService> logger)
    : DelayedBackgroundService
{
    private const string KeyspaceName = "chatify";

    private const int ExpectedTablesCount = 18;

    public static readonly List<Cassandra.Mapping.Mappings> MappingsList = Assembly
        .GetExecutingAssembly()
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                    t.IsSubclassOf(typeof(Cassandra.Mapping.Mappings)))
        .Select(Activator.CreateInstance)
        .Cast<Cassandra.Mapping.Mappings>()
        .Where(m => m is not null)
        .ToList();

    protected override async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        const int millisDelay = 5_000;
        while ( !cancellationToken.IsCancellationRequested )
        {
            try
            {
                session.ChangeKeyspace("system");
                var result = await mapper.SingleAsync<long>(
                    "SELECT COUNT(*) FROM system_schema.tables WHERE keyspace_name = ?;",
                    KeyspaceName);
                if ( result == ExpectedTablesCount ) break;
            }
            catch ( Exception )
            {
                logger.LogInformation("Cassandra node(s) is/are not up. Retrying again in 5000ms");
                await Task.Delay(millisDelay, cancellationToken);
            }
        }
    }

    protected override async Task ExecuteCoreAsync(CancellationToken stoppingToken)
    {
        try
        {
            MappingConfiguration.Global.ConvertTypesUsing(new CustomTypeConverter());
            foreach ( var mapping in MappingsList )
            {
                logger.LogInformation("Configuring table {TableName}", mapping.GetType().Name.Replace("Mapping", string.Empty));
                MappingConfiguration.Global.Define(mapping);
                
            }

            await DefineUdts(session);
        }
        catch ( Exception e )
        {
            logger.LogError(e, "An exception was thrown during table creation");
        }
    }

    private static async Task DefineUdts(ISession session)
    {
        session.ChangeKeyspace(KeyspaceName);
        await session.UserDefinedTypes.DefineAsync(
            MessageReplierInfo.UdtMap,
            Media.UdtMap,
            MessagePin.UdtMap,
            UdtMap.For<LockoutInfo>(),
            UdtMap.For<PhoneInfo>(),
            UdtMap.For<LoginInfo>(),
            UdtMap.For<TokenInfo>());
    }
}