using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Infrastructure.Authentication;
using Chatify.Infrastructure.Authentication.External.Facebook;
using Chatify.Infrastructure.Authentication.External.Google;
using Chatify.Infrastructure.Data;
using Chatify.Infrastructure.Mailing;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Shared.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Chatify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddData(configuration)
            .AddRepositories()
            .AddServices()
            .AddContexts()
            .AddSingleton<IClock, UtcClock>()
            .AddHttpContextAccessor();

    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddAuthenticationServices()
            .AddTransient<IEmailSender, NullEmailSender>();

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var repositoryTypes = Assembly
            .GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainRepository<,>)))
            .ToList();
            
        foreach (var repositoryType in repositoryTypes)
        {
            services.AddScoped(repositoryType.ImplementedInterfaces.First(), repositoryType);
        }

        return services;
        // return services
        //     .Scan(s => s.FromExecutingAssembly()
        //         .AddClasses(t =>
        //             t.Where(t => t is { IsAbstract: false, IsInterface: false }
        //                          && t.GetInterfaces().Any(i =>
        //                              i.IsGenericType && i.GetGenericTypeDefinition()
        //                              == typeof(IDomainRepository<,>))), false)
        //         .AsImplementedInterfaces()
        //         .WithScopedLifetime());
    }

    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services)
    {
        services.AddHttpClient<IGoogleOAuthClient, GoogleOAuthClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/oauth2/v2/userinfo?alt=json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }).AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IFacebookOAuthClient, FacebookOAuthClient>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }).AddPolicyHandler(GetRetryPolicy());

        return services
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IEmailConfirmationService, EmailConfirmationService>();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}