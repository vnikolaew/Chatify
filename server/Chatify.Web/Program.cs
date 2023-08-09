using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
{
    builder.UseUrls(
        "http://0.0.0.0:5289",
        "https://0.0.0.0:7139"
    );
    builder.Services
        .AddCors()
        .AddProblemDetails(opts => { opts.CustomizeProblemDetails = _ => { }; })
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
        .UseCors(policy =>
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:4200")
                .AllowCredentials())
        // .UseTraceIdentifierMiddleware()
        .UseCachedStaticFiles(app.Environment, "/static")
        .UseDevelopmentSwagger(app.Environment)
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