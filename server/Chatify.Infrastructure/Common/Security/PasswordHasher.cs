using Chatify.Application.Common.Contracts;
using Chatify.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Chatify.Infrastructure.Common.Security;

public class PasswordHasher(IPasswordHasher<ChatifyUser> internalHasher) : IPasswordHasher
{
    public string Secure(string password)
        => internalHasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => internalHasher
               .VerifyHashedPassword(null!, hashedPassword, providedPassword)
           == PasswordVerificationResult.Success;
}