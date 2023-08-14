using Cassandra.Mapping;
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

internal sealed class ProcessChatMessageReplyJob
    : BaseProcessChatMessageJob<Domain.Entities.ChatMessageReply>
{
    public override JobKey JobKey => new(nameof(ProcessChatMessageReplyJob));
    
    protected override Func<Guid, CancellationToken, Task<Domain.Entities.ChatMessageReply?>> GetById { get; init; }
    
    public ProcessChatMessageReplyJob(
        IChatMessageReplyRepository messageReplies,
        IEventDispatcher eventDispatcher,
        IOpenGraphMetadataEnricher openGraphMetadataEnricher)
        : base(openGraphMetadataEnricher, messageReplies, eventDispatcher)
    {
        GetById = Messages.GetAsync;
    }
}