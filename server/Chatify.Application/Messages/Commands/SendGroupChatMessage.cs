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

internal sealed class SendGroupChatMessageHandler(IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IClock clock,
        IChatGroupMemberRepository members,
        IChatMessageRepository messages,
        IGuidGenerator guidGenerator,
        IEventDispatcher eventDispatcher,
        IFileUploadService fileUploadService)
    : ICommandHandler<SendGroupChatMessage, SendGroupChatMessageResult>
{
    public async Task<SendGroupChatMessageResult> HandleAsync(
        SendGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var userIsGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id,
            cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

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

        var messageId = guidGenerator.New();
        var message = new ChatMessage
        {
            UserId = identityContext.Id,
            ChatGroup = chatGroup,
            Id = messageId,
            CreatedAt = clock.Now,
            ChatGroupId = chatGroup.Id,
            Attachments = attachments,
            Content = command.Content
        };

        await messages.SaveAsync(message, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageSentEvent
        {
            UserId = message.UserId,
            Content = message.Content,
            GroupId = chatGroup.Id,
            Timestamp = clock.Now,
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
            UserId = identityContext.Id
        };

        return await fileUploadService.UploadManyAsync(uploadRequest, cancellationToken);
    }
}