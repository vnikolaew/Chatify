using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
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
    IEnumerable<Guid>? MemberIds,
    InputFile? InputFile) : ICommand<CreateChatGroupResult>;

internal sealed class CreateChatGroupHandler(
    IDomainRepository<ChatGroup, Guid> groups,
    IUserRepository users,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher,
    IFileUploadService fileUploadService,
    IGuidGenerator guidGenerator,
    IChatGroupMemberRepository members,
    IClock clock)
    : BaseCommandHandler<CreateChatGroup, CreateChatGroupResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<CreateChatGroupResult> HandleAsync(
        CreateChatGroup command,
        CancellationToken cancellationToken = default)
    {
        Media? groupPicture = default;
        if ( command.InputFile is not null )
        {
            var fileUploadRequest = new SingleFileUploadRequest
            {
                File = command.InputFile,
                UserId = identityContext.Id,
            };

            var result = await fileUploadService.UploadChatGroupMediaAsync(fileUploadRequest, cancellationToken);
            if ( result.Value is Error error ) return new FileUploadError(error.Message);

            var newMedia = result.Value as FileUploadResult;
            groupPicture = new Media
            {
                Id = newMedia!.FileId,
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

        // Create new memberships:
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
        var membersTasks = ( await users.GetByIds(command.MemberIds ?? new List<Guid>(), cancellationToken) )!
            .Select(user => new ChatGroupMember
            {
                Id = guidGenerator.New(),
                CreatedAt = clock.Now,
                ChatGroupId = chatGroup.Id,
                UserId = user.Id,
                Username = user.Username,
                MembershipType = 0
            })
            .Append(groupMember)
            .Select(member => members.SaveAsync(member, cancellationToken))
            .ToList();

        await Task.WhenAll(membersTasks);
        var @events = membersTasks
            .Select(_ => _.Result)
            .Select(m => new ChatGroupMemberAddedEvent
            {
                GroupId = groupId,
                Timestamp = clock.Now,
                MembershipType = m.MembershipType,
                MemberId = m.Id,
                AddedById = chatGroup.CreatorId,
                AddedByUsername = identityContext.Username
            });

        await eventDispatcher.PublishAsync(@events, cancellationToken);
        return chatGroup.Id;
    }
}