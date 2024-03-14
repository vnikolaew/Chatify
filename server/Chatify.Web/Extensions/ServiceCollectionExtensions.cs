using System.Collections.Immutable;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Domain.Entities;
using Chatify.Infrastructure;
using Chatify.Infrastructure.Data.Conversions;
using Chatify.Web.Common;
using Chatify.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

namespace Chatify.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebComponents(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services
            .AddSingleton<IAuthorizationMiddlewareResultHandler,
                AuthorizationResultMiddlewareHandler>()
            .AddUserRateLimiting()
            .AddMappers()
            .AddSingleton<SecureHeadersMiddleware>()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddScoped<IUrlHelper>(x =>
            {
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext!);
            })
            .AddCors()
            .AddProblemDetails(opts => { opts.CustomizeProblemDetails = _ => { }; })
            .AddConfiguredSwagger()
            .Configure<KestrelServerOptions>(opts => opts.AllowSynchronousIO = true)
            .AddControllers(opts =>
            {
                opts.Filters.Add<GlobalExceptionFilter>();
                if ( environment.IsProduction() )
                {
                    opts.Filters.Add<ActivitySourceActionFilter>();
                }
            })
            .AddJsonOptions(ConfigureJsonOptions)
            .ConfigureApiBehaviorOptions(opts => opts.SuppressModelStateInvalidFilter = true);

        return services;
    }

    private static void ConfigureJsonOptions(JsonOptions opts)
    {
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        opts.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();

        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.Converters.Add(new IPAddressConverter());
        opts.JsonSerializerOptions.Converters.Add(new CursorPagedConverter<ChatGroupMessageEntry>());
        opts.JsonSerializerOptions.Converters.Add(new CursorPagedConverter<UserNotification>());
        opts.JsonSerializerOptions.Converters.Add(new CursorPagedConverter<ChatGroupAttachment>());
        opts.JsonSerializerOptions.Converters.Add(new CursorPagedConverter<ChatMessageReply>());
    }

    public static IServiceCollection AddUserRateLimiting(this IServiceCollection services)
        => services
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
            });

    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
        => services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Chatify Server",
                });
                opts.SelectSubTypesUsing(baseType =>
                {
                    if ( baseType == typeof(UserNotification) )
                    {
                        return new[]
                        {
                            typeof(UserNotification),
                            typeof(IncomingFriendInvitationNotification),
                        };
                    }

                    return ImmutableArray<Type>.Empty;
                });
                opts.SwaggerGeneratorOptions.Servers = [new OpenApiServer { Url = "https://localhost:7139" }];
            });
}