using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;

namespace Chatify.Application.Authentication.Commands;

using GithubSignUpResult = OneOf.OneOf<SignUpError, Unit>;

public sealed record GithubSignUp([Required] string Code) : ICommand<GithubSignUpResult>;

internal sealed class GithubSignUpHandler(
        IAuthenticationService authenticationService,
        IEventDispatcher eventDispatcher, IClock clock)
    : ICommandHandler<GithubSignUp, GithubSignUpResult>
{
    public async Task<GithubSignUpResult> HandleAsync(
        GithubSignUp command, CancellationToken cancellationToken = default)
    {
        var result = await authenticationService
            .GithubSignUpAsync(command, cancellationToken);

        if ( result.IsT0 ) return new SignUpError(result.AsT0.Message);

        var events = new IDomainEvent[]
        {
            new UserSignedUpEvent
            {
                Timestamp = clock.Now,
                UserId = result.AsT1.UserId,
                AuthenticationProvider = result.AsT1.AuthenticationProvider
            },
            new UserSignedInEvent
            {
                Timestamp = clock.Now,
                AuthenticationProvider = result.AsT1.AuthenticationProvider,
                UserId = result.AsT1.UserId
            }
        };

        await eventDispatcher.PublishAsync(events, cancellationToken);
        return Unit.Default;
    }
}