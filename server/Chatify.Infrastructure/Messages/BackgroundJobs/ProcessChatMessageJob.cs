using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Quartz;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal sealed class ProcessChatMessageJob
    : BaseProcessChatMessageJob<Domain.Entities.ChatMessage>
{
    public override JobKey JobKey => new(nameof(ProcessChatMessageJob));
    
    protected override Func<Guid, CancellationToken, Task<Domain.Entities.ChatMessage?>> GetById { get; init; }

    public ProcessChatMessageJob(
        IChatMessageRepository messages,
        IOpenGraphMetadataEnricher openGraphMetadataEnricher, IMapper mapper)
        : base(mapper, openGraphMetadataEnricher, messages)
    {
        GetById = Messages.GetAsync;
    }
}
