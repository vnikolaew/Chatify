using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Chatify.Application.Common.Exceptions;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Infrastructure;

namespace Chatify.Application.Common.Behaviours;

// TODO: Replace Exceptions with proper Result Type instead:
[Decorator]
public class RequestValidationDecorator<TCommand>
    : ICommandHandler<TCommand> where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _inner;

    public RequestValidationDecorator(ICommandHandler<TCommand> inner)
        => _inner = inner;

    public async Task HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            throw new ModelValidationException(validationErrors);
        }

        await _inner.HandleAsync(command, cancellationToken);
    }

    private static List<ValidationResult> Validate(TCommand command)
    {
        var properties = typeof(TCommand)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var validationContext = new ValidationContext(command);

        List<ValidationResult> validationResults = new();
        foreach (var property in properties)
        {
            var validationAttributes = property
                .GetCustomAttributes()
                .OfType<ValidationAttribute>()
                .ToArray();

            var propertyValue = property.GetValue(command);
            var validations = validationAttributes
                .Select(va => va.GetValidationResult(propertyValue, validationContext)!)
                .ToArray();

            validationResults.AddRange(validations);
        }

        var validationErrors = validationResults
            .Where(r => r != ValidationResult.Success)
            .ToList();

        return validationErrors;
    }
}

[Decorator]
public class RequestValidationDecorator<TCommand, TResult>
    : ICommandHandler<TCommand, TResult> where TCommand : class, ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;

    public RequestValidationDecorator(ICommandHandler<TCommand, TResult> inner)
        => _inner = inner;

    public async Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            throw new ModelValidationException(validationErrors);
        }

        return await _inner.HandleAsync(command, cancellationToken);
    }

    private static List<ValidationResult> Validate(TCommand command)
    {
        var properties = typeof(TCommand)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var validationContext = new ValidationContext(command);

        List<ValidationResult> validationResults = new();
        foreach (var property in properties)
        {
            var validationAttributes = property
                .GetCustomAttributes()
                .OfType<ValidationAttribute>()
                .ToArray();

            var propertyValue = property.GetValue(command);
            var validations = validationAttributes
                .Select(va => va.GetValidationResult(propertyValue, validationContext)!)
                .ToArray();

            validationResults.AddRange(validations);
        }

        var validationErrors = validationResults
            .Where(r => r != ValidationResult.Success)
            .ToList();

        return validationErrors;
    }
}