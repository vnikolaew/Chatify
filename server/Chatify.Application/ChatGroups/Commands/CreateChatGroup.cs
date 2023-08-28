using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Commands;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;
using OneOf;
using Guid = System.Guid;

namespace Chatify.Application.ChatGroups.Commands;

using CreateChatGroupResult = OneOf<FileUploadError, Guid>;

public record CreateChatGroup(
    [MinLength(0), MaxLength(500)] string? About,
    [Required, MinLength(3), MaxLength(100)]
    string Name,
    InputFile? InputFile) : ICommand<CreateChatGroupResult>;

internal sealed class CreateChatGroupHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher,
        IFileUploadService fileUploadService,
        IGuidGenerator guidGenerator, IChatGroupMemberRepository members, IClock clock)
    : ICommandHandler<CreateChatGroup, CreateChatGroupResult>
{
    public async Task<CreateChatGroupResult> HandleAsync(
        CreateChatGroup command,
        CancellationToken cancellationToken = default)
    {
        Media? groupPicture = default;
        if ( command.InputFile is not null )
        {
            var fileUploadRequest = new SingleFileUploadRequest
            {
                File = command.InputFile,
                UserId = identityContext.Id
            };

            var result = await fileUploadService.UploadAsync(fileUploadRequest, cancellationToken);
            if ( result.Value is Error error ) return new FileUploadError(error.Message);

            var newMedia = result.AsT1!;
            groupPicture = new Media
            {
                Id = newMedia.FileId,
                FileName = newMedia.FileName,
                MediaUrl = newMedia.FileUrl,
                Type = newMedia.FileType
            };
        }

        var groupId = guidGenerator.New();
        var chatGroup = new ChatGroup
        {
            Id = groupId,
            About = command.About ?? string.Empty,
            Name = command.Name,
            AdminIds = new HashSet<Guid> { identityContext.Id },
            CreatorId = identityContext.Id,
            Picture = groupPicture,
            CreatedAt = clock.Now,
        };

        await groups.SaveAsync(chatGroup, cancellationToken);

        var membershipId = guidGenerator.New();
        var groupMember = new ChatGroupMember
        {
            Id = membershipId,
            CreatedAt = clock.Now,
            ChatGroupId = chatGroup.Id,
            UserId = identityContext.Id,
            Username = identityContext.Username,
            MembershipType = 0
        };
        await members.SaveAsync(groupMember, cancellationToken);

        await eventDispatcher.PublishAsync(new ChatGroupCreatedEvent
        {
            CreatorId = chatGroup.CreatorId,
            GroupId = chatGroup.Id,
            Name = chatGroup.Name,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        return chatGroup.Id;
    }
}