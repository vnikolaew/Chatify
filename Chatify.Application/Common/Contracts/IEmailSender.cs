namespace Chatify.Application.Common.Contracts;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}