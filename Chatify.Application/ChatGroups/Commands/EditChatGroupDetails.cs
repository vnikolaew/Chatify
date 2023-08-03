using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.ChatGroups.Commands;

using EditChatGroupDetailsResult = Either<Error, Unit>;

public record EditChatGroupDetails(
    [Required] Guid ChatGroupId,
    [MinLength(3), MaxLength(50)] string? Name,
    [MinLength(3), MaxLength(500)] string? About,
    InputFile? Picture
) : ICommand<EditChatGroupDetailsResult>;

internal sealed class EditChatGroupDetailsHandler : ICommandHandler<EditChatGroupDetails, EditChatGroupDetailsResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IChatGroupMemberRepository _members;
    private readonly IClock _clock;
    private readonly IFileUploadService _fileUploadService;
    private readonly IIdentityContext _identityContext;

    public EditChatGroupDetailsHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IChatGroupMemberRepository members,
        IIdentityContext identityContext, IClock clock,
        IFileUploadService fileUploadService)
    {
        _groups = groups;
        _members = members;
        _identityContext = identityContext;
        _clock = clock;
        _fileUploadService = fileUploadService;
    }

    public async Task<EditChatGroupDetailsResult> HandleAsync(
        EditChatGroupDetails command,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(command.ChatGroupId, cancellationToken);
        if (group is null) return Error.New("");

        var isMember = await _members.Exists(group.Id, _identityContext.Id, cancellationToken);
        if (!isMember) return Error.New("");
        if (!group.AdminIds.Contains(_identityContext.Id)) return Error.New("");

        string? groupPictureUrl = default;
        if (command.Picture is not null)
        {
            var result = await _fileUploadService.UploadAsync(
                new SingleFileUploadRequest
                {
                    UserId = _identityContext.Id,
                    File = command.Picture
                }, cancellationToken);

            groupPictureUrl = result.Match(res => res.FileUrl, _ => null!);
        }

        await _groups.UpdateAsync(group.Id, group =>
        {
            group.Name = command.Name ?? group.Name;
            group.About = command.About ?? group.About;
            group.PictureUrl = groupPictureUrl ?? group.PictureUrl;
            group.UpdatedAt = _clock.Now;
        }, cancellationToken);

        return Unit.Default;
    }
}