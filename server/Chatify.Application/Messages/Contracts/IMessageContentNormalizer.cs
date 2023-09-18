namespace Chatify.Application.Messages.Contracts;

public interface IMessageContentNormalizer
{
    string Normalize(string content);
}