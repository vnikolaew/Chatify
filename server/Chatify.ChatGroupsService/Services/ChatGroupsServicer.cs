using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Services.Shared.ChatGroups;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Chatify.ChatGroupsService.Services;

[Authorize]
internal sealed class ChatGroupsServicer
    : Chatify.Services.Shared.ChatGroups.ChatGroupsServicer.ChatGroupsServicerBase
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

    public new async Task<AddChatGroupMembersResponse> AddChatGroupMembers(
        AddChatGroupMembersRequest request,
        ServerCallContext context)
    {
        var group = await _groups.GetAsync(
            Guid.Parse(request.ChatGroupId), context.CancellationToken);

        if ( group is null )
        {
            return new AddChatGroupMembersResponse
            {
                ChatGroupId = request.ChatGroupId,
                Results =
                {
                    new AddChatGroupMemberResponse
                    {
                        Success = false,
                        ErrorDetails = new ErrorDetails
                        {
                            Errors =
                            {
                                new ErrorDetail { Message = "Group was not found." }
                            }
                        }
                    }
                }
            };
        }

        if ( !group.AdminIds.Contains(_identityContext.Id) )
        {
            return new AddChatGroupMembersResponse
            {
                ChatGroupId = request.ChatGroupId,
                Results =
                {
                    new AddChatGroupMemberResponse
                    {
                        Success = false,
                        ErrorDetails = new ErrorDetails
                        {
                            Errors =
                            {
                                new ErrorDetail { Message = "User is not a group admin." }
                            }
                        }
                    }
                }
            };
        }

        var members = request
            .Members
            .Select(m => new ChatGroupMember
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

    public new async Task<CreateChatGroupResponse> CreateChatGroup(
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

    public new async Task<AddChatGroupAdminResponse> AddChatGroupAdmin(
        AddChatGroupAdminRequest request,
        ServerCallContext context)
    {
        var chatGroup = await _groups.GetAsync(
            Guid.Parse(request.ChatGroupId),
            context.CancellationToken);
        if ( chatGroup is null )
        {
            return new AddChatGroupAdminResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails
                {
                    Errors = { new ErrorDetail { Message = "Chat group was not found", ErrorCode = "1" } }
                }
            };
        }

        var isMember = await _members.Exists(chatGroup.Id, _identityContext.Id, context.CancellationToken);
        if ( !isMember )
        {
            return new AddChatGroupAdminResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails
                {
                    Errors = { new ErrorDetail { Message = "User is not member", ErrorCode = "2" } }
                }
            };
        }

        if ( !chatGroup.HasAdmin(_identityContext.Id) )
        {
            return new AddChatGroupAdminResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails
                {
                    Errors = { new ErrorDetail { Message = "User is not a group admin", ErrorCode = "2" } }
                }
            };
        }

        if ( chatGroup.HasAdmin(Guid.Parse(request.AdminId)) )
        {
            return new AddChatGroupAdminResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails
                {
                    Errors = { new ErrorDetail { Message = "User is already a group admin", ErrorCode = "2" } }
                }
            };
        }

        await _groups.UpdateAsync(chatGroup, group =>
        {
            group.AddAdmin(Guid.Parse(request.AdminId));
            group.UpdatedAt = _clock.Now;
        }, context.CancellationToken);

        return new AddChatGroupAdminResponse { Success = true };
    }

    public new async Task<UpdateChatGroupDetailsResponse> UpdateChatGroupDetails(
        UpdateChatGroupDetailsRequest request,
        ServerCallContext context)
    {
        var group = await _groups.GetAsync(Guid.Parse(request.ChatGroupId), context.CancellationToken);
        if ( group is null )
        {
            return new UpdateChatGroupDetailsResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails { Errors = { new ErrorDetail { Message = "Group was not found." } } }
            };
        }

        var isMember = await _members.Exists(group.Id, _identityContext.Id, context.CancellationToken);
        if ( !isMember )
        {
            return new UpdateChatGroupDetailsResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails { Errors = { new ErrorDetail { Message = "User is not a member." } } }
            };
        }

        if ( !group.HasAdmin(_identityContext.Id) )
        {
            return new UpdateChatGroupDetailsResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails { Errors = { new ErrorDetail { Message = "User is not an admin." } } }
            };
        }

        await _groups.UpdateAsync(group, g =>
        {
            g.Name = request.HasName ? request.Name : g.Name;
            g.About = request.HasAbout ? request.About : g.About;
            g.Picture = request.GroupPicture is not null
                ? new Domain.Entities.Media
                {
                    Id = Guid.Parse(request.GroupPicture.Id),
                    Type = request.GroupPicture.Type,
                    FileName = request.GroupPicture.FileName,
                    MediaUrl = request.GroupPicture.MediaUrl
                }
                : g.Picture;
            g.UpdatedAt = _clock.Now;
        }, context.CancellationToken);

        return new UpdateChatGroupDetailsResponse { Success = true };
    }

    public new async Task<LeaveChatGroupResponse> LeaveChatGroup(
        LeaveChatGroupRequest request,
        ServerCallContext context)
    {
        var group = await _groups.GetAsync(Guid.Parse(request.ChatGroupId), context.CancellationToken);
        if ( group is null )
        {
            return new LeaveChatGroupResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails { Errors = { new ErrorDetail { Message = "" } } }
            };
        }

        var member = await _members.ByGroupAndUser(Guid.Parse(request.ChatGroupId), _identityContext.Id,
            context.CancellationToken);
        if ( member is null )
        {
            return new LeaveChatGroupResponse
            {
                Success = false,
                ErrorDetails = new ErrorDetails { Errors = { new ErrorDetail { Message = "" } } }
            };
        }

        var success = await _members.DeleteAsync(member.Id, context.CancellationToken);
        return success ? new LeaveChatGroupResponse { Success = true } : new LeaveChatGroupResponse();
    }
}