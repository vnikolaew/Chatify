using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Common.Behaviours;

[Decorator]
public sealed class LoggingHandlerDecorator<TCommand>
    : ICommandHandler<TCommand> where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly ILogger<LoggingHandlerDecorator<TCommand>> _logger;
    private readonly IIdentityContext _identityContext;

    public LoggingHandlerDecorator(
        ICommandHandler<TCommand> inner,
        ILogger<LoggingHandlerDecorator<TCommand>> logger,
        IIdentityContext identityContext)
    {
        _inner = inner;
        _logger = logger;
        _identityContext = identityContext;
    }

    public async Task HandleAsync(
        TCommand command, CancellationToken cancellationToken = default)
    {
        Guid? userId = _identityContext.Id == Guid.Empty ? null : _identityContext.Id;
        _logger.LogInformation("Incoming request: {Request} by user with Id '{@UserId}'",
            typeof(TCommand).Name, userId?.ToString() ?? "null");

        await _inner.HandleAsync(command, cancellationToken);
    }
}

[Decorator]
public sealed class LoggingHandlerDecorator<TCommand, TResult>
    : ICommandHandler<TCommand, TResult> where TCommand : class, ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly ILogger<LoggingHandlerDecorator<TCommand, TResult>> _logger;
    private readonly IIdentityContext _identityContext;

    public LoggingHandlerDecorator(
        ICommandHandler<TCommand, TResult> inner,
        ILogger<LoggingHandlerDecorator<TCommand, TResult>> logger,
        IIdentityContext identityContext)
    {
        _inner = inner;
        _logger = logger;
        _identityContext = identityContext;
    }

    public async Task<TResult> HandleAsync(
        TCommand command, CancellationToken cancellationToken = default)
    {
        Guid? userId = _identityContext.Id == Guid.Empty ? null : _identityContext.Id;
        _logger.LogInformation("Incoming request: {Request} by user with Id '{@UserId}'",
            typeof(TCommand).Name, userId?.ToString() ?? "null");

        return await _inner.HandleAsync(command, cancellationToken);
    }
}