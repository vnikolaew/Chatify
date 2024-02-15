using Microsoft.Net.Http.Headers;

namespace Chatify.Web.Middleware;

public sealed class SecureHeadersMiddleware : IMiddleware
{
    private static readonly Dictionary<string, string> SecureHeaders = new()
    {
        { HeaderNames.XFrameOptions, "DENY" },
        { HeaderNames.XContentTypeOptions, "nosniff" },
        { HeaderNames.XXSSProtection, "1; mode=block" },
        { "Referrer-Policy", "no-referrer" },
        { HeaderNames.ContentSecurityPolicy, "default-src 'self';" }
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        foreach ( var (name, value) in SecureHeaders )
        {
            context.Response.Headers.Append(name, value);
        }

        await next(context);
    }
}

public static class SecureHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecureHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecureHeadersMiddleware>();
}