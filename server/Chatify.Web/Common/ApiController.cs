using System.Net.Mime;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Dispatchers;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Common;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
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

    protected Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>
        => Dispatcher.QueryAsync(query, cancellationToken);

    protected IActionResult Accepted(Unit _) => Accepted();

    protected new IActionResult Accepted(string? message = default)
        => Accepted(ApiResponse<Unit>.Success(Unit.Default, message));

    protected IActionResult NoContent(Unit _) => NoContent();

    protected IActionResult Ok(Unit _) => Ok();

    protected static IActionResult Ok<T>(T data)
        where T : notnull
        => new OkObjectResult(ApiResponse<T>.Success(data));

    protected static object AsObject<T>(T value) => value!;
}