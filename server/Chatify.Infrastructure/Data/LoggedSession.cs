using System.Net;
using Cassandra;
using Cassandra.DataStax.Graph;
using Cassandra.Metrics;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Data;

public class LoggedSession(ISession inner, ILogger<LoggedSession> logger) : ISession
{
    public void Dispose()
    {
        inner.Dispose();
    }

    public IAsyncResult BeginExecute(
        IStatement statement, AsyncCallback callback, object state)
        => inner.BeginExecute(statement, callback, state);

    public IAsyncResult BeginExecute(string cqlQuery, ConsistencyLevel consistency, AsyncCallback callback,
        object state)
        => inner.BeginExecute(cqlQuery, consistency, callback, state);

    public IAsyncResult BeginPrepare(string cqlQuery, AsyncCallback callback, object state)
        => inner.BeginPrepare(cqlQuery, callback, state);

    public void ChangeKeyspace(string keyspaceName)
        => inner.ChangeKeyspace(keyspaceName);

    public void CreateKeyspace(string keyspaceName, Dictionary<string, string> replication = null,
        bool durableWrites = true)
        => inner.ChangeKeyspace(keyspaceName);

    public void CreateKeyspaceIfNotExists(string keyspaceName, Dictionary<string, string> replication = null,
        bool durableWrites = true)
        => inner.CreateKeyspaceIfNotExists(keyspaceName, replication, durableWrites);

    public void DeleteKeyspace(string keyspaceName)
        => inner.DeleteKeyspace(keyspaceName);

    public void DeleteKeyspaceIfExists(string keyspaceName)
        => inner.DeleteKeyspaceIfExists(keyspaceName);

    public RowSet EndExecute(IAsyncResult ar)
        => inner.EndExecute(ar);

    public PreparedStatement EndPrepare(IAsyncResult ar)
        => inner.EndPrepare(ar);

    public RowSet Execute(IStatement statement, string executionProfileName)
    {
        LogStatement(statement);
        return inner.Execute(statement, executionProfileName);
    }

    public RowSet Execute(IStatement statement)
    {
        LogStatement(statement);
        return inner.Execute(statement);
    }

    public RowSet Execute(string cqlQuery)
    {
        logger.LogInformation("Executing query: {Query}", cqlQuery);
        return inner.Execute(cqlQuery);
    }

    public RowSet Execute(string cqlQuery, string executionProfileName)
    {
        logger.LogInformation("Executing query: {Query}", cqlQuery);
        return inner.Execute(cqlQuery, executionProfileName);
    }

    public RowSet Execute(string cqlQuery, ConsistencyLevel consistency)
    {
        logger.LogInformation("Executing query: {Query}", cqlQuery);
        return inner.Execute(cqlQuery, consistency);
    }

    public RowSet Execute(string cqlQuery, int pageSize)
    {
        return inner.Execute(cqlQuery, pageSize);
    }

    public Task<RowSet> ExecuteAsync(IStatement statement)
    {
        LogStatement(statement);
        return inner.ExecuteAsync(statement);
    }

    public Task<RowSet> ExecuteAsync(IStatement statement, string executionProfileName)
    {
        LogStatement(statement);
        return inner.ExecuteAsync(statement, executionProfileName);
    }

    private void LogStatement(IStatement statement)
    {
        if (statement is SimpleStatement ss)
        {
            logger.LogInformation("Executing query: {Query}", ss.QueryString);
        }
    }

    public PreparedStatement Prepare(string cqlQuery)
        => inner.Prepare(cqlQuery);

    public PreparedStatement Prepare(string cqlQuery, IDictionary<string, byte[]> customPayload)
        => inner.Prepare(cqlQuery, customPayload);

    public PreparedStatement Prepare(string cqlQuery, string keyspace)
        => inner.Prepare(cqlQuery, keyspace);

    public PreparedStatement Prepare(string cqlQuery, string keyspace, IDictionary<string, byte[]> customPayload)
        => inner.Prepare(cqlQuery, keyspace, customPayload);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery)
        => inner.PrepareAsync(cqlQuery);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, IDictionary<string, byte[]> customPayload)
        => inner.PrepareAsync(cqlQuery);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, string keyspace)
        => inner.PrepareAsync(cqlQuery, keyspace);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, string keyspace,
        IDictionary<string, byte[]> customPayload)
        => inner.PrepareAsync(cqlQuery, keyspace, customPayload);

    public IDriverMetrics GetMetrics()
        => inner.GetMetrics();

    public GraphResultSet ExecuteGraph(IGraphStatement statement)
        => inner.ExecuteGraph(statement);

    public Task<GraphResultSet> ExecuteGraphAsync(IGraphStatement statement)
        => inner.ExecuteGraphAsync(statement);

    public GraphResultSet ExecuteGraph(IGraphStatement statement, string executionProfileName)
        => inner.ExecuteGraph(statement, executionProfileName);

    public Task<GraphResultSet> ExecuteGraphAsync(IGraphStatement statement, string executionProfileName)
        => inner.ExecuteGraphAsync(statement, executionProfileName);

    public Task ShutdownAsync()
        => inner.ShutdownAsync();

    public void WaitForSchemaAgreement(RowSet rs)
        => inner.WaitForSchemaAgreement(rs);

    public bool WaitForSchemaAgreement(IPEndPoint forHost)
        => inner.WaitForSchemaAgreement(forHost);

    public int BinaryProtocolVersion => inner.BinaryProtocolVersion;

    public ICluster Cluster => inner.Cluster;

    public bool IsDisposed => inner.IsDisposed;

    public string Keyspace => inner.Keyspace;

    public UdtMappingDefinitions UserDefinedTypes => inner.UserDefinedTypes;

    public string SessionName => inner.SessionName;
}