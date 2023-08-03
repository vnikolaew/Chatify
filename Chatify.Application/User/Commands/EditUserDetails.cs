﻿using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Domain.Common;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.User.Commands;

using EditUserDetailsResult = Either<Error, Unit>;

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
    private readonly IAuthenticationService _authenticationService;
    private readonly IClock _clock;

    public EditUserDetailsHandler(
        IDomainRepository<Domain.Entities.User, Guid> users,
        IIdentityContext identityContext,
        IClock clock,
        IFileUploadService fileUploadService,
        IAuthenticationService authenticationService)
    {
        _users = users;
        _identityContext = identityContext;
        _clock = clock;
        _fileUploadService = fileUploadService;
        _authenticationService = authenticationService;
    }

    public async Task<EditUserDetailsResult> HandleAsync(
        EditUserDetails command,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(_identityContext.Id, cancellationToken);
        if (user is null) return Error.New("");

        var newProfilePicture = user.ProfilePictureUrl;
        var newUsername = command.Username ?? user.Username;
        var newDisplayName = command.DisplayName ?? user.DisplayName;
        
        if (command.ProfilePicture is not null)
        {
            var result = await _fileUploadService.UploadAsync(
                new SingleFileUploadRequest
                {
                    UserId = _identityContext.Id,
                    File = command.ProfilePicture
                }, cancellationToken);

            result.Do(res => newProfilePicture = res.FileUrl);
        }

        if (command.NewPasswordInput is not null)
        {
            var result = await _authenticationService.ChangePasswordAsync(
                user.Id,
                command.NewPasswordInput.OldPassword,
                command.NewPasswordInput.NewPassword,
                cancellationToken);

            if (result.IsLeft) return result.LeftToArray()[0];
        }

        await _users.UpdateAsync(user.Id, user =>
        {
            user.Username = newUsername;
            user.DisplayName = newDisplayName;
            user.UpdatedAt = _clock.Now;
            user.ProfilePictureUrl = newProfilePicture;
        }, cancellationToken);
        
        return Unit.Default;
    }
}