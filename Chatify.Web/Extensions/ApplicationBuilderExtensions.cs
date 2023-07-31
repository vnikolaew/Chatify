namespace Chatify.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDevelopmentSwagger(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger()
                .UseSwaggerUI();
        }

        return app;
    }
}