using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenGraphNet;

namespace Playgrounds;

public class MarkdownPlaygrounds
{
    public static async Task Run()
    {
        const string text = """
                            This is a %%%text%%% with some @usermention@ [@User Name](U1231314)
                             *emphasis* ![alt text](image.jpg) [title](https://www.example.com) [redocly](https://www.npmjs.com/package/@redocly/cli)
                            """;

        // User mention format => [@User Name](U1231314)

        var pipelineBuilder = new MarkdownPipelineBuilder();
        // pipelineBuilder.InlineParsers.AddIfNotAlready<UserMentionsParser>();

        var pipeline = pipelineBuilder
            .Use<UserMentionExtension>()
            .Build();

        string html = Markdown.ToHtml(text, pipeline);

        var document = Markdown.Parse(text, pipeline);
        var descendants = document.Descendants<LinkInline>();

        var userMentions = document
            .Descendants<LinkInline>()
            .Where(li => li is { Title: not null, Url: not null, IsImage: false, FirstChild: LiteralInline literal }
                         && literal.Content.Text.StartsWith('@')
                         && li.Url.StartsWith("U")
                         && int.TryParse(li.Url[1..], out _))
            .ToList();

        foreach ( var userMention in userMentions )
        {
            var literals = userMention.Descendants<LiteralInline>();
            var userDisplayName = string.Join(" ",
                literals.Select(l => l.Content.ToString())
            );
        }

        var usersMentioned = userMentions
            .SelectMany(ei => ei.Descendants<LiteralInline>())
            .Select(li => li.Content.ToString())
            .ToList();

        foreach ( var linkInline in descendants )
        {
            var url = linkInline.Url;
            if ( !Uri.IsWellFormedUriString(url, UriKind.Absolute) ) continue;
            var openGraph = await OpenGraph.ParseUrlAsync(url!);
            Console.WriteLine(openGraph.Image);
        }
    }
}

public sealed class UserMentionsParser : EmphasisInlineParser
{
    public UserMentionsParser()
    {
        EmphasisDescriptors.Clear();
        EmphasisDescriptors.Add(new EmphasisDescriptor('@', 1, 1, false));
    }
}

public sealed class UserMentionExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        var parser = pipeline.InlineParsers.FindExact<EmphasisInlineParser>();
        if ( parser is not null && !parser.HasEmphasisChar('@') )
        {
            parser.EmphasisDescriptors.Add(new EmphasisDescriptor('@', 1, 1, false));
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if ( renderer is not HtmlRenderer ) return;

        var emphasisRenderer = renderer.ObjectRenderers.FindExact<EmphasisInlineRenderer>();
        if ( emphasisRenderer is null ) return;

        var previousTag = emphasisRenderer.GetTag;
        emphasisRenderer.GetTag = inline =>
            ( inline is { DelimiterCount: 1, DelimiterChar: '@' } ? "a" : null )
            ?? previousTag(inline);
    }
}