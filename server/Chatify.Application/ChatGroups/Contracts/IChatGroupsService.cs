using Chatify.Application.ChatGroups.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
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

    Task<OneOf<Error, Unit>> LeaveChatGroupAsync(
        LeaveChatGroupRequest request,
        CancellationToken cancellationToken);

    Task<OneOf<Error, Unit>> RemoveChatGroupAdminAsync(
        RemoveChatGroupAdminRequest request,
        CancellationToken cancellationToken);

    Task<OneOf<Error, Unit>> RemoveChatGroupMemberAsync(
        RemoveChatGroupMemberRequest request, CancellationToken cancellationToken);

    Task<OneOf<Error, ChatGroup>> GetChatGroupDetails(Guid chatGroupId,
        CancellationToken cancellationToken);

    Task<OneOf<Error, ChatGroupMember>> GetChatGroupMembershipDetailsAsync(
        GetChatGroupMembershipDetailsRequest request, CancellationToken cancellationToken);

    Task<OneOf<Error, List<Guid>>> GetChatGroupMemberIdsAsync(
        Guid chatGroupId,
        CancellationToken cancellationToken);

    Task<CursorPaged<ChatGroupAttachment>> GetChatGroupSharedAttachments(
        GetChatGroupSharedAttachments request, CancellationToken cancellationToken);
}

public record GetChatGroupMembershipDetailsRequest(
    Guid ChatGroupId,
    Guid UserId
);

public record RemoveChatGroupMemberRequest(
    Guid ChatGroupId,
    Guid MemberId
);

public record RemoveChatGroupAdminRequest(
    Guid ChatGroupId,
    Guid AdminId
);

public record LeaveChatGroupRequest(
    Guid ChatGroupId,
    string? Reason
);

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