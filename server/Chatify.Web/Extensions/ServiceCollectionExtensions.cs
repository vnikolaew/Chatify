using System.Reflection;
using System.Text.Json.Serialization;
using Chatify.Infrastructure;
using Chatify.Web.Middleware;
using Microsoft.OpenApi.Models;

namespace Chatify.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
        => services.AddAutoMapper(config =>
            config.AddMaps(
                typeof(IAssemblyMarker),
                typeof(Application.IAssemblyMarker)));

    public static IServiceCollection AddWebComponents(this IServiceCollection services)
    {
        services
            // .AddScoped<TraceIdentifierMiddleware>()
            .AddControllers(opts => opts.Filters.Add<GlobalExceptionFilter>())
            .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(opts => opts.SuppressModelStateInvalidFilter = true);
        
        services.AddControllersWithViews();
        return services;
    }

    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
        => services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Chatify Server",
                });
                opts.SwaggerGeneratorOptions.Servers = new List<OpenApiServer>()
                {
                    new OpenApiServer() { Url = "https://localhost:7139"}
                };
                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
            });
}