using System.Reflection;
using Chatify.Application.Common.Behaviours;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Common.Behaviours.Validation;
using Chatify.Application.Messages.Common;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Dispatchers;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Commands;
using Chatify.Shared.Infrastructure.Dispatchers;
using Chatify.Shared.Infrastructure.Events;
using Chatify.Shared.Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;
using static Chatify.Shared.Infrastructure.Events.Extensions;

namespace Chatify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assemblies = new[] { Assembly.GetExecutingAssembly() };
        services
            .AddCommands(assemblies)
            .AddQueries(assemblies)
            .AddEvents(assemblies, EventDispatcherType.FireAndForget)
            .AddScoped<AttachmentOperationHandler>()
            .AddSingleton<IDispatcher, InMemoryDispatcher>();

        services.TryDecorate(typeof(ICommandHandler<>), typeof(RequestValidationDecorator<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(RequestValidationDecorator<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingHandlerDecorator<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingHandlerDecorator<,>));
        
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(TimedHandlerDecorator<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(CachedQueryHandlerDecorator<,>));
        return services;
    }
}