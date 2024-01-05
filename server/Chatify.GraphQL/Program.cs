using AutoMapper.Extensions.ExpressionMapping;
using Chatify.Application;
using Chatify.GraphQL;
using Chatify.GraphQL.Queries;
using Chatify.Infrastructure;
using Chatify.Shared.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddAutoMapper(config =>
    {
        config
            .AddExpressionMapping()
            .AddMaps(
                typeof(Chatify.Infrastructure.IAssemblyMarker),
                typeof(Chatify.Application.IAssemblyMarker));
        config.AllowNullDestinationValues = true;
    })
    .AddHttpContextAccessor()
    .AddGraphQLServer()
    .AddErrorFilter<ChatifyErrorFilter>()
    .AddQueryType<Query>();

var app = builder.Build();

app
    .UseHttpsRedirection()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseContext()
    .UseEndpoints(_ => { _.MapGraphQL(); });

app.Run();