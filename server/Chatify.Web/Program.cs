using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Chatify.Application;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Web.Common;
using Chatify.Web.Extensions;

[assembly: InternalsVisibleTo("Chatify.IntegrationTesting")]
var builder = WebApplication.CreateBuilder(args);
{
    builder.UseUrls(
        "http://0.0.0.0:5289",
        "https://0.0.0.0:7139"
    );
    builder.Services
        .AddRateLimiter(opts =>
        {
            opts.AddPolicy(ApiController.DefaultUserRateLimitPolicy,
                ctx =>
                {
                    if ( ctx.User.Identity?.IsAuthenticated is false )
                    {
                        return RateLimitPartition.GetNoLimiter(string.Empty);
                    }

                    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    return RateLimitPartition.GetSlidingWindowLimiter(userId,
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            QueueLimit = 20,
                            QueueProcessingOrder = QueueProcessingOrder.NewestFirst,
                            Window = TimeSpan.FromSeconds(10)
                        });
                });
            opts.OnRejected = (ctx,
                ct) =>
            {
                if ( ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) )
                {
                    ctx.HttpContext.Response.Headers.RetryAfter =
                        ( ( int )retryAfter.TotalSeconds ).ToString(NumberFormatInfo.InvariantInfo);
                }

                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return ValueTask.CompletedTask;
            };

            opts.RejectionStatusCode = ( int )HttpStatusCode.TooManyRequests;
        })
        .AddWebComponents()
        .AddMappers()
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    app
        .UseHttpsRedirection()
        .UseConfiguredCors()
        .UseCachedStaticFiles(app.Environment, "/static")
        .UseDevelopmentSwagger(app.Environment)
        .UseConfiguredCookiePolicy()
        .UseRouting()
        .UseRateLimiter()
        .UseAuthentication()
        .UseAuthorization()
        .UseContext()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints
                .MapNotifications()
                .MapFallback(() => TypedResults.NotFound());
        });

    app.Run();
}

public partial class Program
{
}