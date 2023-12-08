using Cassandra.Mapping;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Quartz;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal static class JobBuilderExtensions
{
    public static JobBuilder WithMessageId(
        this JobBuilder builder,
        Guid messageId)
        => builder.UsingJobData(nameof(ProcessChatMessageJob.MessageId), messageId);
}

internal sealed class ProcessChatMessageReplyJob(
    IChatMessageReplyRepository messageReplies,
    IEventDispatcher eventDispatcher,
    IOpenGraphMetadataEnricher openGraphMetadataEnricher)
    : BaseProcessChatMessageJob<ChatMessageReply>(openGraphMetadataEnricher, messageReplies, eventDispatcher)
{
    public override JobKey JobKey => new(nameof(ProcessChatMessageReplyJob));

    protected override Task<ChatMessageReply?> GetById(Guid id,
        CancellationToken cancellationToken = default)
        => Messages.GetAsync(id, cancellationToken);
}