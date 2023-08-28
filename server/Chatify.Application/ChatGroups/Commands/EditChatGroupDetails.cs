using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Commands;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using EditChatGroupDetailsResult =
    OneOf<ChatGroupNotFoundError, FileUploadError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record EditChatGroupDetails(
    [Required] Guid ChatGroupId,
    [MinLength(3), MaxLength(50)] string? Name,
    [MinLength(3), MaxLength(500)] string? About,
    InputFile? Picture
) : ICommand<EditChatGroupDetailsResult>;

internal sealed class EditChatGroupDetailsHandler(IDomainRepository<ChatGroup, Guid> groups,
        IChatGroupMemberRepository members,
        IIdentityContext identityContext, IClock clock,
        IFileUploadService fileUploadService)
    : ICommandHandler<EditChatGroupDetails, EditChatGroupDetailsResult>
{
    public async Task<EditChatGroupDetailsResult> HandleAsync(
        EditChatGroupDetails command,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(command.ChatGroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(group.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, group.Id);
        if ( !group.AdminIds.Contains(identityContext.Id) )
            return new UserIsNotGroupAdminError(identityContext.Id, group.Id);

        Media? groupPicture = default;
        if ( command.Picture is not null )
        {
            if ( group.Picture is not null )
            {
                var deleteResult = await fileUploadService.DeleteAsync(
                    new SingleFileDeleteRequest
                    {
                        UserId = identityContext.Id,
                        FileUrl = group.Picture.MediaUrl
                    }, cancellationToken);

                if ( deleteResult.Value is Error error ) return new FileUploadError(error.Message);
            }

            var result = await fileUploadService.UploadAsync(
                new SingleFileUploadRequest
                {
                    UserId = identityContext.Id,
                    File = command.Picture
                }, cancellationToken);
            if ( result.Value is Error uploadError ) return new FileUploadError(uploadError.Message);
            
            if ( result.Value is FileUploadResult newMedia )
            {
                groupPicture = new Media
                {
                    Id = newMedia.FileId,
                    FileName = newMedia.FileName,
                    MediaUrl = newMedia.FileUrl,
                    Type = newMedia.FileType
                };
            }
        }

        await groups.UpdateAsync(group, group =>
        {
            group.Name = command.Name ?? group.Name;
            group.About = command.About ?? group.About;
            group.Picture = groupPicture;
            group.UpdatedAt = clock.Now;
        }, cancellationToken);

        return Unit.Default;
    }
}