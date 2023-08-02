using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Commands;

using SendGroupChatMessageResult = Either<Error, Guid>;

public record SendGroupChatMessage(
    [Required] Guid GroupId,
    [Required, MinLength(1), MaxLength(500)]
    string Content,
    [Required] IEnumerable<InputFile>? Attachments = default
) : ICommand<SendGroupChatMessageResult>;

internal sealed class SendGroupChatMessageHandler
    : ICommandHandler<SendGroupChatMessage, SendGroupChatMessageResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IFileUploadService _fileUploadService;
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IChatMessageRepository _messages;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public SendGroupChatMessageHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IClock clock,
        IChatGroupMemberRepository members,
        IChatMessageRepository messages,
        IGuidGenerator guidGenerator,
        IEventDispatcher eventDispatcher,
        IFileUploadService fileUploadService)
    {
        _groups = groups;
        _identityContext = identityContext;
        _clock = clock;
        _members = members;
        _messages = messages;
        _guidGenerator = guidGenerator;
        _eventDispatcher = eventDispatcher;
        _fileUploadService = fileUploadService;
    }

    public async Task<SendGroupChatMessageResult> HandleAsync(
        SendGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await _groups.GetAsync(command.GroupId, cancellationToken);
        if (chatGroup is null) return Error.New($"Chat Group with Id '{command.GroupId}' was not found.");

        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id,
            cancellationToken);

        if (!userIsGroupMember) return Error.New("Current user is not a member of this Chat Group.");

        // TODO: Handle any file uploads:
        var uploadedFilesUrls = new List<string>();
        if (command.Attachments?.Any() ?? false)
        {
            var filesUploadResults = await _fileUploadService.UploadManyAsync(new MultipleFileUploadRequest
            {
                Files = command.Attachments,
                UserId = _identityContext.Id
            }, cancellationToken);
            
            uploadedFilesUrls = filesUploadResults
                .Where(r => r.IsRight)
                .Select(r => r.Match(_ => null!, r => r))
                .Select(r => r.FileUrl)
                .ToList();
        }

        var messageId = _guidGenerator.New();

        var message = new ChatMessage
        {
            UserId = _identityContext.Id,
            ChatGroup = chatGroup,
            Id = messageId,
            CreatedAt = _clock.Now,
            Attachments = uploadedFilesUrls,
            Content = command.Content,
        };

        await _messages.SaveAsync(message, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageSentEvent
        {
            UserId = message.UserId,
            Content = message.Content,
            GroupId = message.ChatGroupId,
            Timestamp = _clock.Now,
            MessageId = message.Id
        }, cancellationToken);

        return message.Id;
    }
}