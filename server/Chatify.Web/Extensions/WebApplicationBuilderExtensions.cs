namespace Chatify.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseUrls(
        this WebApplicationBuilder app,
        params string[] urls)
    {
        app.WebHost.UseUrls(urls);
        return app;
    }
    
    public static IWebHostBuilder UseProductionHttps(
        this IWebHostBuilder webHostBuilder,
        IWebHostEnvironment environment,
        int httpPort = 5289,
        int httpsPort = 7139
    )
    {
        // if ( !environment.IsProduction() ) return webHostBuilder;

        var pfxFilePath = Path.Combine("certs", "myapp.pfx");
        return webHostBuilder.UseKestrel((_, opts) =>
        {
            opts.ListenLocalhost(httpPort);
            opts.ListenLocalhost(httpsPort, opts =>
                opts.UseHttps(pfxFilePath, "changeit"));
        });
    }
}