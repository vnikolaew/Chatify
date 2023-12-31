﻿using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using RegularSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

public sealed record RegularSignUp(
    [Required] string Username,
    [EmailAddress, Required] string Email,
    [Required] [Password] string Password) : ICommand<RegularSignUpResult>;

internal sealed class RegularSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<RegularSignUp, RegularSignUpResult>
{
    public async Task<RegularSignUpResult> HandleAsync(
        RegularSignUp command,
        CancellationToken cancellationToken = default)
    {
        var result =
            await authService
                .RegularSignUpAsync(command, cancellationToken);

        if ( result.IsT0 ) return new SignUpError(result.AsT0.Message);

        var signUpResult = result.AsT1;
        await eventDispatcher.PublishAsync(new UserSignedUpEvent
        {
            Timestamp = DateTime.Now,
            UserId = signUpResult.UserId,
            AuthenticationProvider = signUpResult.AuthenticationProvider
        }, cancellationToken);
        
        return Unit.Default;
    }
}