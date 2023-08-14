using System.Text.Json;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Quartz;
using ChatMessageReply = Chatify.Infrastructure.Data.Models.ChatMessageReply;

namespace Chatify.Infrastructure.Messages.BackgroundJobs;

internal abstract class BaseProcessChatMessageJob<TMessage> : IJob
    where TMessage : ChatMessage
{
    public Guid MessageId { private get; set; }
    
    private readonly IMapper _mapper;
    
    private readonly IOpenGraphMetadataEnricher _openGraphMetadataEnricher;
    // private readonly ILogger<ProcessChatMessageReplyJob> _logger;

    protected abstract Func<Guid, CancellationToken, Task<TMessage?>> GetById { get; init; }

    protected readonly IDomainRepository<TMessage, Guid> Messages;
    
    public abstract JobKey JobKey { get; }
        
    public BaseProcessChatMessageJob(
        IMapper mapper,
        IOpenGraphMetadataEnricher openGraphMetadataEnricher,
        IDomainRepository<TMessage, Guid> messages)
    {
        _mapper = mapper;
        _openGraphMetadataEnricher = openGraphMetadataEnricher;
        Messages = messages;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var message = await GetById(MessageId, context.CancellationToken);
        if ( message is null )
        {
            // _logger.LogInformation("Message with {Id} not found. ", MessageId);
            return;
        }

        var metadatas = await _openGraphMetadataEnricher
            .GetAsync(message.Content, context.CancellationToken);
        if ( !metadatas.Any() )
        {
            // _logger.LogInformation("No generated OG metadatas");
            return;
        }
            
        var ogMetadataStrings = JsonSerializer.Serialize(metadatas.ToList());
        await _mapper.UpdateAsync<ChatMessageReply>(" SET metadata['og-metadatas'] = ? WHERE id = ?",
            ogMetadataStrings,
            MessageId);

        await Messages.UpdateAsync(
            MessageId,
            message => message.Metadata.Add("og-metadatas", ogMetadataStrings));
    }
}