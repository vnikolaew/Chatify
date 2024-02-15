using System.Net;
using Chatify.Application.Common;
using Chatify.Shared.Abstractions.Dispatchers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Chatify.IntegrationTesting;

public sealed class BasicTests : IClassFixture<ChatifyWebApplicationFactory<Program>>
{
    private readonly ChatifyWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _testOutputHelper;

    public BasicTests(
        ChatifyWebApplicationFactory<Program> factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public void RunTest()
    {
        Assert.Equal(4 , 2 + 2);
    }
    
    [Fact]
    public async Task PingTest()
    {
        var dispatcher = _factory.Services.GetRequiredService<IDispatcher>();
        var result = await dispatcher.QueryAsync(new Ping());
        
        Assert.Equal("pong", result.Value);
    }

    [Fact]
    public async Task Test1()
    {
        _testOutputHelper.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        var responseMessage = await _httpClient.GetAsync("/not-existing");
        Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode );
    }
}