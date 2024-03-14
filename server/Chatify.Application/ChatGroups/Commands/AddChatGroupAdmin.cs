using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Models;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using AddChatGroupAdminResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record ChatGroupNotFoundError;

public record UserIsNotMemberError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is not a member of this chat group.");

public record UserIsNotGroupAdminError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is not an admin of this chat group.");

public record AddChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid NewAdminId
) : ICommand<AddChatGroupAdminResult>;

internal sealed class AddChatGroupAdminHandler(
    IChatGroupsService chatGroupsService,
    IEventDispatcher eventDispatcher,
    IIdentityContext identityContext,
    IClock clock)
    : BaseCommandHandler<AddChatGroupAdmin, AddChatGroupAdminResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<AddChatGroupAdminResult> HandleAsync(
        AddChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        var response = await chatGroupsService.AddChatGroupAdminAsync(
            new AddChatGroupAdminRequest(command.ChatGroupId, command.NewAdminId),
            cancellationToken);
        if ( response.Value is Error error ) return Unit.Default;

        await eventDispatcher.PublishAsync(new ChatGroupAdminAdded
        {
            GroupId = command.ChatGroupId,
            AdminId = command.NewAdminId,
            Timestamp = clock.Now
        }, cancellationToken);
        
        return Unit.Default;
    }
}