using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.JoinRequests.Commands;

using AcceptChatGroupJoinRequestResult = OneOf<Error, Guid>;

public record AcceptChatGroupJoinRequest(
    [Required] Guid RequestId
) : ICommand<AcceptChatGroupJoinRequestResult>;

internal sealed class AcceptChatGroupJoinRequestHandler
    : ICommandHandler<AcceptChatGroupJoinRequest, AcceptChatGroupJoinRequestResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IUserRepository _users;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IChatGroupRepository _groups;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatGroupJoinRequest, Guid> _joinRequests;

    public AcceptChatGroupJoinRequestHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests,
        IChatGroupRepository groups,
        IGuidGenerator guidGenerator,
        IClock clock,
        IEventDispatcher eventDispatcher,
        IUserRepository users)
    {
        _members = members;
        _identityContext = identityContext;
        _joinRequests = joinRequests;
        _groups = groups;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
        _users = users;
    }

    public async Task<AcceptChatGroupJoinRequestResult> HandleAsync(
        AcceptChatGroupJoinRequest command,
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

        var user = await _users.GetAsync(request.UserId, cancellationToken);

        var membershipId = _guidGenerator.New();
        var groupMember = new ChatGroupMember
        {
            Id = membershipId,
            ChatGroupId = group.Id,
            UserId = user!.Id,
            Username = user.Username,
            CreatedAt = _clock.Now,
            User = user
        };

        await _members.SaveAsync(groupMember, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatGroupJoinRequestAccepted
        {
            RequestId = request.Id,
            UserId = request.UserId,
            AcceptedById = _identityContext.Id,
            Timestamp = _clock.Now,
            Username = user.Username,
            GroupId = group.Id
        }, cancellationToken);

        return groupMember.Id;
    }
}