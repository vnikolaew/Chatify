using Chatify.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Contexts;

public static class Extensions
{
    public static IServiceCollection AddContexts(this IServiceCollection services)
        => services
            .AddSingleton<ContextAccessor>()
            .AddSingleton<ContextMiddleware>()
            .AddHttpContextAccessor()
            .AddScoped<IIdentityContext, IdentityContext>(sp =>
                new IdentityContext(sp.GetRequiredService<IHttpContextAccessor>().HttpContext!))
            .AddTransient(sp => sp.GetRequiredService<ContextAccessor>().Context);

    public static IApplicationBuilder UseContext(this IApplicationBuilder app)
        => app.UseMiddleware<ContextMiddleware>();

    public sealed class ContextMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.RequestServices.GetRequiredService<ContextAccessor>().Context = new Context(context);
            return next(context);
        }
    }
}