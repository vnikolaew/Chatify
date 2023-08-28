using System.ComponentModel.DataAnnotations;

namespace Chatify.Application.Authentication.Commands;

internal sealed class PasswordAttribute() : RegularExpressionAttribute(PasswordRegex)
{
    private const string PasswordRegex = @"^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{6,}$";
}