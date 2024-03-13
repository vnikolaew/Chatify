using Chatify.Infrastructure.Services.External.ChatGroupsService;
using Chatify.Infrastructure.Services.External.Common;
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
        services.AddGrpcClient<ChatGroupsServicer.ChatGroupsServicerClient>(opts =>
            {
                opts.Address = new Uri(configuration["Services:ChatGroups"]!);
            }).AddInterceptor<TokenAuthInterceptor>(InterceptorScope.Client)
            .ConfigureChannel(opts => { opts.Credentials = ChannelCredentials.SecureSsl; });

        return services;
    }
}