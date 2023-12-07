namespace Chatify.Web.Middleware;

public sealed class SecureHeadersMiddleware : IMiddleware
{
    private static readonly Dictionary<string, string> SecureHeaders = new()
    {
        { "X-Frame-Options", "DENY" },
        { "X-Content-Type-Options", "nosniff" },
        { "X-Xss-Protection", "1; mode=block" },
        { "Referrer-Policy", "no-referrer" },
        { "Content-Security-Policy", "default-src 'self';" }
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        foreach ( var (name, value) in SecureHeaders )
        {
            context.Response.Headers.Add(name, value);
        }

        await next(context);
    }
}

public static class SecureHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecureHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecureHeadersMiddleware>();
}