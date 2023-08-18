using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;

namespace Chatify.Infrastructure.Authentication;

public static class Extensions
{
    public static Error? ToError(this IdentityResult identityResult)
        => identityResult.Succeeded
            ? default
            : Error.New(string.Join(".", identityResult.Errors
                .Select(e => Error.New(e.Description))));

    public static async Task<UserLoginInfo?> GetByNameAsync<TUser>(
        this UserManager<TUser> userManager,
        TUser user,
        string loginProvider) where TUser : class
    {
        var loginInfos = await userManager.GetLoginsAsync(user);
        return loginInfos
            .FirstOrDefault(i => i.LoginProvider == loginProvider);
    }
}