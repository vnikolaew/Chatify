using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using AspNetCore.Identity.Cassandra;
using AspNetCore.Identity.Cassandra.Models;
using Cassandra;
using Cassandra.Data.Linq;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvalidOperationException = System.InvalidOperationException;

namespace Chatify.Infrastructure.Data.Services;

public class DatabaseInitializationService(CassandraOptions cassandraOptions,
        ISession session,
        ILogger<DatabaseInitializationService> logger)
    : BackgroundService
{
    private const string KeyspaceName = "chatify";

    private readonly string _schemaFilePath = Path.Combine(
        Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName, "Data",
        "schema.cql");

    private static Regex GetUdtCollectionRegex(string keyspaceName)
    {
        var udts = new[] { "tokeninfo", "logininfo", "lockoutinfo", "phoneinfo", "media" };
        return new Regex($"""(?:list|set|map)<"{keyspaceName}"."({string.Join("|", udts)})">""");
    }

    private static async Task CreateTableIfNotExists<T>(
        ISession session,
        Table<T> table)
    {
        var assembly = Assembly.Load("Cassandra");
        var cqlGeneratorType = assembly.GetType("Cassandra.Mapping.Statements.CqlGenerator")!;

        var controlConnectionProp =
            typeof(Metadata).GetProperty("ControlConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var cc = controlConnectionProp.GetValue(session.Cluster.Metadata);
        var sm = cc!.GetType().GetProperty("Serializer", BindingFlags.Instance | BindingFlags.Public)?.GetValue(cc) ??
                 default;
        var serializer = sm.GetType()
            .GetMethod("GetCurrentSerializer")!
            .Invoke(sm, Array.Empty<object>());

        var createCqlQueryMethod = cqlGeneratorType.GetMethod("GetCreate")!;

        var pocoData = table.GetType()
            .GetProperty("PocoData",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(table);

        var collectionRegex = GetUdtCollectionRegex(KeyspaceName);
        var res = createCqlQueryMethod.Invoke(null,
            BindingFlags.Static | BindingFlags.Public,
            null!, parameters: new[]
            {
                serializer,
                pocoData,
                table.Name,
                table.KeyspaceName,
                false
            }, null!) as List<string>;

        var newCql = Regex.Replace(res![0], collectionRegex.ToString(), match =>
        {
            var matchedValue = match.Value.AsSpan();
            var ltIndex = matchedValue.IndexOf('<') + 1;
            var gtIndex = matchedValue.IndexOf('>') + 1;

            return $"{matchedValue[..ltIndex]}frozen<{matchedValue[ltIndex..gtIndex]}>";
        }).Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS");

        await session.ExecuteAsync(new SimpleStatement(newCql));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await DefineUdts(session);
            await CreateIdentityTablesAsync(cassandraOptions);
            await CreateApplicationTablesAsync(stoppingToken);
        }
        catch ( Exception e )
        {
            logger.LogError(e, "An exception was thrown during table creation");
        }
    }

    private static async Task DefineUdts(ISession session)
    {
        session.ChangeKeyspace(Constants.KeyspaceName);
        session.Execute("""
                        CREATE TYPE IF NOT EXISTS message_replier_info (
                            user_id uuid,
                            username text,
                            profile_picture_url text,
                        );
                        """);
        session.Execute("""
                        CREATE TYPE IF NOT EXISTS media (
                            id uuid,
                            media_url text,
                            file_name text,
                            type text
                        );
                        """);
        await session.UserDefinedTypes.DefineAsync(
            MessageReplierInfo.UdtMap,
            Media.UdtMap,
            MessagePin.UdtMap
        );
    }

    private async Task CreateApplicationTablesAsync(CancellationToken stoppingToken)
    {
        var schemaCql = await File.ReadAllTextAsync(_schemaFilePath, Encoding.UTF8, stoppingToken);
        var cqlClauses = schemaCql.Split(
            ";", StringSplitOptions.TrimEntries
                 | StringSplitOptions.RemoveEmptyEntries);

        logger.LogInformation("Starting creation of database tables ...");
        foreach ( var cqlClause in cqlClauses )
        {
            var statement = new SimpleStatement(cqlClause);
            statement.SetKeyspace(Constants.KeyspaceName);

            var rs = await session.ExecuteAsync(statement);
        }

        logger.LogInformation(
            "Finished creation of database tables. Created {TableCount} tables",
            cqlClauses.Length);
    }

    private async Task CreateIdentityTablesAsync(CassandraOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if ( string.IsNullOrEmpty(options.KeyspaceName) )
            throw new InvalidOperationException("Keyspace is null or empty.");
        try
        {
            session.ChangeKeyspace(options.KeyspaceName);
        }
        catch ( InvalidQueryException )
        {
            session.CreateKeyspaceIfNotExists(options.KeyspaceName, options.Replication, options.DurableWrites);
            session.ChangeKeyspace(options.KeyspaceName);
        }

        session.Execute("CREATE TYPE IF NOT EXISTS " + options.KeyspaceName +
                        ".LockoutInfo (EndDate timestamp, Enabled boolean, AccessFailedCount int);");
        session.Execute("CREATE TYPE IF NOT EXISTS " + options.KeyspaceName +
                        ".PhoneInfo (Number text, ConfirmationTime timestamp);");
        session.Execute("CREATE TYPE IF NOT EXISTS " + options.KeyspaceName +
                        ".LoginInfo (LoginProvider text, ProviderKey text, ProviderDisplayName text);");
        session.Execute("CREATE TYPE IF NOT EXISTS " + options.KeyspaceName +
                        ".TokenInfo (LoginProvider text, Name text, Value text);");

        await session.UserDefinedTypes.DefineAsync(
            UdtMap.For<LockoutInfo>(),
            UdtMap.For<PhoneInfo>(),
            UdtMap.For<LoginInfo>(),
            UdtMap.For<TokenInfo>());

        var usersTable = new Table<ChatifyUser>(session);
        var rolesTable = new Table<CassandraIdentityRole>(session);

        try
        {
            await CreateTableIfNotExists(session, usersTable);
            await CreateTableIfNotExists(session, rolesTable);
        }
        catch ( AlreadyExistsException )
        {
        }

        session.Execute("CREATE TABLE IF NOT EXISTS " + options.KeyspaceName +
                        ".userclaims ( UserId uuid,  Type text,  Value text,  PRIMARY KEY (UserId, Type, Value));");
        session.Execute("CREATE TABLE IF NOT EXISTS " + options.KeyspaceName +
                        ".roleclaims ( RoleId uuid,  Type text,  Value text,  PRIMARY KEY (RoleId, Type, Value));");

        var usersTableName = usersTable.GetTable().Name;
        var rolesTableName = rolesTable.GetTable().Name;

        var sessionHelperType = AssemblyLoadContext
            .Default
            .Assemblies
            .FirstOrDefault(a => a.FullName!.Contains("AspNetCore.Identity.Cassandra"))!
            .DefinedTypes
            .FirstOrDefault(t => t.Name.Contains("CassandraSessionHelper"))!;

        sessionHelperType
            .GetProperty("UsersTableName", BindingFlags.Public | BindingFlags.Static)!
            .SetValue(null, usersTableName);

        sessionHelperType
            .GetProperty("RolesTableName", BindingFlags.Public | BindingFlags.Static)!
            .SetValue(null, rolesTableName);

        session.Execute("CREATE MATERIALIZED VIEW IF NOT EXISTS users_by_email AS SELECT * FROM " +
                        options.KeyspaceName + "." + usersTableName +
                        " WHERE NormalizedEmail IS NOT NULL AND id IS NOT NULL AND username IS NOT NULL AND created_at IS NOT NULL PRIMARY KEY (NormalizedEmail, id)");
        session.Execute("CREATE MATERIALIZED VIEW IF NOT EXISTS users_by_username AS SELECT * FROM " +
                        options.KeyspaceName + "." + usersTableName +
                        " WHERE NormalizedUserName IS NOT NULL AND Id IS NOT NULL AND username IS NOT NULL AND created_at IS NOT NULL PRIMARY KEY (NormalizedUserName, Id)");
        session.Execute("CREATE MATERIALIZED VIEW IF NOT EXISTS roles_by_name AS SELECT * FROM " +
                        options.KeyspaceName + "." + rolesTableName +
                        " WHERE NormalizedName IS NOT NULL AND Id IS NOT NULL PRIMARY KEY (NormalizedName, Id)");
        session.Execute("CREATE MATERIALIZED VIEW IF NOT EXISTS userclaims_by_type_and_value AS SELECT * FROM " +
                        options.KeyspaceName +
                        ".userclaims WHERE Type IS NOT NULL AND Value IS NOT NULL AND userid IS NOT NULL PRIMARY KEY ((Type, Value), UserId)");
    }
}