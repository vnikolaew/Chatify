using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.JoinRequests.Commands;

using DeclineChatGroupJoinRequestResult = OneOf<Error, Unit>;

public record DeclineChatGroupJoinRequest(
    [Required] Guid RequestId
) : ICommand<DeclineChatGroupJoinRequestResult>;

internal sealed class DeclineChatGroupJoinRequestHandler
    : ICommandHandler<DeclineChatGroupJoinRequest, DeclineChatGroupJoinRequestResult>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IChatGroupRepository _groups;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatGroupJoinRequest, Guid> _joinRequests;

    public DeclineChatGroupJoinRequestHandler(
        IChatGroupRepository groups,
        IIdentityContext identityContext,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests, IEventDispatcher eventDispatcher, IClock clock)
    {
        _groups = groups;
        _identityContext = identityContext;
        _joinRequests = joinRequests;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<DeclineChatGroupJoinRequestResult> HandleAsync(DeclineChatGroupJoinRequest command,
        CancellationToken cancellationToken = default)
    {
        var request = await _joinRequests.GetAsync(command.RequestId, cancellationToken);
        if ( request is null ) return Error.New("");

        var group = await _groups.GetAsync(request.ChatGroupId, cancellationToken);
        if ( group is null ) return Error.New("");

        var isCurrentUserGroupAdmin = group
            .AdminIds
            .Any(_ => _ == _identityContext.Id);
        if ( !isCurrentUserGroupAdmin ) return Error.New("");

        await _joinRequests.DeleteAsync(request.Id, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatGroupJoinRequestDeclined
        {
            RequestId = request.Id,
            UserId = request.UserId,
            Timestamp = _clock.Now,
            GroupId = group.Id,
            DeclinedById = _identityContext.Id
        }, cancellationToken);
        return Unit.Default;
    }
}