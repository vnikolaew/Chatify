﻿using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Commands;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using CreateChatGroupResult = OneOf<FileUploadError, Guid>;

public record CreateChatGroup(
    [MinLength(0), MaxLength(500)] string? About,
    [Required, MinLength(3), MaxLength(100)]
    string Name,
    InputFile? InputFile) : ICommand<CreateChatGroupResult>;

internal sealed class CreateChatGroupHandler : ICommandHandler<CreateChatGroup, CreateChatGroupResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IIdentityContext _identityContext;
    private readonly IFileUploadService _fileUploadService;
    private readonly IGuidGenerator _guidGenerator;

    public CreateChatGroupHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher,
        IFileUploadService fileUploadService,
        IGuidGenerator guidGenerator)
    {
        _groups = groups;
        _identityContext = identityContext;
        _eventDispatcher = eventDispatcher;
        _fileUploadService = fileUploadService;
        _guidGenerator = guidGenerator;
    }

    public async Task<CreateChatGroupResult> HandleAsync(
        CreateChatGroup command,
        CancellationToken cancellationToken = default)
    {
        string groupPictureUrl = default!;
        if ( command.InputFile is not null )
        {
            var fileUploadRequest = new SingleFileUploadRequest
            {
                File = command.InputFile,
                UserId = _identityContext.Id
            };

            var result = await _fileUploadService.UploadAsync(fileUploadRequest, cancellationToken);
            if ( result.IsLeft ) return new FileUploadError(result.LeftToArray()[0].Message);

            groupPictureUrl = result.Match(r => r.FileUrl, _ => null!);
        }

        var groupId = _guidGenerator.New();
        var chatGroup = new ChatGroup
        {
            Id = groupId,
            About = command.About ?? string.Empty,
            Name = command.Name,
            AdminIds = new System.Collections.Generic.HashSet<Guid> { _identityContext.Id },
            CreatorId = _identityContext.Id,
            PictureUrl = groupPictureUrl
        };

        await _groups.SaveAsync(chatGroup, cancellationToken);
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