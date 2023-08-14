using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Chatify.Shared.Infrastructure;

public static class Extensions
{
    private const string AppSectionName = "app";
    private const string CorrelationIdKey = "correlation-id";

    public static AppOptions GetAppOptions(this IConfiguration configuration)
        => configuration.GetOptions<AppOptions>(AppSectionName);
    
    public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
        => configuration.GetSection(sectionName).GetOptions<T>();
    
    public static T GetOptions<T>(this IConfiguration configuration) where T : new()
        => configuration.GetSection(typeof(T).Name).GetOptions<T>();
        
    public static T GetOptions<T>(this IConfigurationSection section) where T : new()
    {
        var options = new T();
        section.Bind(options);
        return options;
    }
    private static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.Use((ctx, next) =>
        {
            ctx.Items.Add(CorrelationIdKey, Guid.NewGuid());
            return next();
        });
        
    public static Guid? TryGetCorrelationId(this HttpContext context)
        => context.Items.TryGetValue(CorrelationIdKey, out var id) ? (Guid) id! : default;
}