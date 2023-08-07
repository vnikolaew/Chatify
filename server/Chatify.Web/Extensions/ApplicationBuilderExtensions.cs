using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Chatify.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    private const string StaticFilesDirectoryName = "Files";

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

    public static IApplicationBuilder UseCachedStaticFiles(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        PathString path)
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