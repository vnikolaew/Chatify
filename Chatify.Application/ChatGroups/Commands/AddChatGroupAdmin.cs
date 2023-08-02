using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.ChatGroups.Commands;

using AddChatGroupAdminResult = Either<Error, Unit>;

public record AddChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid NewAdminId
) : ICommand<AddChatGroupAdminResult>;

internal sealed class AddChatGroupAdminHandler : ICommandHandler<AddChatGroupAdmin, AddChatGroupAdminResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IIdentityContext _identityContext;

    public AddChatGroupAdminHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IEventDispatcher eventDispatcher,
        IIdentityContext identityContext,
        IClock clock)
    {
        _groups = groups;
        _eventDispatcher = eventDispatcher;
        _identityContext = identityContext;
        _clock = clock;
    }

    public async Task<AddChatGroupAdminResult> HandleAsync(
        AddChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await _groups.GetAsync(command.ChatGroupId, cancellationToken);
        if (chatGroup is null) return Error.New("");

        if (!chatGroup.AdminIds.Contains(_identityContext.Id))
            return Error.New("Current user is not an Admin of the specified Chat Group.");

        if (chatGroup.AdminIds.Contains(command.NewAdminId))
            return Error.New("User is already an Admin of the specified Chat Group.");

        await _groups.UpdateAsync(chatGroup.Id, group =>
        {
            group.AdminIds.Add(command.NewAdminId);
            group.UpdatedAt = _clock.Now;
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatGroupAdminAdded
        {
            GroupId = chatGroup.Id,
            AdminId = command.NewAdminId,
            Timestamp = _clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}