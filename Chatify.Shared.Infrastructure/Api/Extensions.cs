﻿using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Api;

public static class Extensions
{
    public static T Bind<T>(this T model, Expression<Func<T, object>> expression, object value)
        => model.Bind<T, object>(expression, value);

    public static T BindId<T>(this T model, Expression<Func<T, string>> expression)
        => model.Bind(expression, Guid.NewGuid());

    private static TModel Bind<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression,
        object value)
    {
        var memberExpression = expression.Body as MemberExpression ??
                               ((UnaryExpression) expression.Body).Operand as MemberExpression;
        if (memberExpression is null)
        {
            return model;
        }

        var propertyName = memberExpression.Member.Name.ToLowerInvariant();
        var field = model?
            .GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>"));
        
        if (field is null) return model;

        field.SetValue(model, value);
        return model;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("cors");
        var corsOptions = section.GetOptions<CorsOptions>();
        
        services.Configure<CorsOptions>(section);
            
        return services
            .AddCors(cors =>
            {
                var allowedHeaders = corsOptions.AllowedHeaders ?? Enumerable.Empty<string>();
                var allowedMethods = corsOptions.AllowedMethods ?? Enumerable.Empty<string>();
                var allowedOrigins = corsOptions.AllowedOrigins ?? Enumerable.Empty<string>();
                var exposedHeaders = corsOptions.ExposedHeaders ?? Enumerable.Empty<string>();
                cors.AddPolicy("cors", corsBuilder =>
                {
                    var origins = allowedOrigins.ToArray();
                    
                    if (corsOptions.AllowCredentials && origins.FirstOrDefault() != "*")
                    {
                        corsBuilder.AllowCredentials();
                    }
                    else
                    {
                        corsBuilder.DisallowCredentials();
                    }

                    corsBuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        // .WithHeaders(allowedHeaders.ToArray())
                        // .WithMethods(allowedMethods.ToArray())
                        .WithOrigins(origins.ToArray());
                    // .WithExposedHeaders(exposedHeaders.ToArray());
                });
            });
    }
        
    public static string GetUserIpAddress(this HttpContext context)
    {
        if (context is null) return string.Empty;

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor))
            return ipAddress ?? string.Empty;
        
        var ipAddresses = forwardedFor.ToString().Split(",", StringSplitOptions.RemoveEmptyEntries);
        if (ipAddresses.Any()) ipAddress = ipAddresses[0];

        return ipAddress ?? string.Empty;
    }
}