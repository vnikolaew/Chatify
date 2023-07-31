using System.Net;
using Cassandra;
using Cassandra.DataStax.Graph;
using Cassandra.Metrics;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Data;

public class LoggedSession : ISession
{
    private readonly ISession _inner;
    private readonly ILogger<LoggedSession> _logger;

    public LoggedSession(ISession inner, ILogger<LoggedSession> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Dispose()
    {
        _inner.Dispose();
    }

    public IAsyncResult BeginExecute(
        IStatement statement, AsyncCallback callback, object state)
        => _inner.BeginExecute(statement, callback, state);

    public IAsyncResult BeginExecute(string cqlQuery, ConsistencyLevel consistency, AsyncCallback callback,
        object state)
        => _inner.BeginExecute(cqlQuery, consistency, callback, state);

    public IAsyncResult BeginPrepare(string cqlQuery, AsyncCallback callback, object state)
        => _inner.BeginPrepare(cqlQuery, callback, state);

    public void ChangeKeyspace(string keyspaceName)
        => _inner.ChangeKeyspace(keyspaceName);

    public void CreateKeyspace(string keyspaceName, Dictionary<string, string> replication = null,
        bool durableWrites = true)
        => _inner.ChangeKeyspace(keyspaceName);

    public void CreateKeyspaceIfNotExists(string keyspaceName, Dictionary<string, string> replication = null,
        bool durableWrites = true)
        => _inner.CreateKeyspaceIfNotExists(keyspaceName, replication, durableWrites);

    public void DeleteKeyspace(string keyspaceName)
        => _inner.DeleteKeyspace(keyspaceName);

    public void DeleteKeyspaceIfExists(string keyspaceName)
        => _inner.DeleteKeyspaceIfExists(keyspaceName);

    public RowSet EndExecute(IAsyncResult ar)
        => _inner.EndExecute(ar);

    public PreparedStatement EndPrepare(IAsyncResult ar)
        => _inner.EndPrepare(ar);

    public RowSet Execute(IStatement statement, string executionProfileName)
    {
        LogStatement(statement);
        return _inner.Execute(statement, executionProfileName);
    }

    public RowSet Execute(IStatement statement)
    {
        LogStatement(statement);
        return _inner.Execute(statement);
    }

    public RowSet Execute(string cqlQuery)
    {
        _logger.LogInformation("Executing query: {Query}", cqlQuery);
        return _inner.Execute(cqlQuery);
    }

    public RowSet Execute(string cqlQuery, string executionProfileName)
    {
        _logger.LogInformation("Executing query: {Query}", cqlQuery);
        return _inner.Execute(cqlQuery, executionProfileName);
    }

    public RowSet Execute(string cqlQuery, ConsistencyLevel consistency)
    {
        _logger.LogInformation("Executing query: {Query}", cqlQuery);
        return _inner.Execute(cqlQuery, consistency);
    }

    public RowSet Execute(string cqlQuery, int pageSize)
    {
        return _inner.Execute(cqlQuery, pageSize);
    }

    public Task<RowSet> ExecuteAsync(IStatement statement)
    {
        LogStatement(statement);
        return _inner.ExecuteAsync(statement);
    }

    public Task<RowSet> ExecuteAsync(IStatement statement, string executionProfileName)
    {
        LogStatement(statement);
        return _inner.ExecuteAsync(statement, executionProfileName);
    }
    
    private void LogStatement(IStatement statement)
    {
        if (statement is SimpleStatement ss)
        {
            _logger.LogInformation("Executing query: {Query}", ss.QueryString);
        }
    }

    public PreparedStatement Prepare(string cqlQuery)
        => _inner.Prepare(cqlQuery);

    public PreparedStatement Prepare(string cqlQuery, IDictionary<string, byte[]> customPayload)
        => _inner.Prepare(cqlQuery, customPayload);

    public PreparedStatement Prepare(string cqlQuery, string keyspace)
        => _inner.Prepare(cqlQuery, keyspace);

    public PreparedStatement Prepare(string cqlQuery, string keyspace, IDictionary<string, byte[]> customPayload)
        => _inner.Prepare(cqlQuery, keyspace, customPayload);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery)
        => _inner.PrepareAsync(cqlQuery);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, IDictionary<string, byte[]> customPayload)
        => _inner.PrepareAsync(cqlQuery);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, string keyspace)
        => _inner.PrepareAsync(cqlQuery, keyspace);

    public Task<PreparedStatement> PrepareAsync(string cqlQuery, string keyspace, IDictionary<string, byte[]> customPayload)
        => _inner.PrepareAsync(cqlQuery, keyspace, customPayload);

    public IDriverMetrics GetMetrics()
        => _inner.GetMetrics();

    public GraphResultSet ExecuteGraph(IGraphStatement statement)
        => _inner.ExecuteGraph(statement);

    public Task<GraphResultSet> ExecuteGraphAsync(IGraphStatement statement)
        => _inner.ExecuteGraphAsync(statement);

    public GraphResultSet ExecuteGraph(IGraphStatement statement, string executionProfileName)
        => _inner.ExecuteGraph(statement, executionProfileName);

    public Task<GraphResultSet> ExecuteGraphAsync(IGraphStatement statement, string executionProfileName)
        => _inner.ExecuteGraphAsync(statement, executionProfileName);

    public Task ShutdownAsync()
        => _inner.ShutdownAsync();

    public void WaitForSchemaAgreement(RowSet rs)
        => _inner.WaitForSchemaAgreement(rs);

    public bool WaitForSchemaAgreement(IPEndPoint forHost)
        => _inner.WaitForSchemaAgreement(forHost);

    public int BinaryProtocolVersion => _inner.BinaryProtocolVersion;
    
    public ICluster Cluster => _inner.Cluster;

    public bool IsDisposed => _inner.IsDisposed;

    public string Keyspace => _inner.Keyspace;

    public UdtMappingDefinitions UserDefinedTypes => _inner.UserDefinedTypes;

    public string SessionName => _inner.SessionName;
}