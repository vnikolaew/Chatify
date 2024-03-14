using Chatify.Domain.Entities;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Contracts;

public interface IChatGroupsService
{
    Task<OneOf<Error, Guid>> CreateChatGroupAsync(CreateChatGroupRequest request, CancellationToken cancellationToken);

    Task<List<OneOf<Error, Guid>>> AddChatGroupMembersAsync(AddChatGroupMembersRequest request,
        CancellationToken cancellationToken);

    Task<OneOf<Error, Unit>> AddChatGroupAdminAsync(AddChatGroupAdminRequest request,
        CancellationToken cancellationToken);

    Task<OneOf<Error, Unit>> UpdateChatGroupDetailsAsync(
        UpdateChatGroupDetailsRequest request,
        CancellationToken cancellationToken);
}

public record UpdateChatGroupDetailsRequest(
    Guid ChatGroupId,
    string? Name,
    string? About,
    Media? Picture);

public record AddChatGroupAdminRequest(
    Guid ChatGroupId,
    Guid AdminId);

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