using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Mappings.Serialization;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Services.External.Common;

public sealed class DatabaseInitializationService(ISession session, ILogger<DatabaseInitializationService> logger)
    : BackgroundService
{
    private const string KeyspaceName = "chatify";

    public static readonly List<Mappings> MappingsList = typeof(IAssemblyMarker)
        .Assembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                    t.IsSubclassOf(typeof(Mappings)))
        .Select(Activator.CreateInstance)
        .Cast<Mappings>()
        .Where(m => m is not null)
        .ToList();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MappingConfiguration.Global.ConvertTypesUsing(new CustomTypeConverter());
        foreach ( var mapping in MappingsList )
        {
            logger.LogInformation("Configuring table {TableName}",
                mapping.GetType().Name.Replace("Mapping", string.Empty));
            MappingConfiguration.Global.Define(mapping);
        }

        await DefineUdts(session);
    }

    private static async Task DefineUdts(ISession session)
    {
        session.ChangeKeyspace(KeyspaceName);
        await session.UserDefinedTypes.DefineAsync(
            MessageReplierInfo.UdtMap,
            Infrastructure.Data.Models.Media.UdtMap,
            MessagePin.UdtMap,
            UdtMap.For<LockoutInfo>(),
            UdtMap.For<PhoneInfo>(),
            UdtMap.For<LoginInfo>(),
            UdtMap.For<TokenInfo>());
    }
}