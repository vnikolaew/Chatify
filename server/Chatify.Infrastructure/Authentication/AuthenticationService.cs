using System.Net;
using System.Security.Claims;
using AspNetCore.Identity.Cassandra.Models;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.User.Commands;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Authentication.External.Facebook;
using Chatify.Infrastructure.Authentication.External.Github;
using Chatify.Infrastructure.Authentication.External.Google;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Contexts;
using Humanizer;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using OneOf;
using static Chatify.Infrastructure.Authentication.External.Constants.AuthProviders;
using static Chatify.Infrastructure.Authentication.External.Constants.ClaimNames;
using Claim = System.Security.Claims.Claim;
using Constants = Chatify.Infrastructure.Data.Constants;
using IAuthenticationService = Chatify.Application.Authentication.Contracts.IAuthenticationService;

namespace Chatify.Infrastructure.Authentication;

public sealed class AuthenticationService(UserManager<ChatifyUser> userManager,
        SignInManager<ChatifyUser> signInManager,
        IGoogleOAuthClient googleOAuthClient,
        IFacebookOAuthClient facebookOAuthClient,
        IContext context,
        IUserRepository users,
        IIdentityContext identityContext,
        IGithubOAuthClient githubOAuthClient)
    : IAuthenticationService
{
    private const string AuthenticationTokenName = "access_token";
    private const string AuthenticationCodeName = "code";

    private static Error UserNotFoundError => Error.New("User not found.");
    private static Error InvalidOrUnconfirmedEmailError => Error.New("Email address is invalid or not confirmed.");
    private static Error UserLoginInfoNotFoundError => Error.New("Uer login information was not found.");


    public async Task<OneOf<Error, UserSignedUpResult>> RegularSignUpAsync(
        RegularSignUp request,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if ( existingUser is not null ) return Error.New("User already exists");

        var chatifyUser = new ChatifyUser
        {
            Email = request.Email,
            UserName = request.Username,
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>
            {
                IPAddress.TryParse(context.IpAddress, out var address)
                    ? IPAddress.IsLoopback(address) ? IPAddress.Loopback : address
                    : IPAddress.None
            },
            ProfilePicture = Constants.DefaultUserProfilePicture
        };

        var identityResult = await userManager
            .CreateAsync(chatifyUser, request.Password);
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        identityResult = await userManager.AddLoginAsync(
            chatifyUser,
            new UserLoginInfo(
                RegularLogin,
                chatifyUser.Id.ToString(),
                nameof(RegularLogin)));

        await userManager.AddClaimsAsync(
            chatifyUser,
            new List<Claim>
            {
                new(nameof(ChatifyUser.DisplayName), chatifyUser.DisplayName),
                new(Locale, identityContext.UserLocale ?? string.Empty),
                new("location", identityContext.UserLocation?.ToString() ?? string.Empty),
                new(External.Constants.ClaimNames.Picture, chatifyUser.ProfilePicture.MediaUrl)
            });

        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        var signInResult = await signInManager.PasswordSignInAsync(
            chatifyUser, request.Password, isPersistent: true, lockoutOnFailure: true);

        return signInResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = RegularLogin
            }
            : identityResult.ToError()!;
    }

    public async Task<OneOf<Error, UserSignedInResult>> RegularSignInAsync(
        RegularSignIn request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if ( user is null ) return Error.New("User with the provided email was not found.");

        var success = await userManager.CheckPasswordAsync(user, request.Password);
        if ( !success ) return Error.New("Invalid credentials.");

        var result = await signInManager.PasswordSignInAsync(user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: false);

        var ipAddress = IPAddress.Parse(context.IpAddress);
        if ( !user.DeviceIps.Contains(ipAddress) )
        {
            await users.UpdateAsync(user.Id, user => { user.DeviceIps.Add(ipAddress); }, cancellationToken);
        }

        return result.Succeeded
            ? new UserSignedInResult
            {
                UserId = user.Id,
                AuthenticationProvider = RegularLogin
            }
            : Error.New($"Error during sign-in: {result}");
    }

    public async Task<OneOf<Error, Unit>> SignOutAsync(
        CancellationToken cancellationToken = default)
    {
        await signInManager.SignOutAsync();
        return Unit.Default;
    }

    private static ExternalLoginInfo GetGoogleExternalLoginInfoForUser(
        ClaimsPrincipal principal,
        string userId)
        => new(principal,
            Google,
            userId,
            Google);

    private static ExternalLoginInfo GetGithubExternalLoginInfoForUser(
        ClaimsPrincipal principal,
        string userId)
        => new(principal,
            Github,
            userId,
            Github);

    public async Task<OneOf<Error, UserSignedUpResult>> GoogleSignUpAsync(
        GoogleSignUp request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await googleOAuthClient
            .GetUserInfoAsync(request.AccessToken, cancellationToken);
        if ( userInfo is null ) return UserNotFoundError;

        var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
        if ( existingUser is not null )
        {
            return await SignInWithLoginProvider(existingUser, Google);
        }

        var chatifyUser = new ChatifyUser
        {
            Email = userInfo.Email,
            UserName = userInfo.Name.Underscore(),
            DisplayName = $"{userInfo.GivenName} {userInfo.FamilyName}",
            Metadata = new Dictionary<string, string>
            {
                { "ExternalId", userInfo.Id },
                { "Locale", userInfo.Locale }
            },
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>
            {
                IPAddress.Parse(context.IpAddress)
            },
            EmailConfirmationTime = userInfo.VerifiedEmail ? DateTimeOffset.Now : default,
            ProfilePicture = new Media
            {
                Type = "",
                FileName = "",
                MediaUrl = userInfo.Picture
            }
        };
        chatifyUser.AddToken(new TokenInfo(Google, AuthenticationTokenName, request.AccessToken));

        var identityResult = await userManager.CreateAsync(chatifyUser);
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        var principal = userInfo.ToClaimsPrincipal();
        identityResult = await userManager.AddClaimsAsync(
            chatifyUser,
            new List<Claim>
            {
                new(External.Constants.ClaimNames.Picture, chatifyUser.ProfilePicture.MediaUrl),
                new(Locale, userInfo.Locale)
            });
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        identityResult = await userManager.AddLoginAsync(
            chatifyUser,
            GetGoogleExternalLoginInfoForUser(principal, userInfo.Id));

        return identityResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = Google
            }
            : identityResult.ToError()!;
    }


    public async Task<OneOf<Error, UserSignedUpResult>> GithubSignUpAsync(
        GithubSignUp request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await githubOAuthClient.GetUserInfoAsync(request.Code, cancellationToken);
        if ( userInfo is null ) return UserNotFoundError;

        var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
        if ( existingUser is not null )
        {
            return await SignInWithLoginProvider(existingUser, Github);
        }

        var chatifyUser = new ChatifyUser
        {
            Email = userInfo.Email,
            UserName = userInfo.Login,
            DisplayName = userInfo.Name,
            Metadata = new Dictionary<string, string>
            {
                { "ExternalId", userInfo.Id.ToString()! },
                { "Locale", userInfo.Location }
            },
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>
            {
                IPAddress.Parse(context.IpAddress)
            },
            EmailConfirmationTime = DateTimeOffset.Now,
            ProfilePicture = new Media
            {
                Type = "",
                FileName = "",
                MediaUrl = userInfo.AvatarUrl
            }
        };
        chatifyUser.AddToken(new TokenInfo(Github, AuthenticationCodeName, request.Code));

        var identityResult = await userManager.CreateAsync(chatifyUser);
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        var principal = userInfo.ToClaimsPrincipal();
        identityResult = await userManager.AddClaimsAsync(
            chatifyUser,
            new List<Claim>
            {
                new(External.Constants.ClaimNames.Picture, chatifyUser.ProfilePicture.MediaUrl),
                new(Locale, userInfo.Location)
            });

        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        identityResult = await userManager.AddLoginAsync(
            chatifyUser,
            GetGithubExternalLoginInfoForUser(principal, userInfo.Id.ToString()!));
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        var info = await userManager.GetByNameAsync(chatifyUser, Github);
        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey,
            isPersistent: true, true);

        return identityResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = Github
            }
            : identityResult.ToError()!;
    }

    public async Task<OneOf<Error, UserSignedUpResult>> FacebookSignUpAsync(
        FacebookSignUp request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await facebookOAuthClient
            .GetUserInfoAsync(request.AccessToken, cancellationToken);

        if ( userInfo is null ) return UserNotFoundError;
        if ( userInfo.Email is null ) return InvalidOrUnconfirmedEmailError;

        var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
        if ( existingUser is not null )
        {
            return await SignInWithLoginProvider(existingUser, Facebook);
        }

        var chatifyUser = new ChatifyUser
        {
            Email = userInfo.Email,
            UserName = userInfo.Name,
            Metadata = new Dictionary<string, string>
            {
                { "ExternalId", userInfo.Id }
            },
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>()
            {
                IPAddress.Parse(context.IpAddress)
            },
            EmailConfirmationTime = userInfo.Email is not null ? DateTimeOffset.Now : default,
            ProfilePicture = new Media
            {
                MediaUrl = userInfo.Picture.Data.Url
            }
        };
        chatifyUser.AddToken(new TokenInfo(Facebook, AuthenticationTokenName, request.AccessToken));

        var identityResult = await userManager.CreateAsync(chatifyUser);
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        var principal = userInfo.ToClaimsPrincipal();
        identityResult = await userManager.AddClaimsAsync(
            chatifyUser,
            new[] { new Claim(External.Constants.ClaimNames.Picture, userInfo.Picture.Data.Url) });
        if ( !identityResult.Succeeded ) return identityResult.ToError()!;

        identityResult = await userManager.AddLoginAsync(
            chatifyUser,
            new ExternalLoginInfo(principal,
                Facebook,
                userInfo.Id,
                Facebook.Titleize()));

        return identityResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = Facebook
            }
            : identityResult.ToError()!;
    }

    private async Task<OneOf<Error, UserSignedUpResult>> SignInWithLoginProvider(
        ChatifyUser existingUser,
        string loginProvider)
    {
        var loginInfo = await userManager.GetByNameAsync(
            existingUser,
            loginProvider);
        if ( loginInfo is null ) return UserLoginInfoNotFoundError;

        var result = await signInManager.ExternalLoginSignInAsync(
            loginInfo.LoginProvider,
            loginInfo.ProviderKey,
            true, true);

        return result.Succeeded
            ? new UserSignedUpResult
            {
                UserId = existingUser.Id,
                AuthenticationProvider = loginInfo.LoginProvider
            }
            : Error.New("");
    }

    public async Task<OneOf<UserNotFound, string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if ( user is null ) return new UserNotFound();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }

    public async Task<OneOf<UserNotFound, PasswordChangeError, Unit>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if ( user is null ) return new UserNotFound();

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded
            ? Unit.Default
            : new PasswordChangeError(result.Errors.First().Description);
    }
}