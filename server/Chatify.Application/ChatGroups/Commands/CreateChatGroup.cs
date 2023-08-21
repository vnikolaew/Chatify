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

internal sealed class CreateChatGroupHandler
    : ICommandHandler<CreateChatGroup, CreateChatGroupResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IChatGroupMemberRepository _members;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IIdentityContext _identityContext;
    private readonly IFileUploadService _fileUploadService;
    private readonly IGuidGenerator _guidGenerator;

    public CreateChatGroupHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher,
        IFileUploadService fileUploadService,
        IGuidGenerator guidGenerator, IChatGroupMemberRepository members, IClock clock)
    {
        _groups = groups;
        _identityContext = identityContext;
        _eventDispatcher = eventDispatcher;
        _fileUploadService = fileUploadService;
        _guidGenerator = guidGenerator;
        _members = members;
        _clock = clock;
    }

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
                UserId = _identityContext.Id
            };

            var result = await _fileUploadService.UploadAsync(fileUploadRequest, cancellationToken);
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

        var groupId = _guidGenerator.New();
        var chatGroup = new ChatGroup
        {
            Id = groupId,
            About = command.About ?? string.Empty,
            Name = command.Name,
            AdminIds = new HashSet<Guid> { _identityContext.Id },
            CreatorId = _identityContext.Id,
            Picture = groupPicture,
            CreatedAt = _clock.Now,
        };

        await _groups.SaveAsync(chatGroup, cancellationToken);

        var membershipId = _guidGenerator.New();
        var groupMember = new ChatGroupMember
        {
            Id = membershipId,
            CreatedAt = _clock.Now,
            ChatGroupId = chatGroup.Id,
            UserId = _identityContext.Id,
            Username = _identityContext.Username,
            MembershipType = 0
        };
        await _members.SaveAsync(groupMember, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatGroupCreatedEvent
        {
            CreatorId = chatGroup.CreatorId,
            GroupId = chatGroup.Id,
            Name = chatGroup.Name,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);

        return chatGroup.Id;
    }
}