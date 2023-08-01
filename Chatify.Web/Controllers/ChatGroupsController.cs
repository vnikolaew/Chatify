using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Models;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Guid = System.Guid;
using CreateChatGroupResult = LanguageExt.Either<System.Guid, LanguageExt.Common.Error>;
using AddChatGroupMemberResult = LanguageExt.Either<LanguageExt.Common.Error, System.Guid>;

namespace Chatify.Web.Controllers;

public record CreateChatGroupRequest(
    string? About,
    string Name,
    IFormFile? File)
{
    public CreateChatGroup ToCommand()
        => new(About, Name, File is not null
            ? new InputFile { Data = File.OpenReadStream(), FileName = File.FileName }
            : default);
}

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

    [HttpPost]
    [Route("members")]
    public Task<IActionResult> AddMember(
        [FromBody] AddChatGroupMember addChatGroupMember,
        CancellationToken cancellationToken = default)
        => SendAsync<AddChatGroupMember, AddChatGroupMemberResult>(addChatGroupMember, cancellationToken)
            .ToAsync()
            .Match(id => Ok(id), err => err.ToBadRequest());
}