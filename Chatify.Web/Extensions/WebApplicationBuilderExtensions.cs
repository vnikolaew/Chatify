namespace Chatify.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseUrls(this WebApplicationBuilder app, params string[] urls)
    {
        app.WebHost.UseUrls(
            "http://0.0.0.0:5289",
            "https://0.0.0.0:7139"
        );
        
        return app;
    }
}