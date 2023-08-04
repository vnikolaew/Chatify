using Chatify.Application.Authentication.Commands;
using LanguageExt;
using LanguageExt.Common;
using CancellationToken = System.Threading.CancellationToken;

namespace Chatify.Application.Authentication.Contracts;

public abstract class UserAuthResult
{
    public Guid UserId { get; set; }

    public string AuthenticationProvider { get; set; }
}

public sealed class UserSignedUpResult : UserAuthResult
{
}

public sealed class UserSignedInResult : UserAuthResult
{
}

public interface IAuthenticationService
{
    Task<Either<Error, UserSignedUpResult>> RegularSignUpAsync(
        RegularSignUp request,
        CancellationToken cancellationToken = default);

    Task<Either<Error, UserSignedInResult>> RegularSignInAsync(
        RegularSignIn request,
        CancellationToken cancellationToken = default);

    Task<Either<Error, UserSignedUpResult>> GoogleSignUpAsync(
        GoogleSignUp request,
        CancellationToken cancellationToken = default);

    Task<Either<Error, UserSignedUpResult>> FacebookSignUpAsync(
        FacebookSignUp request,
        CancellationToken cancellationToken = default);

    Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Either<Error, Unit>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}