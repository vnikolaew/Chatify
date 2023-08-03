﻿using Chatify.Application.ChatGroups.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Guid = System.Guid;
using AddChatGroupMemberResult = LanguageExt.Either<LanguageExt.Common.Error, System.Guid>;
using EditChatGroupDetailsResult = LanguageExt.Either<LanguageExt.Common.Error, LanguageExt.Unit>;
using static Chatify.Web.Features.ChatGroups.Models.Models;

namespace Chatify.Web.Features.ChatGroups;

public class ChatGroupsController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateChatGroupRequest request,
        CancellationToken cancellationToken = default)
        => await SendAsync<CreateChatGroup, Either<Guid, Error>>(request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(err => err.ToBadRequest(),
                groupId => CreatedAtAction(nameof(Details), "ChatGroups", new { groupId },
                    new { groupId }));

    [HttpGet]
    [Route("{groupId:guid}")]
    public async Task<IActionResult> Details(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => Ok(groupId);

    [HttpPatch]
    [Route("{groupId:guid}")]
    public Task<IActionResult> Edit(
        [FromBody] EditChatGroupDetailsRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => SendAsync<EditChatGroupDetails, EditChatGroupDetailsResult>(
                (request with { ChatGroupId = groupId }).ToCommand(),
                cancellationToken)
            .ToAsync()
            .Match(_ => Accepted(),
                err => err.ToBadRequest());

    [HttpPost]
    [Route("members")]
    public Task<IActionResult> AddMember(
        [FromBody] AddChatGroupMember addChatGroupMember,
        CancellationToken cancellationToken = default)
        => SendAsync<AddChatGroupMember, AddChatGroupMemberResult>(
                addChatGroupMember,
                cancellationToken)
            .ToAsync()
            .Match(id => Ok(id), err => err.ToBadRequest());
}