using System.Net;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.Messages.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Guid = System.Guid;
using CreateChatGroupResult = OneOf.OneOf<Chatify.Application.User.Commands.FileUploadError, System.Guid>;
using SearchChatGroupsByNameResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Domain.Entities.ChatGroup>>;
using GetChatGroupMembershipDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, LanguageExt.Common.Error,
        Chatify.Domain.Entities.ChatGroupMember>;
using SearchChatGroupMembersByNameResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        System.Collections.Generic.List<Chatify.Domain.Entities.User>>;
using GetChatGroupPinnedMessagesResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        System.Collections.Generic.List<Chatify.Domain.Entities.ChatMessage>>;
using GetChatGroupDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Queries.ChatGroupDetailsEntry>;
using AddChatGroupMemberResult =
    OneOf.OneOf<Chatify.Application.User.Common.UserNotFound,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError,
        Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsAlreadyGroupMemberError, System.Guid>;
using EditChatGroupDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.User.Commands.FileUploadError, Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;
using AddChatGroupAdminResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;
using RemoveChatGroupMemberResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;
using LeaveChatGroupResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, LanguageExt.Unit>;
using GetChatGroupsFeedResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Application.ChatGroups.Queries.ChatGroupFeedEntry>>;
using GetChatGroupSharedAttachmentsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, Chatify.Shared.Abstractions.Queries.
        CursorPaged<Chatify.Domain.Entities.ChatGroupAttachment>>;
using RemoveChatGroupAdminResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;
using StarChatGroupResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.User.Common.UserNotFound, LanguageExt.Unit>;
using GetStarredGroupsResult =
    OneOf.OneOf<Chatify.Application.User.Common.UserNotFound,
        System.Collections.Generic.List<Chatify.Domain.Entities.ChatGroup>>;
using UnstarChatGroupResult =
    OneOf.OneOf<Chatify.Application.User.Common.UserNotFound,
        Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError, LanguageExt.Unit>;
using static Chatify.Web.Features.ChatGroups.Models.Models;

namespace Chatify.Web.Features.ChatGroups;

public class ChatGroupsController : ApiController
{
    private const string GroupDetailsRoute = "{groupId:guid}";
    
    private const string FeedRoute = "feed";
    private const string StarredFeedRoute = "starred/feed";
    
    private const string SearchMembersRoute = "{groupId:guid}/members/search";
    private const string SearchRoute = "search";
        
    private const string AttachmentsRoute = "{groupId:guid}/attachments";
    private const string PinnedRoute = "{groupId:guid}/pins";
        
    private const string EditGroupRoute = "{groupId:guid}";
    private const string MembersRoute = "members";
        
    private const string MembershipDetailsRoute = "members/{groupId:guid}/{memberId:guid}";
    private const string DeleteMemberRoute = "members";
    
    private const string LeaveChatGroupRoute = "leave";
    private const string AddAminRoute = "admins";
        
    private const string RemoveAdminRoute = "{chatGroupId:guid}/admins/{adminId:guid}";
    private const string StarChatGroupRoute = "starred/{chatGroupId:guid}";
    private const string StarredRoute = "starred";
    
    [HttpPost]
    [ProducesBadRequestApiResponse]
    [ProducesCreatedAtApiResponse]
    public async Task<IActionResult> Create(
        [FromForm] CreateChatGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<CreateChatGroup, CreateChatGroupResult>(
            request.ToCommand(), cancellationToken);
        return result.Match(
            err => err.ToBadRequest(),
            id => CreatedAtAction(nameof(Details), "ChatGroups",
                new { groupId = id },
                ApiResponse<object>.Success(new { groupId = id },
                    "Chat group successfully created.")));
    }

    [HttpGet]
    [Route(GroupDetailsRoute)]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<ChatGroupDetailsEntry>]
    public Task<IActionResult> Details(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupDetails, GetChatGroupDetailsResult>(
                new GetChatGroupDetails(groupId), cancellationToken)
            .MatchAsync(_ => NotFound(),
                _ => _.ToBadRequest(),
                Ok);

    [HttpGet]
    [Route(FeedRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroupFeedEntry>>]
    public Task<IActionResult> Feed(
        [FromQuery] int limit,
        [FromQuery] int offset,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupsFeed, GetChatGroupsFeedResult>(
                new GetChatGroupsFeed(limit, offset), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                Ok
            );

    [HttpGet]
    [Route(StarredFeedRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroupFeedEntry>>]
    public Task<IActionResult> StarredFeed(CancellationToken cancellationToken = default)
        => QueryAsync<GetStarredChatGroupsFeed, GetChatGroupsFeedResult>(
                new GetStarredChatGroupsFeed(), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(), Ok
            );

    [HttpGet]
    [Route(SearchMembersRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<User>>]
    public Task<IActionResult> SearchMembers(
        [FromQuery(Name = "q")] string usernameQuery,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>(
                new SearchChatGroupMembersByName(groupId, usernameQuery), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(), Ok
            );

    [HttpGet]
    [Route(SearchRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroup>>]
    public Task<IActionResult> Search(
        [FromQuery(Name = "q")] string? nameQuery,
        CancellationToken cancellationToken = default)
        => QueryAsync<SearchChatGroupsByName, SearchChatGroupsByNameResult>(
                new SearchChatGroupsByName(nameQuery ?? string.Empty), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(), Ok
            );

    [HttpGet]
    [Route(AttachmentsRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<CursorPaged<ChatGroupAttachment>>]
    public Task<IActionResult> Attachments(
        [FromQuery] GetChatGroupSharedAttachmentsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(
                ( request with { GroupId = groupId } ).ToCommand(), cancellationToken)
            .MatchAsync(
                _ => _.ToBadRequest(),
                Ok
            );

    [HttpGet]
    [Route(PinnedRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<ChatMessage>>]
    public Task<IActionResult> PinnedMessages(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>(
                new GetChatGroupPinnedMessages(groupId), cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                Ok
            );

    [HttpPatch]
    [Route(EditGroupRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> Edit(
        [FromForm] EditChatGroupDetailsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => SendAsync<EditChatGroupDetails, EditChatGroupDetailsResult>(
                ( request with { ChatGroupId = groupId } ).ToCommand(),
                cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                err => err.ToBadRequest(),
                _ => _.ToBadRequest(),
                _ => _.ToBadRequest(),
                Accepted);

    [HttpPost]
    [Route(MembersRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<Guid>]
    public Task<IActionResult> AddMember(
        [FromBody] AddChatGroupMember addChatGroupMember,
        CancellationToken cancellationToken = default)
        => SendAsync<AddChatGroupMember, AddChatGroupMemberResult>(
                addChatGroupMember,
                cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                _ => NotFound(),
                _ => _.ToBadRequest(),
                id => Accepted(id));

    [HttpGet]
    [Route(MembershipDetailsRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<ChatGroupMember>]
    public Task<IActionResult> MembershipDetails(
        Guid groupId,
        Guid memberId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupMembershipDetails, GetChatGroupMembershipDetailsResult>(
                new GetChatGroupMembershipDetails(groupId, memberId),
                cancellationToken)
            .MatchAsync(
                _ => _.ToBadRequest(),
                _ => NotFound(),
                Ok);


    [HttpDelete]
    [Route(DeleteMemberRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesNoContentApiResponse]
    [ProducesResponseType(( int )HttpStatusCode.NoContent)]
    public async Task<IActionResult> RemoveMember(
        [FromBody] RemoveChatGroupMember removeChatGroupMember,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RemoveChatGroupMember, RemoveChatGroupMemberResult>(
            removeChatGroupMember,
            cancellationToken);

        return result
            .Match(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                _ => _.ToBadRequest(),
                _ => NoContent());
    }

    [HttpPost]
    [Route(LeaveChatGroupRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> LeaveChatGroup(
        [FromBody] LeaveChatGroup leaveChatGroup,
        CancellationToken cancellationToken = default)
        => SendAsync<LeaveChatGroup, LeaveChatGroupResult>(
                leaveChatGroup,
                cancellationToken)
            .MatchAsync(
                _ => BadRequest(),
                _ => _.ToBadRequest(),
                Accepted
            );

    [HttpPost]
    [Route(AddAminRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> AddAdmin(
        [FromBody] AddChatGroupAdmin addChatGroupAdmin,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<AddChatGroupAdmin, AddChatGroupAdminResult>(
            addChatGroupAdmin,
            cancellationToken);

        return result
            .Match(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                _ => _.ToBadRequest(),
                _ => Accepted());
    }

    [HttpDelete]
    [Route(RemoveAdminRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    // ReSharper disable once RouteTemplates.MethodMissingRouteParameters
    public async Task<IActionResult> RemoveAdmin(
        [FromRoute] RemoveChatGroupAdmin removeChatGroupAdmin,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RemoveChatGroupAdmin, RemoveChatGroupAdminResult>(
            removeChatGroupAdmin,
            cancellationToken);
        return result.Match(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                _ => _.ToBadRequest(),
                _ => Accepted()
            );
    }

    [HttpPost]
    [Route(StarChatGroupRoute)]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> StarChatGroup(
        [FromRoute] Guid chatGroupId,
        CancellationToken cancellationToken = default)
        => SendAsync<StarChatGroup, StarChatGroupResult>(
                new StarChatGroup(chatGroupId),
                cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                _ => NotFound(),
                _ => Accepted()
            );

    [HttpDelete]
    [Route(StarChatGroupRoute)]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> UnstarChatGroup(
        [FromRoute] Guid chatGroupId,
        CancellationToken cancellationToken = default)
        => SendAsync<UnstarChatGroup, UnstarChatGroupResult>(
                new UnstarChatGroup(chatGroupId),
                cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                _ => NotFound(),
                _ => Accepted()
            );

    [HttpGet]
    [Route(StarredRoute)]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<ChatGroup>>]
    public Task<IActionResult> StarredGroups(CancellationToken cancellationToken = default)
        => QueryAsync<GetStarredGroups, GetStarredGroupsResult>(
                new GetStarredGroups(),
                cancellationToken)
            .MatchAsync(_ => NotFound(), Ok);
}