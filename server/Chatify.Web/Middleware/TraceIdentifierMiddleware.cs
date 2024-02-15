using Chatify.Shared.Abstractions.Contexts;

namespace Chatify.Web.Middleware;

internal sealed class TraceIdentifierMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);
        if (!context.Response.HasStarted)
        {
            var traceIdentifier = context
                .RequestServices
                .GetRequiredService<IContext>()
                .TraceId;
            
            context.Response.Headers["X-Trace-Id"] = traceIdentifier;
        }
    }
}

internal static class TraceIdentifierMiddlewareExtensions
{
    public static IApplicationBuilder UseTraceIdentifier(this IApplicationBuilder app)
        => app.UseMiddleware<TraceIdentifierMiddleware>();
}
