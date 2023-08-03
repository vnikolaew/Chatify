namespace Chatify.Application.Common.Contracts;

public interface IPasswordHasher
{
    string Secure(string password);
    bool Verify(string hashedPassword, string providedPassword);
}