using System.IO.Compression;
using Chatify.Application.Common.Contracts;
using Chatify.ChatGroupsService.Extensions;
using Chatify.Infrastructure;
using Chatify.Infrastructure.Common;
using Chatify.Infrastructure.Data;
using Chatify.Infrastructure.Data.Repositories;
using Chatify.Infrastructure.Services.External.Common;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Shared.Infrastructure.Time;
using ChatGroupsServicer = Chatify.ChatGroupsService.Services.ChatGroupsServicer;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGrpc(opts =>
    {
        opts.EnableDetailedErrors = true;
        opts.ResponseCompressionLevel = CompressionLevel.Optimal;
        opts.Interceptors.Add<LoggingInterceptor>();
    });
    
    builder.Services
        .AddGrpcReflection()
        .AddMappings()
        .AddContexts()
        .AddCassandraIdentity(builder.Configuration)
        .AddJwtBearer(builder.Configuration)
        .AddCassandra(builder.Configuration)
        .AddRepositories()
        .AddCaching(builder.Configuration)
        .AddHostedService<DatabaseInitializationService>()
        .AddSingleton<IClock, UtcClock>()
        .AddTransient<IPagingCursorHelper, CassandraPagingCursorHelper>()
        .AddSingleton<IGuidGenerator, TimeUuidGenerator>();
}

var app = builder.Build();
{
    app
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseContext();

    if ( app.Environment.IsDevelopment() )
    {
        app.MapGrpcReflectionService();
    }

    app.MapGrpcService<ChatGroupsServicer>();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Logger.LogInformation("Running as host {HostName}", Environment.MachineName);
    app.Run();
}