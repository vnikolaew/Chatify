namespace Chatify.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    private const string HttpPortEnvVariableName = "ASPNETCORE_HTTP_PORT";
    private const string HttpsPortEnvVariableName = "ASPNETCORE_HTTPS_PORT";

    public static IWebHostBuilder UseProductionHttps(
        this IWebHostBuilder webHostBuilder,
        IWebHostEnvironment environment)
    {
        if ( !environment.IsProduction() ) return webHostBuilder;
        var httpPort = int.TryParse(
            Environment.GetEnvironmentVariable(HttpPortEnvVariableName), out var port)
            ? port : 80;
        var httpsPort = int.TryParse(
            Environment.GetEnvironmentVariable(HttpsPortEnvVariableName), out var port2)
            ? port2 : 443;

        var pfxFilePath = Path.Combine("certs", "myapp.pfx");
        return webHostBuilder.UseKestrel((_, opts) =>
        {
            opts.ListenLocalhost(httpPort);
            opts.ListenLocalhost(httpsPort, opts =>
                opts.UseHttps(pfxFilePath, "changeit"));
        });
    }
}