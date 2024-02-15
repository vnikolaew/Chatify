using Chatify.Shared.Abstractions.Queries;
using OneOf.Types;

namespace Chatify.Application.Common;

public record Ping : IQuery<Result<string>>;

public sealed class PingHandler() : IQueryHandler<Ping, Result<string>>
{
    public Task<Result<string>> HandleAsync(
        Ping _,
        CancellationToken cancellationToken = default)
    {
        var result = new Result<string>("pong");
        return Task.FromResult(result);
    }
}