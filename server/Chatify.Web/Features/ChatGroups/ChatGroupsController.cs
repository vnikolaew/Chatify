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
using UnstarChatGroupResult = OneOf.OneOf<Chatify.Application.User.Common.UserNotFound, Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError, LanguageExt.Unit>;

using static Chatify.Web.Features.ChatGroups.Models.Models;

namespace Chatify.Web.Features.ChatGroups;

public class ChatGroupsController : ApiController
{
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
    [Route("{groupId:guid}")]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<ChatGroupDetailsEntry>]
    public Task<IActionResult> Details(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupDetails, GetChatGroupDetailsResult>(
                new GetChatGroupDetails(groupId),
                cancellationToken)
            .MatchAsync(_ => NotFound(),
                _ => _.ToBadRequest(),
                Ok);

    [HttpGet]
    [Route("feed")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroupFeedEntry>>]
    public async Task<IActionResult> Feed(
        [FromQuery] int limit,
        [FromQuery] int offset,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupsFeed, GetChatGroupsFeedResult>(
            new GetChatGroupsFeed(limit, offset), cancellationToken);
        return result.Match(
            err => err.ToBadRequest(),
            Ok);
    }
    
    [HttpGet]
    [Route("starred/feed")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroupFeedEntry>>]
    public async Task<IActionResult> StarredFeed(CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetStarredChatGroupsFeed, GetChatGroupsFeedResult>(
            new GetStarredChatGroupsFeed(), cancellationToken);
        
        return result.Match(
            err => err.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("{groupId:guid}/members/search")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<User>>]
    public async Task<IActionResult> SearchMembers(
        [FromQuery(Name = "q")] string usernameQuery,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>(
            new SearchChatGroupMembersByName(groupId, usernameQuery), cancellationToken);

        return result.Match(
            err => err.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("search")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatGroup>>]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string? nameQuery,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<SearchChatGroupsByName, SearchChatGroupsByNameResult>(
            new SearchChatGroupsByName(nameQuery ?? string.Empty), cancellationToken);

        return result.Match(
            err => err.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("{groupId:guid}/attachments")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<CursorPaged<ChatGroupAttachment>>]
    public async Task<IActionResult> Attachments(
        [FromQuery] GetChatGroupSharedAttachmentsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(
            ( request with { GroupId = groupId } ).ToCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => _.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("{groupId:guid}/pins")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<ChatMessage>>]
    public async Task<IActionResult> PinnedMessages(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>(
            new GetChatGroupPinnedMessages(groupId), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Ok);
    }

    [HttpPatch]
    [Route("{groupId:guid}")]
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
    [Route("members")]
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
    [Route("members/{groupId:guid}/{memberId:guid}")]
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
    [Route("members")]
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
    [Route("leave")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> LeaveChatGroup(
        [FromBody] LeaveChatGroup leaveChatGroup,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<LeaveChatGroup, LeaveChatGroupResult>(
            leaveChatGroup,
            cancellationToken);
        return result.Match(
            _ => BadRequest(),
            _ => _.ToBadRequest(),
            Accepted);
    }

    [HttpPost]
    [Route("admins")]
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
    [Route("{chatGroupId:guid}/admins/{adminId:guid}")]
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

        return result
            .Match(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                _ => _.ToBadRequest(),
                _ => Accepted());
    }

    [HttpPost]
    [Route("starred/{chatGroupId:guid}")]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> StarChatGroup(
        [FromRoute] Guid chatGroupId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<StarChatGroup, StarChatGroupResult>(
            new StarChatGroup(chatGroupId),
            cancellationToken);

        return result.Match(
            _ => NotFound(),
            _ => NotFound(),
            _ => Accepted());
    }

    [HttpDelete]
    [Route("starred/{chatGroupId:guid}")]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> UnstarChatGroup(
        [FromRoute] Guid chatGroupId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UnstarChatGroup, UnstarChatGroupResult>(
            new  UnstarChatGroup(chatGroupId),
            cancellationToken);

        return result.Match(
            _ => NotFound(),
            _ => NotFound(),
            _ => Accepted());
    }

    [HttpGet]
    [Route("starred")]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<ChatGroup>>]
    public async Task<IActionResult> StarChatGroup(CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetStarredGroups, GetStarredGroupsResult>(
            new GetStarredGroups(),
            cancellationToken);

        return result.Match(_ => NotFound(), Ok);
    }
}