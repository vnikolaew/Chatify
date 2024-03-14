using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.User.Contracts;
using Chatify.Infrastructure.Services.External.Common;
using Chatify.Services.Shared.ChatGroups;
using Chatify.Services.Shared.Users;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Services.External;

public static class DependencyInjection
{
    public static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddTransient<TokenAuthInterceptor>()
            .AddGrpcClient<ChatGroupsServicer.ChatGroupsServicerClient>(opts =>
            {
                opts.Address = new Uri(configuration["Services:ChatGroups"]!);
            }).AddInterceptor<TokenAuthInterceptor>(InterceptorScope.Client)
            .ConfigureChannel(opts => { opts.Credentials = ChannelCredentials.SecureSsl; });

        services
            .AddGrpcClient<UsersServicer.UsersServicerClient>(opts =>
            {
                opts.Address = new Uri(configuration["Services:Users"]!);
            }).AddInterceptor<TokenAuthInterceptor>(InterceptorScope.Client)
            .ConfigureChannel(opts => { opts.Credentials = ChannelCredentials.SecureSsl; });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<IChatGroupsService, ChatGroups.ChatGroupsService>()
            .AddScoped<IUsersService, Users.UsersService>();

    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddServices()
            .AddGrpcClients(configuration);
}