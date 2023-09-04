using Chatify.Application.Common.Models;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Infrastructure;
using Microsoft.Extensions.Logging;
using Guid = System.Guid;

namespace Chatify.Application.Common.Behaviours;

[Decorator]
public sealed class LoggingHandlerDecorator<TCommand>(ICommandHandler<TCommand> inner,
        ILogger<LoggingHandlerDecorator<TCommand>> logger,
        IIdentityContext identityContext)
    : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    public async Task HandleAsync(
        TCommand command, CancellationToken cancellationToken = default)
    {
        Guid? userId = identityContext.Id == Guid.Empty ? null : identityContext.Id;
        logger.LogInformation("Incoming request: {Request} by user with Id '{@UserId}'",
            typeof(TCommand).Name, userId?.ToString() ?? "null");

        await inner.HandleAsync(command, cancellationToken);
    }
}

[Decorator]
public sealed class LoggingHandlerDecorator<TCommand, TResult>(ICommandHandler<TCommand, TResult> inner,
        ILogger<LoggingHandlerDecorator<TCommand, TResult>> logger,
        IIdentityContext identityContext)
    : ICommandHandler<TCommand, TResult>
    where TCommand : class, ICommand<TResult>
{
    public async Task<TResult> HandleAsync(
        TCommand command, CancellationToken cancellationToken = default)
    {
        Guid? userId = identityContext.Id == Guid.Empty ? null : identityContext.Id;
        logger.LogInformation("Incoming request: {Request} by user with Id '{@UserId}'",
            typeof(TCommand).Name, userId?.ToString() ?? "null");

        var result = await inner.HandleAsync(command, cancellationToken);

        if ( result is BaseError baseError )
        {
            logger.LogInformation("Encountered an application error of type '{ErrorType'}: {ErrorMessage}",
                typeof(TResult).Name, baseError.Message);
        }

        return result;
    }
}