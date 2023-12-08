using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Quartz;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal sealed class ProcessChatMessageJob(
        IChatMessageRepository messages,
        IEventDispatcher eventDispatcher,
        IOpenGraphMetadataEnricher openGraphMetadataEnricher)
    : BaseProcessChatMessageJob<ChatMessage>(openGraphMetadataEnricher, messages, eventDispatcher)
{
    public override JobKey JobKey => new(nameof(ProcessChatMessageJob));

    protected override Task<ChatMessage?> GetById(Guid id,
        CancellationToken cancellationToken = default)
        => Messages.GetAsync(id, cancellationToken);
}