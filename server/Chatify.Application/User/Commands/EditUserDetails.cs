using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Users;
using Chatify.Domain.ValueObjects;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.User.Commands;

using EditUserDetailsResult = OneOf<UserNotFound, FileUploadError, PasswordChangeError, Unit>;


public record FileUploadError(string? Message = default) : BaseError(Message);

public record PasswordChangeError(string? Message = default) : BaseError(Message);

public record NewPasswordInput(
    [Password] string OldPassword,
    [Password] string NewPassword);

public record EditUserDetails(
    string? Username,
    [MinLength(3), MaxLength(50)] string? DisplayName,
    InputFile? ProfilePicture,
    ISet<string>? PhoneNumbers,
    NewPasswordInput? NewPasswordInput) : ICommand<EditUserDetailsResult>;

internal sealed class EditUserDetailsHandler(IDomainRepository<Domain.Entities.User, Guid> users,
        IIdentityContext identityContext,
        IClock clock,
        IFileUploadService fileUploadService,
        IAuthenticationService authenticationService,
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
            if ( deleteResult.IsT0 )
            {
                return new FileUploadError(deleteResult.AsT0.Message);
            }

            var uploadResult = await fileUploadService
                .UploadAsync(
                    new SingleFileUploadRequest
                    {
                        UserId = identityContext.Id,
                        File = command.ProfilePicture
                    }, cancellationToken);

            if ( uploadResult.Value is FileUploadResult res )
            {
                user.ProfilePicture = new Media
                {
                    MediaUrl = res.FileUrl,
                    Id = res.FileId,
                    FileName = res.FileUrl,
                    Type = res.FileType
                };

                newProfilePicture.MediaUrl = res.FileUrl;
                newProfilePicture.Id = res.FileId;
            }
        }

        if ( command.NewPasswordInput is not null )
        {
            var result = await authenticationService.ChangePasswordAsync(
                user.Id,
                command.NewPasswordInput.OldPassword,
                command.NewPasswordInput.NewPassword,
                cancellationToken);

            if ( result.Value is UserNotFound userNotFound ) return userNotFound;
            if ( result.Value is PasswordChangeError passwordChangeError ) return passwordChangeError;
        }

        await users.UpdateAsync(user, user =>
        {
            if ( command.PhoneNumbers?.Any() ?? false )
            {
                user.PhoneNumbers = command.PhoneNumbers
                    .Select(_ => new PhoneNumber(_))
                    .ToHashSet();
            }

            user.Username = newUsername;
            user.DisplayName = newDisplayName;
            user.UpdatedAt = clock.Now;
            user.ProfilePicture = newProfilePicture;
        }, cancellationToken);

        await eventDispatcher.PublishAsync(new UserDetailsEditedEvent
        {
            UserId = user.Id,
            ProfilePicture = user.ProfilePicture,
            PhoneNumbers = command.PhoneNumbers,
            DisplayName = user.DisplayName
        }, cancellationToken);
        return Unit.Default;
    }
}