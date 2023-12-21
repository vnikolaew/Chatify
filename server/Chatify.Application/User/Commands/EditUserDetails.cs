using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Domain.ValueObjects;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.User.Commands;

using EditUserDetailsResult = OneOf<UserNotFound, FileUploadError, PasswordChangeError, Unit>;

public record FileUploadError(string? Message = default) : BaseError(Message);

public record PasswordChangeError(string? Message = default) : BaseError(Message);

public record EditUserDetails(
    string? Username,
    [MinLength(3), MaxLength(50)] string? DisplayName,
    InputFile? ProfilePicture,
    System.Collections.Generic.HashSet<string>? PhoneNumbers) : ICommand<EditUserDetailsResult>;

internal sealed class EditUserDetailsHandler(
    IUserRepository users,
    IAuthenticationService authenticationService,
    IIdentityContext identityContext,
    IClock clock,
    IFileUploadService fileUploadService,
    IEventDispatcher eventDispatcher)
    : ICommandHandler<EditUserDetails, EditUserDetailsResult>
{
    public async Task<EditUserDetailsResult> HandleAsync(
        EditUserDetails command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(identityContext.Id, cancellationToken);
        if ( user is null ) return new UserNotFound();

        var newProfilePicture = user.ProfilePicture;
        var newUsername = command.Username ?? user.Username;
        var newDisplayName = command.DisplayName ?? user.DisplayName;

        if ( command.ProfilePicture is not null )
        {
            var deleteResult = await fileUploadService.DeleteAsync(
                new SingleFileDeleteRequest
                {
                    UserId = user.Id,
                    FileUrl = user.ProfilePicture.MediaUrl
                }, cancellationToken);
            if ( deleteResult.Value is Error error ) return new FileUploadError(error.Message);

            var uploadResult = await fileUploadService
                .UploadUserProfilePictureAsync(
                    new SingleFileUploadRequest
                    {
                        UserId = identityContext.Id,
                        File = command.ProfilePicture
                    }, cancellationToken);

            if ( uploadResult.Value is FileUploadResult res )
            {
                newProfilePicture = new Media
                {
                    FileName = res.FileName,
                    MediaUrl = res.FileUrl,
                    Id = res.FileId,
                    Type = res.FileType
                };
            }
        }

        await authenticationService.SignOutAsync(cancellationToken);

        await users.UpdateAsync(user, user =>
        {
            if ( command.PhoneNumbers?.Any() ?? false )
            {
                user.PhoneNumbers = command
                    .PhoneNumbers
                    .Select(n => new PhoneNumber(n))
                    .ToHashSet();
            }

            user.Username = newUsername;
            user.DisplayName = newDisplayName;
            user.UpdatedAt = clock.Now;
            user.ProfilePicture = newProfilePicture;
        }, cancellationToken);

        await authenticationService.RefreshUserClaimsAsync(user, cancellationToken);
        await eventDispatcher.PublishAsync(new UserDetailsEditedEvent
        {
            UserId = user.Id,
            ProfilePicture = newProfilePicture,
            PhoneNumbers = command.PhoneNumbers,
            DisplayName = user.DisplayName
        }, cancellationToken);
        
        return Unit.Default;
    }
}