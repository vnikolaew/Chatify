using System.Reflection;
using Chatify.Shared.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Chatify.Shared.Infrastructure.Commands;

public static class Extensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        => services
            .AddSingleton<ICommandDispatcher, CommandDispatcher>()
            .Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c =>
                    c.AssignableToAny(typeof(ICommandHandler<>), typeof(ICommandHandler<,>))
                        .WithoutAttribute<DecoratorAttribute>(), publicOnly: false)
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
}