using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Messages.Commands;

using SendGroupChatMessageResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Guid>;

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
        IFileUploadService fileUploadService
    )
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
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id,
            cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        // TODO: Handle file uploads:
        var uploadedFileResults = await HandleFileUploads(
            command.Attachments,
            cancellationToken);

        var uploadedFiles = uploadedFileResults
            .Where(r => r.IsT1)
            .Select(r => r.AsT1)
            .ToList();

        var attachments = uploadedFiles
            .Select(r => new Media
            {
                Id = r.FileId,
                MediaUrl = r.FileUrl,
                Type = r.FileType,
                FileName = r.FileName
            }).ToList();

        var messageId = _guidGenerator.New();
        var message = new ChatMessage
        {
            UserId = _identityContext.Id,
            ChatGroup = chatGroup,
            Id = messageId,
            CreatedAt = _clock.Now,
            Attachments = attachments,
            Content = command.Content
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

    private async Task<List<OneOf<Error, FileUploadResult>>> HandleFileUploads(
        IEnumerable<InputFile>? inputFiles,
        CancellationToken cancellationToken)
    {
        if ( inputFiles is null || !inputFiles.Any() ) return new List<OneOf<Error, FileUploadResult>>();

        var uploadRequest = new MultipleFileUploadRequest
        {
            Files = inputFiles,
            UserId = _identityContext.Id
        };

        return await _fileUploadService.UploadManyAsync(uploadRequest, cancellationToken);
    }
}