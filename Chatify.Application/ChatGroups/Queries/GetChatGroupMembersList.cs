using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupMembersListResult = Either<Error, List<ChatGroupMember>>;

public record GetChatGroupMembersList(
    [Required] Guid ChatGroupId
) : ICommand<GetChatGroupMembersListResult>;

internal sealed class GetChatGroupMembersListHandler
    : ICommandHandler<GetChatGroupMembersList, GetChatGroupMembersListResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IIdentityContext _identityContext;

    public GetChatGroupMembersListHandler(
        IChatGroupMemberRepository members,
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext)
    {
        _members = members;
        _groups = groups;
        _identityContext = identityContext;
    }

    public async Task<GetChatGroupMembersListResult> HandleAsync(
        GetChatGroupMembersList command,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(
            command.ChatGroupId,
            cancellationToken);
        if (group is null) return Error.New("Chat Group does not exist.");

        var isMember = await _members.Exists(
            group.Id, _identityContext.Id, cancellationToken);
        if (!isMember) return Error.New("Current user is not a member of the Chat Group.");

        var members = await _members
            .ByGroup(group.Id, cancellationToken);
        
        return members;
    }
}