using Chatify.ChatGroupsService.Messages;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Media = Chatify.Domain.Entities.Media;

namespace Chatify.ChatGroupsService.Services;

[Authorize]
internal sealed class ChatGroupsServicer(
    IChatGroupRepository groups,
    IIdentityContext identityContext,
    IGuidGenerator guidGenerator,
    IClock clock)
    : Messages.ChatGroupsServicer.ChatGroupsServicerBase
{
    public override async Task<CreateChatGroupResponse> CreateChatGroup(
        CreateChatGroupRequest request,
        ServerCallContext context)
    {
        context.CreatePropagationToken();
        var groupId = guidGenerator.New();
        var chatGroup = new ChatGroup
        {
            Id = groupId,
            About = request.About ?? string.Empty,
            Name = request.Name,
            AdminIds = new HashSet<Guid> { identityContext.Id },
            CreatorId = identityContext.Id,
            Picture = new Media
            {
                Id = Guid.Parse(request.GroupPicture.Id),
                Type = request.GroupPicture.Type,
                FileName = request.GroupPicture.FileName,
                MediaUrl = request.GroupPicture.MediaUrl
            },
            CreatedAt = clock.Now
        };
        
        await groups.SaveAsync(chatGroup, context.CancellationToken);
        return new CreateChatGroupResponse
        {
            Success = true,
            ChatGroupId = chatGroup.Id.ToString(),
            ErrorDetails = null
        };
    }
}