using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Quartz;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal sealed class ProcessChatMessageJob
    : BaseProcessChatMessageJob<Domain.Entities.ChatMessage>
{
    public override JobKey JobKey => new(nameof(ProcessChatMessageJob));
    
    protected override Func<Guid, CancellationToken, Task<Domain.Entities.ChatMessage?>> GetById { get; init; }

    public ProcessChatMessageJob(
        IChatMessageRepository messages,
        IEventDispatcher eventDispatcher,
        IOpenGraphMetadataEnricher openGraphMetadataEnricher)
        : base(openGraphMetadataEnricher, messages, eventDispatcher)
    {
        GetById = Messages.GetAsync;
    }
}