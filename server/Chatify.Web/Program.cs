using System.Runtime.CompilerServices;
using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;
using Chatify.Web.Middleware;

[assembly: InternalsVisibleTo("Chatify.IntegrationTesting")]
var builder = WebApplication.CreateBuilder(args);
{
    builder.UseUrls(
        "http://0.0.0.0:5289",
        "https://0.0.0.0:7139"
    );
    builder.Services
        .AddSingleton<SecureHeadersMiddleware>()
        .AddWebComponents()
        .AddUserRateLimiting()
        .AddMappers()
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    app
        .UseHttpsRedirection()
        .UseConfiguredCors()
        // .UseSecureHeaders()
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