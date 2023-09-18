using Chatify.Application.Messages.Contracts;
using ReverseMarkdown;

namespace Chatify.Infrastructure.Messages.Services;

internal sealed class MessageContentNormalizer : IMessageContentNormalizer
{
    private static readonly Converter Converter = new(new Config
    {
        GithubFlavored = true,
        RemoveComments = true,
        SmartHrefHandling = true,
        UnknownTags = Config.UnknownTagsOption.PassThrough
    });

    public string Normalize(string content)
        => Converter.Convert(content);
}