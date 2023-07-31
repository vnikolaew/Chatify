using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Dispatchers;
using Chatify.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private IDispatcher? _dispatcher;

    protected IDispatcher Dispatcher
        => _dispatcher ??= HttpContext.RequestServices.GetRequiredService<IDispatcher>();

    protected Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
        => Dispatcher.SendAsync(command, cancellationToken);

    protected Task<TResult> SendAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>
        => Dispatcher.SendAsync<TCommand, TResult>(command, cancellationToken);

    protected Task QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>
        => Dispatcher.QueryAsync(query, cancellationToken);
}