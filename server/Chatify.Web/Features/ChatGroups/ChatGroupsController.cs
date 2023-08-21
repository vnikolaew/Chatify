using System.Net;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.Messages.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Guid = System.Guid;
using CreateChatGroupResult = OneOf.OneOf<Chatify.Application.User.Commands.FileUploadError, System.Guid>;
using SearchChatGroupsByNameResult = OneOf.OneOf<LanguageExt.Common.Error, System.Collections.Generic.List<Chatify.Domain.Entities.ChatGroup>>;
using SearchChatGroupMembersByNameResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        System.Collections.Generic.List<Chatify.Domain.Entities.User>>;
using GetChatGroupPinnedMessagesResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.ChatGroupNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMemberError,
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
    OneOf.OneOf<LanguageExt.Common.Error,
        System.Collections.Generic.List<Chatify.Application.ChatGroups.Queries.ChatGroupFeedEntry>>;
using GetChatGroupSharedAttachmentsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, Chatify.Shared.Abstractions.Queries.
        CursorPaged<Chatify.Domain.Entities.ChatGroupAttachment>>;
using static Chatify.Web.Features.ChatGroups.Models.Models;

namespace Chatify.Web.Features.ChatGroups;

public class ChatGroupsController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateChatGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<CreateChatGroup, CreateChatGroupResult>(
            request.ToCommand(), cancellationToken);
        return result.Match(
            err => err.ToBadRequest(),
            id => CreatedAtAction(nameof(Details), "ChatGroups",
                new { groupId = id }, new { groupId = id }));
    }

    [HttpGet]
    [Route("{groupId:guid}")]
    public Task<IActionResult> Details(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupDetails, GetChatGroupDetailsResult>(
                new GetChatGroupDetails(groupId),
                cancellationToken)
            .MatchAsync<ChatGroupNotFoundError, UserIsNotMemberError, ChatGroupDetailsEntry, IActionResult>(
                _ => NotFound(), _ => BadRequest(), Ok);

    [HttpGet]
    [Route("feed")]
    [ProducesResponseType(( int )HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(List<ChatGroupFeedEntry>), ( int )HttpStatusCode.OK)]
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
    [Route("{groupId:guid}/members/search")]
    public async Task<IActionResult> SearchMembers(
        [FromQuery(Name = "q")] string usernameQuery,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>(
            new SearchChatGroupMembersByName(groupId, usernameQuery), cancellationToken);

        return result.Match(
            err => ( IActionResult )BadRequest(),
            entries => Ok(new { Data = entries }));
    }
    
    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string nameQuery,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<SearchChatGroupsByName, SearchChatGroupsByNameResult >(
            new SearchChatGroupsByName(nameQuery), cancellationToken);

        return result.Match(
            err => ( IActionResult )BadRequest(),
            entries => Ok(new { Data = entries }));
    }

    [HttpGet]
    [Route("{groupId:guid}/attachments")]
    public async Task<IActionResult> Attachments(
        [FromBody] GetChatGroupSharedAttachmentsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(
            ( request with { GroupId = groupId } ).ToCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            attachments => Ok(new { Data = attachments }));
    }

    [HttpGet]
    [Route("{groupId:guid}/pins")]
    public async Task<IActionResult> PinnedMessages(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>(
            new GetChatGroupPinnedMessages(groupId), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => BadRequest(),
            messages => Ok(new { Data = messages }));
    }

    [HttpPatch]
    [Route("{groupId:guid}")]
    public Task<IActionResult> Edit(
        [FromForm] EditChatGroupDetailsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => SendAsync<EditChatGroupDetails, EditChatGroupDetailsResult>(
                ( request with { ChatGroupId = groupId } ).ToCommand(),
                cancellationToken)
            .MatchAsync(
                _ => BadRequest(),
                err => err.ToBadRequest(),
                _ => BadRequest(),
                _ => BadRequest(),
                Accepted);

    [HttpPost]
    [Route("members")]
    public Task<IActionResult> AddMember(
        [FromBody] AddChatGroupMember addChatGroupMember,
        CancellationToken cancellationToken = default)
        => SendAsync<AddChatGroupMember, AddChatGroupMemberResult>(
                addChatGroupMember,
                cancellationToken)
            .MatchAsync(
                _ => BadRequest(),
                _ => BadRequest(),
                _ => BadRequest(),
                _ => BadRequest(),
                id => ( IActionResult )Ok(id));

    [HttpDelete]
    [Route("members")]
    public async Task<IActionResult> RemoveMember(
        [FromBody] RemoveChatGroupMember removeChatGroupMember,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RemoveChatGroupMember, RemoveChatGroupMemberResult>(
            removeChatGroupMember,
            cancellationToken);

        return result
            .Match(
                _ => BadRequest(),
                _ => BadRequest(),
                _ => BadRequest(),
                _ => ( IActionResult )NoContent());
    }

    [HttpPost]
    [Route("leave")]
    public async Task<IActionResult> LeaveChatGroup(
        [FromBody] LeaveChatGroup leaveChatGroup,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<LeaveChatGroup, LeaveChatGroupResult>(
            leaveChatGroup,
            cancellationToken);
        return result.Match(
            _ => BadRequest(),
            _ => BadRequest(),
            Accepted);
    }


    [HttpPost]
    [Route("admins")]
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
                _ => BadRequest(),
                _ => BadRequest(),
                _ => ( IActionResult )Accepted());
    }
}