using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Commands;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;

namespace Chatify.Application.Messages.Common;

public class AttachmentOperationHandler
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IChatGroupAttachmentRepository _attachments;
    private readonly IClock _clock;
    private readonly IIdentityContext _identityContext;

    public AttachmentOperationHandler(
        IFileUploadService fileUploadService,
        IIdentityContext identityContext,
        IChatGroupAttachmentRepository attachments, IClock clock)
    {
        _fileUploadService = fileUploadService;
        _identityContext = identityContext;
        _attachments = attachments;
        _clock = clock;
    }

    private async Task HandleAddAsync(
        ChatMessage message,
        AddAttachmentOperation addAttachmentOperation,
        CancellationToken cancellationToken = default)
    {
        var uploadResult = await _fileUploadService.UploadAsync(
                new SingleFileUploadRequest
                {
                    UserId = _identityContext.Id,
                    File = addAttachmentOperation.InputFile
                }, cancellationToken);
        if ( uploadResult.IsT0 ) return;

        var fileUploadResult = uploadResult.AsT1;
        var newMedia = new Media
        {
            Id = fileUploadResult.FileId,
            FileName = fileUploadResult.FileName,
            MediaUrl = fileUploadResult.FileUrl,
            Type = fileUploadResult.FileType,
        };
        message.AddAttachment(newMedia);
        
        // Add attachment to Attachments "View" table:
        var attachment = new ChatGroupAttachment
        {
            AttachmentId = fileUploadResult.FileId,
            UserId = message.UserId,
            CreatedAt = _clock.Now,
            Username = _identityContext.Username,
            MediaInfo = newMedia,
            ChatGroupId = message.ChatGroupId
        };
        await _attachments.SaveAsync(attachment, cancellationToken);
    }

    private async Task HandleDeleteAsync(
        ChatMessage message,
        DeleteAttachmentOperation deleteAttachmentOperation,
        CancellationToken cancellationToken = default)
    {
        var media = message
            .Attachments
            .FirstOrDefault(m => m.Id == deleteAttachmentOperation.AttachmentId);
        if ( media is null ) return;

        var deleteResult = await _fileUploadService
            .DeleteAsync(new SingleFileDeleteRequest
            {
                FileUrl = media.MediaUrl,
                UserId = _identityContext.Id
            }, cancellationToken);
        
        deleteResult.MapT1(async _ =>
        {
            message.DeleteAttachment(media.Id);
            await _attachments.DeleteAsync(media.Id, cancellationToken);
        });
    }

    public async Task HandleAsync(
        ChatMessage message,
        IEnumerable<AttachmentOperation> attachmentOperations,
        CancellationToken cancellationToken = default)
    {
        foreach ( var attachmentOperation in attachmentOperations )
        {
            await ( attachmentOperation switch
            {
                AddAttachmentOperation add => HandleAddAsync(message, add, cancellationToken),
                DeleteAttachmentOperation delete => HandleDeleteAsync(message, delete, cancellationToken),
                _ => Task.CompletedTask
            } );
        }
    }
}