using Microsoft.AspNetCore.Mvc.Testing;

namespace Chatify.IntegrationTesting;

public class ChatifyWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    // private readonly ContainerBuilder _cassandraContainerBuilder =
    //     new ContainerBuilder()
    //         .WithImage("cassandra:latest")
    //         .WithResourceMapping(AppDomain.CurrentDomain.BaseDirectory)
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
        });
    }
}