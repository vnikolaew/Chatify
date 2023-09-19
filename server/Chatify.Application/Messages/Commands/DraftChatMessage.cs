using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.Messages.Commands.Common;
using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.Messages.Commands;

using DraftChatMessageResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Guid>;

public record DraftChatMessage(
    [Required] Guid GroupId,
    [Required, MinLength(1), MaxLength(500)]
    string Content,
    IEnumerable<InputFile>? Attachments = default
) : SendChatMessageBase<DraftChatMessageResult>(Content, Attachments);

internal sealed class DraftChatMessageHandler(
    IChatGroupRepository groups,
        IMessageContentNormalizer contentNormalizer,
    IDomainRepository<ChatMessageDraft, Guid> drafts,
    IChatGroupMemberRepository members,
    IGuidGenerator guidGenerator,
    IFileUploadService fileUploadService,
    IEventDispatcher eventDispatcher,
    IClock clock,
    IIdentityContext identityContext) : SendChatMessageBaseHandler<DraftChatMessage, DraftChatMessageResult>(
    fileUploadService, identityContext)
{
    public override async Task<DraftChatMessageResult> HandleAsync(
        DraftChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var userIsGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id,
            cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

        // Handle file uploads:
        var uploadedFileResults = await HandleFileUploads(
            command.Attachments,
            cancellationToken);
        var attachments = GetMediae(uploadedFileResults);

        var messageId = guidGenerator.New();
        var draft = new ChatMessageDraft
        {
            UserId = identityContext.Id,
            ChatGroup = chatGroup,
            Id = messageId,
            CreatedAt = clock.Now,
            ChatGroupId = chatGroup.Id,
            Attachments = attachments,
            Content = contentNormalizer.Normalize(command.Content)
        };

        await drafts.SaveAsync(draft, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageDraftedEvent
        {
            UserId = draft.UserId,
            Content = draft.Content,
            GroupId = chatGroup.Id,
            Timestamp = clock.Now,
            MessageId = draft.Id,
            Attachments = draft.Attachments.ToList()
        }, cancellationToken);

        return draft.Id;
    }
}