using System.Net;
using System.Security.Claims;
using AspNetCore.Identity.Cassandra.Models;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Authentication.External.Facebook;
using Chatify.Infrastructure.Authentication.External.Google;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Contexts;
using Humanizer;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using static Chatify.Infrastructure.Authentication.External.Constants.AuthProviders;

namespace Chatify.Infrastructure.Authentication;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly SignInManager<ChatifyUser> _signInManager;
    private readonly UserManager<ChatifyUser> _userManager;
    private readonly IUserRepository _users;
    private readonly IContext _context;

    private readonly IGoogleOAuthClient _googleOAuthClient;
    private readonly IFacebookOAuthClient _facebookOAuthClient;

    private const string AuthenticationTokenName = "access_token";

    public AuthenticationService(
        UserManager<ChatifyUser> userManager,
        SignInManager<ChatifyUser> signInManager,
        IGoogleOAuthClient googleOAuthClient,
        IFacebookOAuthClient facebookOAuthClient,
        IContext context,
        IUserRepository users)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _googleOAuthClient = googleOAuthClient;
        _facebookOAuthClient = facebookOAuthClient;
        _context = context;
        _users = users;
    }

    public async Task<Either<Error, UserSignedUpResult>> RegularSignUpAsync(
        RegularSignUp request,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null) return Error.New("User already exists");

        var chatifyUser = new ChatifyUser
        {
            Email = request.Email,
            UserName = request.Username,
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>
            {
                IPAddress.TryParse(_context.IpAddress, out var address)
                    ? IPAddress.IsLoopback(address) ? IPAddress.Loopback : address
                    : IPAddress.None
            }
        };

        var identityResult = await _userManager.CreateAsync(chatifyUser, request.Password);
        if (!identityResult.Succeeded)
        {
            return Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
        }

        identityResult = await _userManager.AddLoginAsync(
            chatifyUser,
            new UserLoginInfo(RegularLogin, chatifyUser.Id.ToString(),
                nameof(RegularLogin)));
        if (!identityResult.Succeeded)
        {
            return Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            chatifyUser, request.Password, isPersistent: true, lockoutOnFailure: true);

        return signInResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = RegularLogin
            }
            : Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
    }

    public async Task<Either<Error, UserSignedInResult>> RegularSignInAsync(
        RegularSignIn request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Error.New("User with the provided email was not found.");
        if (!user.EmailConfirmed) return Error.New("User has not confirmed their email yet.");

        var success = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!success) return Error.New("Invalid credentials.");

        var result = await _signInManager.PasswordSignInAsync(user,
            request.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        var ipAddress = IPAddress.Parse(_context.IpAddress);
        if (!user.DeviceIps.Contains(ipAddress))
        {
            await _users.UpdateAsync(user.Id, user => { user.DeviceIps.Add(ipAddress); }, cancellationToken);
        }

        return result.Succeeded
            ? new UserSignedInResult
            {
                UserId = user.Id,
                AuthenticationProvider = RegularLogin
            }
            : Error.New($"Error during sign-in: {result}");
    }

    private static ExternalLoginInfo GetGoogleExternalLoginInfoForUser(
        ClaimsPrincipal principal,
        string userId)
        => new(principal,
            Google,
            userId,
            Google.Titleize());

    public async Task<Either<Error, UserSignedUpResult>> GoogleSignUpAsync(
        GoogleSignUp request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _googleOAuthClient
            .GetUserInfoAsync(request.AccessToken, cancellationToken);

        if (userInfo is null) return Error.New("");

        var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
        if (existingUser is not null) return Error.New("User already exists");

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
            DeviceIps = new System.Collections.Generic.HashSet<IPAddress>()
            {
                IPAddress.Parse(_context.IpAddress)
            },
            EmailConfirmationTime = userInfo.VerifiedEmail ? DateTimeOffset.Now : default,
            ProfilePictureUrl = userInfo.Picture
        };
        chatifyUser.AddToken(new TokenInfo(Google, AuthenticationTokenName, request.AccessToken));

        var identityResult = await _userManager.CreateAsync(chatifyUser);
        if (!identityResult.Succeeded)
        {
            return Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
        }

        var principal = userInfo.ToClaimsPrincipal();
        identityResult = await _userManager.AddLoginAsync(
            chatifyUser,
            GetGoogleExternalLoginInfoForUser(principal, userInfo.Id));

        return identityResult.Succeeded
            ? new UserSignedUpResult
            {
                UserId = chatifyUser.Id,
                AuthenticationProvider = Google
            }
            : Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
    }

    public async Task<Either<Error, UserSignedUpResult>> FacebookSignUpAsync(
        FacebookSignUp request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _facebookOAuthClient
            .GetUserInfoAsync(request.AccessToken, cancellationToken);

        if (userInfo is null) return Error.New("User was not found or the provided access token is invalid.");
        if (userInfo.Email is null) return Error.New("Email address is invalid or not confirmed.");

        var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
        if (existingUser is not null) return Error.New("User already exists");

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
                IPAddress.Parse(_context.IpAddress)
            },
            EmailConfirmationTime = userInfo.Email is not null ? DateTimeOffset.Now : default,
            ProfilePictureUrl = userInfo.Picture.Data.Url
        };
        chatifyUser.AddToken(new TokenInfo(Facebook, AuthenticationTokenName, request.AccessToken));

        var identityResult = await _userManager.CreateAsync(chatifyUser);
        if (!identityResult.Succeeded)
        {
            return Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
        }

        var principal = userInfo.ToClaimsPrincipal();
        identityResult = await _userManager.AddLoginAsync(
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
            : Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return default;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }

    public async Task<Either<Error, Unit>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return default;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded
            ? Unit.Default
            : Error.Many(
                new Seq<Error>(
                    result.Errors.Select(e => Error.New(e.Description))
                )
            );
    }
}