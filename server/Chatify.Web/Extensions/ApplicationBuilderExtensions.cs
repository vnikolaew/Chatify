using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Chatify.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    private const string StaticFilesDirectoryName = "Files";
    private const string ConsentCookieName = "Cookie-Consent";

    public static IApplicationBuilder UseDevelopmentSwagger(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if ( environment.IsDevelopment() )
        {
            app.UseSwagger()
                .UseSwaggerUI();
        }

        return app;
    }

    public static IApplicationBuilder UseConfiguredCors(
        this IApplicationBuilder app)
        => app.UseCors(policy =>
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:4200/",
                    "https://localhost:3000",
                    "https://localhost:3000/",
                    "http://localhost:3000"
                    )
                .AllowCredentials());

    public static IApplicationBuilder UseConfiguredCookiePolicy(
        this IApplicationBuilder app)
        => app
            .UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.None,
                ConsentCookieValue = true.ToString(),
                CheckConsentNeeded = _ => true,
                ConsentCookie = new CookieBuilder
                {
                    Name = ConsentCookieName,
                    Expiration = TimeSpan.FromHours(24 * 30 * 6),
                    MaxAge = TimeSpan.FromHours(24 * 30 * 6),
                    HttpOnly = false,
                    SameSite = SameSiteMode.None,
                    IsEssential = true,
                    SecurePolicy = CookieSecurePolicy.Always
                }
            });

    public static IApplicationBuilder UseCachedStaticFiles(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        string? path = "/static")
        => app.UseStaticFiles(new StaticFileOptions
        {
            HttpsCompression = HttpsCompressionMode.Compress,
            FileProvider =
                new CompositeFileProvider(
                    new PhysicalFileProvider(
                        Path.Combine(environment.ContentRootPath, StaticFilesDirectoryName)),
                    new PhysicalFileProvider(Path.Combine(environment.ContentRootPath, "swagger"))),
            RequestPath = path,
            ServeUnknownFileTypes = true,
            ContentTypeProvider = new FileExtensionContentTypeProvider(),
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromDays(30),
                    Public = true
                };
                headers.Expires = DateTimeOffset.Now.AddDays(30);
            }
        });
}