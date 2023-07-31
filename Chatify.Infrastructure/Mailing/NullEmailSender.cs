using Chatify.Application.Common.Contracts;

namespace Chatify.Infrastructure.Mailing;

public sealed class NullEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}