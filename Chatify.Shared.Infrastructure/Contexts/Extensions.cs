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
            .AddScoped<IIdentityContext, IdentityContext>(sp =>
                new IdentityContext(sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User))
            .AddTransient(sp => sp.GetRequiredService<ContextAccessor>().Context);

    public static IApplicationBuilder UseContext(this IApplicationBuilder app)
        => app.Use((ctx, next) =>
        {
            ctx.RequestServices.GetRequiredService<ContextAccessor>().Context = new Context(ctx);
            return next();
        });
}