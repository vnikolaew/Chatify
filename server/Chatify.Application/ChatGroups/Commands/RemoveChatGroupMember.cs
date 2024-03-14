using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using RemoveChatGroupMemberResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record RemoveChatGroupMember(
    [Required] Guid GroupId,
    [Required] Guid MemberId
) : ICommand<RemoveChatGroupMemberResult>;

internal sealed class RemoveChatGroupMemberHandler(
    IChatGroupsService chatGroupsService,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher,
    IClock clock)
    : BaseCommandHandler<RemoveChatGroupMember, RemoveChatGroupMemberResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<RemoveChatGroupMemberResult> HandleAsync(
        RemoveChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var response = await chatGroupsService.RemoveChatGroupMemberAsync(
            new RemoveChatGroupMemberRequest(command.GroupId, command.MemberId), cancellationToken);

        if ( response.Value is Unit )
        {
            await eventDispatcher.PublishAsync(new ChatGroupMemberRemovedEvent
            {
                GroupId = command.GroupId,
                Timestamp = clock.Now,
                MemberId = identityContext.Id,
                RemovedById = identityContext.Id,
            }, cancellationToken);
        }

        return Unit.Default;
    }
}