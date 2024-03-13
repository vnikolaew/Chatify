using Chatify.Domain.Entities;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Contracts;

public interface IChatGroupsService
{
    Task<OneOf<Error, Guid>> CreateChatGroupAsync(CreateChatGroupRequest request, CancellationToken cancellationToken);
    Task<List<OneOf<Error, Guid>>> AddChatGroupMembersAsync(AddChatGroupMembersRequest request, CancellationToken cancellationToken);
}

public record AddChatGroupMembersRequest(
    Guid ChatGroupId,
    List<AddChatGroupMemberRequest> AddChatGroupMembers);

public record AddChatGroupMemberRequest(
    Guid UserId,
    string Username,
    sbyte MembershipType
    );

public record CreateChatGroupRequest(
    string? About,
    string Name,
    IEnumerable<Guid>? MemberIds,
    Media? Media);