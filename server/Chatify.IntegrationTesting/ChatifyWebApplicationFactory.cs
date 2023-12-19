using AspNetCore.Identity.Cassandra;
using Cassandra;
using Chatify.Infrastructure.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IMapper = Cassandra.Mapping.IMapper;
using ISession = Cassandra.ISession;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.IntegrationTesting;

public class ChatifyWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private const int DbPort = 9043;
    
    private readonly IContainer _cassandraContainerBuilder =
        new ContainerBuilder()
            .WithImage("cassandra:latest")
            .WithName("test-containers-cassandra")
            .WithAutoRemove(true)
            .WithPortBinding(9045, DbPort)
            .WithResourceMapping(
                new FileInfo(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "cassandra", "cassandra.yaml")),
                new FileInfo("/etc/cassandra/cassandra.yaml"))
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9043))
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context,
            configurationBuilder) =>
        {
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(ISession));
            services.RemoveAll(typeof(IMapper));
            var s = services.ToList();

            services.Configure<CassandraOptions>(opts =>
            {
                opts.ContactPoints.Add(_cassandraContainerBuilder.Hostname);
                opts.Credentials = new CassandraCredentials
                {
                    UserName = "Cassandra",
                    Password = "Cassandra",
                };
                opts.Port = _cassandraContainerBuilder.GetMappedPublicPort(DbPort);
                opts.Query = new CassandraQueryOptions
                {
                    ConsistencyLevel = ConsistencyLevel.One,
                    PageSize = 25
                };
                opts.KeyspaceName = "chatify";
                opts.RetryCount = 3;
            });

            services
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<ISession>()))
                .AddSingleton<ISession>(sp =>
                {
                    var cluster = Cluster.Builder()
                        .AddContactPoints(
                            $"{_cassandraContainerBuilder.Hostname}")
                        .WithPort(_cassandraContainerBuilder.GetMappedPublicPort(DbPort))
                        .WithTypeSerializers(DependencyInjection.GetTypeSerializerDefinitions())
                        .Build();

                    ISession session = cluster.Connect();
                    return session;
                });
        });

        builder.ConfigureServices(services => { });
    }

    public async Task InitializeAsync()
    {
        await _cassandraContainerBuilder.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _cassandraContainerBuilder.StopAsync();
    }
}