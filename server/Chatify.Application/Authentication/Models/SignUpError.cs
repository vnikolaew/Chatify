using Chatify.Application.Common.Models;

namespace Chatify.Application.Authentication.Models;

public record SignUpError(string? Message) : BaseError(Message);
