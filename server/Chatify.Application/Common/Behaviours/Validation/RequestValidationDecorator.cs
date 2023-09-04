using Chatify.Application.Common.Exceptions;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Infrastructure;

namespace Chatify.Application.Common.Behaviours.Validation;

// TODO: Replace Exceptions with proper Result Type instead:
[Decorator]
public class RequestValidationDecorator<TCommand>(ICommandHandler<TCommand> inner) : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    public async Task HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validator.Validate(command);
        if ( validationErrors.Count > 0 )
        {
            throw new ModelValidationException(validationErrors);
        }

        await inner.HandleAsync(command, cancellationToken);
    }
}

[Decorator]
public class RequestValidationDecorator<TCommand, TResult>
    (ICommandHandler<TCommand, TResult> inner) : ICommandHandler<TCommand, TResult>
    where TCommand : class, ICommand<TResult>
{
    public async Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validator.Validate(command);
        if ( validationErrors.Count > 0 )
        {
            throw new ModelValidationException(validationErrors);
        }

        return await inner.HandleAsync(command, cancellationToken);
    }
}