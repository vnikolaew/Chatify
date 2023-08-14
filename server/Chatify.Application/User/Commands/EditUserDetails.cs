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

internal sealed class EditUserDetailsHandler
    : ICommandHandler<EditUserDetails, EditUserDetailsResult>
{
    private readonly IDomainRepository<Domain.Entities.User, Guid> _users;
    private readonly IFileUploadService _fileUploadService;
    private readonly IIdentityContext _identityContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAuthenticationService _authenticationService;
    private readonly IClock _clock;

    public EditUserDetailsHandler(
        IDomainRepository<Domain.Entities.User, Guid> users,
        IIdentityContext identityContext,
        IClock clock,
        IFileUploadService fileUploadService,
        IAuthenticationService authenticationService,
        IEventDispatcher eventDispatcher)
    {
        _users = users;
        _identityContext = identityContext;
        _clock = clock;
        _fileUploadService = fileUploadService;
        _authenticationService = authenticationService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<EditUserDetailsResult> HandleAsync(
        EditUserDetails command,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(_identityContext.Id, cancellationToken);
        if ( user is null ) return new UserNotFound();

        var newProfilePicture = user.ProfilePicture;
        var newUsername = command.Username ?? user.Username;
        var newDisplayName = command.DisplayName ?? user.DisplayName;

        if ( command.ProfilePicture is not null )
        {
            var deleteResult = await _fileUploadService.DeleteAsync(
                new SingleFileDeleteRequest
                {
                    UserId = user.Id,
                    FileUrl = user.ProfilePicture.MediaUrl
                }, cancellationToken);
            if ( deleteResult.IsT0 )
            {
                return new FileUploadError(deleteResult.AsT0.Message);
            }

            var uploadResult = await _fileUploadService
                .UploadAsync(
                    new SingleFileUploadRequest
                    {
                        UserId = _identityContext.Id,
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
            var result = await _authenticationService.ChangePasswordAsync(
                user.Id,
                command.NewPasswordInput.OldPassword,
                command.NewPasswordInput.NewPassword,
                cancellationToken);

            if ( result.Value is UserNotFound userNotFound ) return userNotFound;
            if ( result.Value is PasswordChangeError passwordChangeError ) return passwordChangeError;
        }

        await _users.UpdateAsync(user.Id, user =>
        {
            if ( command.PhoneNumbers?.Any() ?? false )
            {
                user.PhoneNumbers = command.PhoneNumbers
                    .Select(_ => new PhoneNumber(_))
                    .ToHashSet();
            }

            user.Username = newUsername;
            user.DisplayName = newDisplayName;
            user.UpdatedAt = _clock.Now;
            user.ProfilePicture = newProfilePicture;
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new UserDetailsEditedEvent
        {
            UserId = user.Id,
            ProfilePicture = user.ProfilePicture,
            PhoneNumbers = command.PhoneNumbers,
            DisplayName = user.DisplayName
        }, cancellationToken);
        return Unit.Default;
    }
}