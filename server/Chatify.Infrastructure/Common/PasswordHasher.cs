using Chatify.Application.Common.Contracts;
using Chatify.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Chatify.Infrastructure.Common;

public class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<ChatifyUser> _internalHasher;

    public PasswordHasher(IPasswordHasher<ChatifyUser> internalHasher)
        => _internalHasher = internalHasher;

    public string Secure(string password)
        => _internalHasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => _internalHasher
               .VerifyHashedPassword(null!, hashedPassword, providedPassword)
           == PasswordVerificationResult.Success;
}