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
        IWebHostEnvironment environment)
    {
        if ( !environment.IsProduction() ) return webHostBuilder;
        var httpPort = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS"), out var port)
            ? port
            : 80;
        var httpsPort = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS"), out var port2)
            ? port2
            : 443;


        var pfxFilePath = Path.Combine("certs", "myapp.pfx");
        return webHostBuilder.UseKestrel((_, opts) =>
        {
            opts.ListenLocalhost(httpPort);
            opts.ListenLocalhost(httpsPort, opts =>
                opts.UseHttps(pfxFilePath, "changeit"));
        });
    }
}