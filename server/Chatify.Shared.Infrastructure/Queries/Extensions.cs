using System.Reflection;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Queries.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Shared.Infrastructure.Queries;

public static class Extensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        => services
            .AddSingleton<IQueryDispatcher, QueryDispatcher>()
            .Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute<DecoratorAttribute>(), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

    public static IServiceCollection AddPagedQueryDecorator(this IServiceCollection services)
    {
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(PagedQueryHandlerDecorator<,>));
        return services;
    }
}