using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;
using Chatify.Web.Middleware;

var builder = WebApplication.CreateBuilder(args).UseUrls(
    "http://0.0.0.0:5289",
    "https://0.0.0.0:7139"
);
{
    builder.Services
        .AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
            };
        })
        .AddWebComponents()
        .AddMappers()
        .AddInfrastructure(builder.Configuration)
        .AddApplication()
        .AddConfiguredSwagger();
}

var app = builder.Build();
{
    app
        .UseHttpsRedirection()
        .UseTraceIdentifierMiddleware()
        .UseDevelopmentSwagger(app.Environment)
        .UseCachedStaticFiles(app.Environment, "/static")
        .UseRouting()
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