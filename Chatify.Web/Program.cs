using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddWebComponents()
        .AddMappers()
        .AddInfrastructure(builder.Configuration)
        .AddApplication()
        .AddConfiguredSwagger();
}

var app = builder.Build();
{
    app
        .UseDevelopmentSwagger(app.Environment)
        .UseHttpsRedirection()
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseContext()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallback(() => TypedResults.NotFound());
        });

    app.Run();
}