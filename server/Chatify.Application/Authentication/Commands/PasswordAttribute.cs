using System.ComponentModel.DataAnnotations;

namespace Chatify.Application.Authentication.Commands;

internal sealed class PasswordAttribute : RegularExpressionAttribute
{
    private const string PasswordRegex = @"^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{6,}$";

    public PasswordAttribute() : base(PasswordRegex)
    {
    }
}