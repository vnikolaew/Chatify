﻿using Chatify.Application.Authentication.Commands;
using Chatify.Application.User.Commands;
using Chatify.Application.User.Common;
using LanguageExt;
using LanguageExt.Common;
using OneOf;
using CancellationToken = System.Threading.CancellationToken;

namespace Chatify.Application.Authentication.Contracts;

public abstract class UserAuthResult
{
    public Guid UserId { get; init; }

    public string AuthenticationProvider { get; init; }
}

public sealed class UserSignedUpResult : UserAuthResult
{
}

public sealed class UserSignedInResult : UserAuthResult
{
}

public interface IAuthenticationService
{
    Task<OneOf<Error, UserSignedUpResult>> RegularSignUpAsync(
        RegularSignUp request,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, UserSignedInResult>> RegularSignInAsync(
        RegularSignIn request,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, Unit>> SignOutAsync(CancellationToken cancellationToken = default);

    Task<OneOf<Error, UserSignedUpResult>> GoogleSignUpAsync(
        GoogleSignUp request,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, UserSignedUpResult>> GithubSignUpAsync(
        GithubSignUp request,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, UserSignedUpResult>> FacebookSignUpAsync(
        FacebookSignUp request,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserNotFound, string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, Unit>> AcceptCookiePolicy(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, Unit>> DeclineCookiePolicy(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OneOf<UserNotFound, PasswordChangeError, Unit>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<OneOf<Error, Unit>> RefreshUserClaimsAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken = default);
}