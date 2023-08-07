namespace Chatify.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseUrls(this WebApplicationBuilder app,
        params string[] urls)
    {
        app.WebHost.UseUrls(urls);
        return app;
    }
}