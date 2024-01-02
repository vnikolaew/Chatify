using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
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

using DeclineChatGroupJoinRequestResult = OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, Error, Unit>;

public record DeclineChatGroupJoinRequest(
    [Required] Guid RequestId
) : ICommand<DeclineChatGroupJoinRequestResult>;

internal sealed class DeclineChatGroupJoinRequestHandler(IChatGroupRepository groups,
        IIdentityContext identityContext,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests,
        IEventDispatcher eventDispatcher,
        IClock clock)
    : ICommandHandler<DeclineChatGroupJoinRequest, DeclineChatGroupJoinRequestResult>
{
    public async Task<DeclineChatGroupJoinRequestResult> HandleAsync(DeclineChatGroupJoinRequest command,
        CancellationToken cancellationToken = default)
    {
        var request = await joinRequests.GetAsync(command.RequestId, cancellationToken);
        if ( request is null ) return Error.New("");

        var group = await groups.GetAsync(request.ChatGroupId, cancellationToken);
        if ( group is null ) return Error.New("");

        var isCurrentUserGroupAdmin = group
            .AdminIds
            .Contains(identityContext.Id);
        if ( !isCurrentUserGroupAdmin ) return Error.New(string.Empty);

        await joinRequests.DeleteAsync(request.Id, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatGroupJoinRequestDeclined
        {
            RequestId = request.Id,
            UserId = request.UserId,
            Timestamp = clock.Now,
            GroupId = group.Id,
            DeclinedById = identityContext.Id
        }, cancellationToken);
        return Unit.Default;
    }
}