using System.Net;
using Chatify.Shared.Abstractions.Dispatchers;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Web.Common;
using FastEndpoints;
using LanguageExt;
using Microsoft.AspNetCore.Identity;

namespace Chatify.Web.FastEndpoints_Features.Common;

public class BaseGroup : Group
{
    public const string ApiPrefix = "api";

    public BaseGroup()
        => Configure(ApiPrefix,
            ep => ep.Description(x => x.WithTags("base")));
}

public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    private IDispatcher? _dispatcher;

    public override void Configure()
    {
        AuthSchemes(IdentityConstants.ApplicationScheme);
        Group<BaseGroup>();
    }

    protected IDispatcher Dispatcher
        => _dispatcher ??= Resolve<IDispatcher>();

    protected Task SendAsync<TCommand>(TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand, Shared.Abstractions.Commands.ICommand =>
        Dispatcher.SendAsync(command, cancellationToken);

    protected Task<TResult> SendAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, Shared.Abstractions.Commands.ICommand<TResult>
        => Dispatcher.SendAsync<TCommand, TResult>(command, cancellationToken);

    protected Task<TResult> QueryAsync<TQuery, TResult>(TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>
        => Dispatcher.QueryAsync(query, cancellationToken);

    protected IResult Accepted(Unit _)
        => TypedResults.StatusCode(( int )HttpStatusCode.Accepted);

    protected IResult Accepted(string? message = default)
        => TypedResults.Accepted(string.Empty,
            ApiResponse<Unit>.Success(Unit.Default, message));

    protected IResult NoContent(Unit _)
        => TypedResults.NoContent();

    protected IResult Ok(Unit _) => TypedResults.Ok();

    protected static IResult Ok<T>(T data)
        where T : notnull
        => TypedResults.Ok(data);

    protected static object AsObject<T>(T value) => value!;
}