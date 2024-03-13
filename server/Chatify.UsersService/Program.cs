using System.IO.Compression;
using Chatify.Application.Common.Contracts;
using Chatify.Infrastructure;
using Chatify.Infrastructure.Common;
using Chatify.Infrastructure.Data;
using Chatify.Infrastructure.Data.Repositories;
using Chatify.Infrastructure.Services.External.Common;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Contexts;
using Chatify.Shared.Infrastructure.Time;
using Chatify.UsersService.Services;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGrpc(opts =>
    {
        opts.EnableDetailedErrors = true;
        opts.ResponseCompressionLevel = CompressionLevel.Optimal;
        opts.Interceptors.Add<LoggingInterceptor>();
    });
    builder.Services.AddGrpcReflection();
    builder.Services
        .AddContexts()
        .AddCassandraIdentity(builder.Configuration)
        .AddJwtBearer(builder.Configuration);

    builder.Services
        .AddCassandra(builder.Configuration)
        .AddMappers()
        .AddRepositories()
        .AddCaching(builder.Configuration)
        .AddHostedService<DatabaseInitializationService>()
        .AddSingleton<IClock, UtcClock>()
        .AddTransient<IPagingCursorHelper, CassandraPagingCursorHelper>()
        .AddSingleton<IGuidGenerator, TimeUuidGenerator>();
}
// Add services to the container.
builder.Services.AddGrpc();

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

    app.MapGrpcService<UsersServicer>();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Logger.LogInformation("Running as host {HostName}", Environment.MachineName);
    app.Run();
}