using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Domain.Entities;
using Chatify.Infrastructure;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

namespace Chatify.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
        => services.AddAutoMapper(config =>
        {
            config.AddMaps(
                typeof(IAssemblyMarker),
                typeof(Application.IAssemblyMarker));
            config.AllowNullDestinationValues = true;
        });

    public static IServiceCollection AddWebComponents(this IServiceCollection services)
    {
        services
            // .AddScoped<TraceIdentifierMiddleware>()
            .AddSingleton<IAuthorizationMiddlewareResultHandler,
                AuthorizationResultMiddlewareHandler>()
            .AddMappers()
            .AddCors()
            .AddProblemDetails(opts => { opts.CustomizeProblemDetails = _ => { }; })
            .AddConfiguredSwagger()
            .Configure<KestrelServerOptions>(opts => opts.AllowSynchronousIO = true)
            .AddControllers(opts =>
            {
                opts.Filters.Add<GlobalExceptionFilter>();
            })
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.Converters.Add(new IPAddressConverter());
                opts.JsonSerializerOptions.Converters.Add(new CursorPagedConverter<ChatGroupMessageEntry>());
            })
            .ConfigureApiBehaviorOptions(opts => opts.SuppressModelStateInvalidFilter = true);

        services.AddControllersWithViews();
        return services;
    }

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
                opts.SwaggerGeneratorOptions.Servers = new List<OpenApiServer>()
                {
                    new() { Url = "https://localhost:7139" }
                };
                var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
            });
}