using System.Text.Json;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Events;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Quartz;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal abstract class BaseProcessChatMessageJob<TMessage>(IOpenGraphMetadataEnricher openGraphMetadataEnricher,
        IDomainRepository<TMessage, Guid> messages,
        IEventDispatcher eventDispatcher)
    : IJob
    where TMessage : ChatMessage
{
    public Guid MessageId { private get; set; }

    // private readonly ILogger<ProcessChatMessageReplyJob> _logger;

    protected abstract Func<Guid, CancellationToken, Task<TMessage?>> GetById { get; init; }

    protected readonly IDomainRepository<TMessage, Guid> Messages = messages;

    protected readonly IEventDispatcher EventDispatcher = eventDispatcher;

    public abstract JobKey JobKey { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        var message = await GetById(MessageId, context.CancellationToken);
        if ( message is null )
        {
            // _logger.LogInformation("Message with {Id} not found. ", MessageId);
            return;
        }

        var markDownObjects = ParseMarkdownContent(message.Content).ToList();
        var validLinks = markDownObjects
            .OfType<LinkInline>()
            .Where(_ => !string.IsNullOrEmpty(_.Url) && Uri.IsWellFormedUriString(_.Url, UriKind.Absolute))
            .ToList();

        var metadataTasks = validLinks
            .Select(async l => await openGraphMetadataEnricher.GetAsync(l.Url!, context.CancellationToken))
            .ToList();

        var metadatas = await Task.WhenAll(metadataTasks);
        if ( !metadatas.Any() )
        {
            // _logger.LogInformation("No generated OG metadatas");
            return;
        }

        var ogMetadataStrings = JsonSerializer.Serialize(metadatas.ToList());
        await Messages.UpdateAsync(
            MessageId,
            message => message.Metadata.Add("og-metadatas", ogMetadataStrings));

        // Process any user mentions:
        var userMentionLinks = markDownObjects
            .OfType<LinkInline>()
            .Where(l => l is { Url: not null, IsImage: false, Label: not null }
                        && l.Label.StartsWith('@')
                        && l.Url.StartsWith('U')
                        && Guid.TryParse(l.Url[1..], out _))
            .ToList();
        if(!userMentionLinks.Any()) return;
        
        var userMentionedIds = userMentionLinks
            .Select(l => Guid.Parse(l.Url![1..]))
            .ToList();
        await EventDispatcher.PublishAsync(new UsersMentionedInChatMessageEvent
        {
            MessageId = message.Id,
            ChatGroupId = message.ChatGroupId,
            UsersMentionedIds = userMentionedIds,
            Timestamp = message.CreatedAt.DateTime
        }, context.CancellationToken);
    }

    private static IEnumerable<MarkdownObject> ParseMarkdownContent(string contentRaw)
        => Markdown.Parse(contentRaw).Descendants();
}