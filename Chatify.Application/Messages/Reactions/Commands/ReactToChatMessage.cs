using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Reactions.Commands;

using ReactToChatMessageResult = Either<Error, Guid>;

public record ReactToChatMessage(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required] sbyte ReactionType
) : ICommand<ReactToChatMessageResult>;

internal sealed class ReactToChatMessageHandler
    : ICommandHandler<ReactToChatMessage, ReactToChatMessageResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public ReactToChatMessageHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages, IEventDispatcher eventDispatcher, IClock clock,
        IGuidGenerator guidGenerator)
    {
        _members = members;
        _identityContext = identityContext;
        _messages = messages;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _guidGenerator = guidGenerator;
    }

    public async Task<ReactToChatMessageResult> HandleAsync(
        ReactToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if (!userIsGroupMember) return Error.New("");

        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if (message is null) return Error.New("");

        await _messages.UpdateAsync(message.Id, message =>
        {
            message.UpdatedAt = _clock.Now;
            if (!message.ReactionCounts.TryGetValue(command.ReactionType, out var count))
            {
                message.ReactionCounts[command.ReactionType] = 0;
            }
            else message.ReactionCounts[command.ReactionType]++;

        }, cancellationToken);
        
        var messageReaction = new ChatMessageReaction
        {
            
        }
    }
}