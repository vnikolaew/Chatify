using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenGraphNet;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal interface IOpenGraphMetadataEnricher
{
    Task<OpenGraphMetadata> GetAsync(
        string url, CancellationToken cancellationToken = default);
}

public record OpenGraphMetadata(
    string Title,
    string Description,
    Uri? AbsoluteUrl,
    Uri? Image,
    string? Type
);

internal sealed class OpenGraphMetadataEnricher : IOpenGraphMetadataEnricher
{
    public async Task<OpenGraphMetadata> GetAsync(
        string url, CancellationToken cancellationToken = default)
    {
        // Try and generate an OpenGraph metadata:
        var openGraph = await OpenGraph.ParseUrlAsync(url!, cancellationToken: cancellationToken);

        // Create a new OG object:
        var ogMetadata = new OpenGraphMetadata(
            openGraph.Title,
            openGraph.Title,
            openGraph.Url,
            openGraph.Image,
            openGraph.Type);

        return ogMetadata;
    }
}