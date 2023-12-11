using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chatify.IntegrationTesting.NUnit;

[TestFixture]
public class BasicTest
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public BasicTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.AreEqual(1 + 1, 2);
    }
    
    [Test]
    public async Task Test2()
    {
        var responseMessage = await _httpClient.GetAsync("/not-existing");
        Assert.AreEqual(HttpStatusCode.NotFound, responseMessage.StatusCode );
    }
}