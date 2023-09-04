using System.Net;
using Chatify.Shared.Abstractions.Dispatchers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Chatify.IntegrationTesting;

public class BasicTest : IClassFixture<ChatifyWebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _testOutputHelper;

    public BasicTest(WebApplicationFactory<Program> factory,
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
    public async Task Test1()
    {
        _testOutputHelper.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        var responseMessage = await _httpClient.GetAsync("/not-existing");
        Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode );
    }
}