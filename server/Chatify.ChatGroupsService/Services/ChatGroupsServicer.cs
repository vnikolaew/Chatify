using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Chatify.ChatGroupsService.Services;

[Authorize]
internal sealed class ChatGroupsServicer
    : ChatGroupsService.ChatGroupsServicer.ChatGroupsServicerBase
{
    private readonly IChatGroupRepository _groups;
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public ChatGroupsServicer(IChatGroupRepository groups, IIdentityContext identityContext,
        IGuidGenerator guidGenerator, IClock clock, IChatGroupMemberRepository members)
    {
        _groups = groups;
        _identityContext = identityContext;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _members = members;
    }

    public override async Task<AddChatGroupMembersResponse> AddChatGroupMembers(
        AddChatGroupMembersRequest request,
        ServerCallContext context)
    {
        var members = request.Members.Select(m => new ChatGroupMember()
        {
            Id = _guidGenerator.New(),
            CreatedAt = _clock.Now,
            ChatGroupId = Guid.Parse(request.ChatGroupId),
            UserId = Guid.Parse(m.UserId),
            Username = m.Username,
            MembershipType = ( sbyte )( m.MembershipType - 1 )
        });

        var groupMembers = await members
            .Select(member => _members.SaveAsync(member, context.CancellationToken))
            .ToList();

        return new AddChatGroupMembersResponse
        {
            ChatGroupId = request.ChatGroupId,
            Results =
            {
                groupMembers.Select(m => new AddChatGroupMemberResponse
                {
                    UserId = m?.UserId.ToString(),
                    Success = m is not null,
                    MemberId = m?.Id.ToString()
                })
            }
        };
    }

    public override async Task<CreateChatGroupResponse> CreateChatGroup(
        CreateChatGroupRequest request,
        ServerCallContext context)
    {
        var groupId = _guidGenerator.New();
        var chatGroup = new ChatGroup
        {
            Id = groupId,
            About = request.About ?? string.Empty,
            Name = request.Name,
            AdminIds = new HashSet<Guid> { _identityContext.Id },
            CreatorId = _identityContext.Id,
            Picture = new Domain.Entities.Media
            {
                Id = Guid.Parse(request.GroupPicture.Id),
                Type = request.GroupPicture.Type,
                FileName = request.GroupPicture.FileName,
                MediaUrl = request.GroupPicture.MediaUrl
            },
            CreatedAt = _clock.Now
        };

        await _groups.SaveAsync(chatGroup, context.CancellationToken);
        return new CreateChatGroupResponse
        {
            Success = true,
            ChatGroupId = chatGroup.Id.ToString(),
            ErrorDetails = null
        };
    }
}