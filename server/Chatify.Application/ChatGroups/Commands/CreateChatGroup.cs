using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Commands;
using Chatify.Application.User.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Common.Extensions;
using LanguageExt.Common;
using OneOf;
using Guid = System.Guid;

namespace Chatify.Application.ChatGroups.Commands;

using CreateChatGroupResult = OneOf<FileUploadError, Error, Guid>;

public record CreateChatGroup(
    [MinLength(0), MaxLength(500)] string? About,
    [Required, MinLength(3), MaxLength(100)]
    string Name,
    IEnumerable<Guid>? MemberIds,
    InputFile? InputFile) : ICommand<CreateChatGroupResult>;

internal sealed class CreateChatGroupHandler(
    IChatGroupsService chatGroupsService,
    IUsersService usersService,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher,
    IFileUploadService fileUploadService,
    IClock clock)
    : BaseCommandHandler<CreateChatGroup, CreateChatGroupResult>(eventDispatcher, identityContext, clock)
{
    private async Task<OneOf<Media, FileUploadError>> UploadFileAsync(
        InputFile inputFile,
        CancellationToken cancellationToken)
    {
        var fileUploadRequest = new SingleFileUploadRequest
        {
            File = inputFile,
            UserId = identityContext.Id
        };

        var result = await fileUploadService.UploadChatGroupMediaAsync(fileUploadRequest, cancellationToken);
        if ( result.Value is Error error ) return new FileUploadError(error.Message);

        var newMedia = result.Value as FileUploadResult;
        return new Media
        {
            Id = newMedia!.FileId,
            FileName = newMedia.FileName,
            MediaUrl = newMedia.FileUrl.Replace(Path.DirectorySeparatorChar, '/'),
            Type = newMedia.FileType
        };
    }

    public override async Task<CreateChatGroupResult> HandleAsync(
        CreateChatGroup command,
        CancellationToken cancellationToken = default)
    {
        Media? groupPicture = default;
        if ( command.InputFile is not null )
        {
            var result = await UploadFileAsync(command.InputFile, cancellationToken);
            if ( result.Value is Error error ) return new FileUploadError(error.Message);
            groupPicture = result.AsT0;
        }

        var response = await chatGroupsService.CreateChatGroupAsync(new CreateChatGroupRequest(
            command.About,
            command.Name,
            command.MemberIds,
            groupPicture), cancellationToken);
        if ( response.Value is Error error2 ) return error2;
        var chatGroupId = response.AsT1;

        // Create new memberships:
        var groupMember = new AddChatGroupMemberRequest(identityContext.Id, identityContext.Username, 0);

        var groupUsers = ( await usersService.GetByIds(command.MemberIds ?? new List<Guid>(), cancellationToken) )
            .Select(user => new AddChatGroupMemberRequest(user.Id, user.Username, 0))
            .Append(groupMember)
            .ToList();

        var membersResponse = await chatGroupsService.AddChatGroupMembersAsync(
            new AddChatGroupMembersRequest(chatGroupId, groupUsers), cancellationToken);

        var events = membersResponse
            .OfType2()
            .Select((m, i) => new ChatGroupMemberAddedEvent
            {
                GroupId = chatGroupId,
                Timestamp = clock.Now,
                MembershipType = groupUsers[i].MembershipType,
                MemberId = m,
                AddedById = identityContext.Id,
                AddedByUsername = identityContext.Username
            });

        await eventDispatcher.PublishAsync(events, cancellationToken);
        return chatGroupId;
    }
}