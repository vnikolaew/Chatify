using Chatify.Application.Common.Models;

namespace Chatify.Application.Authentication.Models;

public record SignInError(string? Message) : BaseError(Message);
