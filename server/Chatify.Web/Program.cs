using System.Runtime.CompilerServices;
using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;
using Chatify.Web.Middleware;

[assembly: InternalsVisibleTo("Chatify.IntegrationTesting")]
var builder = WebApplication.CreateBuilder(args);
{
    // builder.WebHost.UseProductionHttps(builder.Environment);
    builder.Services
        .AddWebComponents(builder.Environment)
        .AddUserRateLimiting()
        .AddMappers()
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    app
        .UseHttpsRedirection()
        .UseConfiguredCors(app.Environment)
        .UseSecureHeaders()
        .UseCachedStaticFiles(app.Environment)
        .UseDevelopmentSwagger(app.Environment)
        .UseConfiguredCookiePolicy()
        .UseRouting()
        .UseRateLimiter()
        .UseAuthentication()
        .UseAuthorization()
        .UseContext()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints
                .MapNotifications()
                .MapFallback(() => TypedResults.NotFound());
        });

    app.Run();
}

public partial class Program
{
}