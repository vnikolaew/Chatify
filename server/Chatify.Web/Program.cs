using System.Runtime.CompilerServices;
using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;

[assembly: InternalsVisibleTo("Chatify.IntegrationTesting")]
var builder = WebApplication.CreateBuilder(args);
{
    var urls = builder.Configuration.GetValue<string[]>("Urls");
    if(urls is not null) builder.WebHost.UseUrls(urls);
    
    builder
        .WebHost
        .UseProductionHttps(builder.Environment);
    
    builder
        .Services
        .AddWebComponents(builder.Environment)
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    app
        .UseWebInfrastructure(app.Environment)
        .UseRouting()
        .UseRateLimiter()
        .UseAuthentication()
        .UseAuthorization()
        .UseContext()
        .UseAppEndpoints();

    app.Run();
}

public partial class Program
{
}