using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenGraphNet;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal interface IOpenGraphMetadataEnricher
{
    Task<IEnumerable<OpenGraphMetadata>> GetAsync(
        string contentRaw, CancellationToken cancellationToken = default);
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
    public async Task<IEnumerable<OpenGraphMetadata>> GetAsync(
        string contentRaw, CancellationToken cancellationToken = default)
    {
        var document = Markdown.Parse(contentRaw);
        var contentLinks = document.Descendants<LinkInline>();
        var validLinks = contentLinks
            .Where(_ => !string.IsNullOrEmpty(_.Url) && Uri.IsWellFormedUriString(_.Url, UriKind.Absolute))
            .ToList();

        if ( validLinks.Count == 0 ) return Enumerable.Empty<OpenGraphMetadata>();
        
        List<OpenGraphMetadata> openGraphMetadatas = new();
        foreach ( var link in validLinks )
        {
            // Try and generate an OpenGraph metadata:
            var openGraph = await OpenGraph.ParseUrlAsync(link.Url!, cancellationToken: cancellationToken);

            // Create a new OG object:
            var ogMetadata = new OpenGraphMetadata(
                openGraph.Title,
                openGraph.Title,
                openGraph.Url,
                openGraph.Image,
                openGraph.Type);

            openGraphMetadatas.Add(ogMetadata);
        }

        return openGraphMetadatas;
    }
}