using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Services.Shared.ChatGroups;
using Chatify.Services.Shared.Models;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ErrorDetail = Chatify.Services.Shared.ChatGroups.ErrorDetail;
using ErrorDetails = Chatify.Services.Shared.ChatGroups.ErrorDetails;
using Media = Chatify.Services.Shared.Models.Media;
using PinnedMessage = Chatify.Services.Shared.Models.PinnedMessage;

namespace Chatify.ChatGroupsService.Services;

[Authorize]
internal sealed class ChatGroupsServicer
    : Chatify.Services.Shared.ChatGroups.ChatGroupsServicer.ChatGroupsServicerBase
{
    private readonly IChatGroupRepository _groups;
    private readonly IChatGroupAttachmentRepository _attachments;
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public ChatGroupsServicer(IChatGroupRepository groups, IIdentityContext identityContext,
        IGuidGenerator guidGenerator, IClock clock, IChatGroupMemberRepository members,
        IChatGroupAttachmentRepository attachments)
    {
        _groups = groups;
        _identityContext = identityContext;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _members = members;
        _attachments = attachments;
    }

    public override async Task<AddChatGroupMembersResponse> AddChatGroupMembers(
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

    public override async Task<AddChatGroupAdminResponse> AddChatGroupAdmin(
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

    public override async Task<UpdateChatGroupDetailsResponse> UpdateChatGroupDetails(
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

    public override async Task<LeaveChatGroupResponse> LeaveChatGroup(
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

    public override async Task<RemoveChatGroupAdminResponse> RemoveChatGroupAdmin(
        RemoveChatGroupAdminRequest request,
        ServerCallContext context)
    {
        var chatGroup = await _groups.GetAsync(Guid.Parse(request.ChatGroupId), context.CancellationToken);
        if ( chatGroup is null ) return new RemoveChatGroupAdminResponse { Success = false };

        var isMember = await _members.Exists(chatGroup.Id, _identityContext.Id, context.CancellationToken);
        if ( !isMember ) return new RemoveChatGroupAdminResponse { Success = false };

        if ( chatGroup.CreatorId != _identityContext.Id )
            return new RemoveChatGroupAdminResponse { Success = false };

        if ( !chatGroup.HasAdmin(Guid.Parse(request.AdminId)) )
            return new RemoveChatGroupAdminResponse { Success = false };

        await _groups.UpdateAsync(chatGroup, group =>
        {
            group.RemoveAdmin(Guid.Parse(request.AdminId));
            group.UpdatedAt = _clock.Now;
        }, context.CancellationToken);
        return new RemoveChatGroupAdminResponse { Success = true };
    }

    public override async Task<RemoveChatGroupMemberResponse> RemoveChatGroupMember(
        RemoveChatGroupMemberRequest request, ServerCallContext context)
    {
        var chatGroup = await _groups.GetAsync(Guid.Parse(request.ChatGroupId), context.CancellationToken);
        if ( chatGroup is null ) return new RemoveChatGroupMemberResponse { Success = false };

        if ( chatGroup.AdminIds.All(id => id != _identityContext.Id) )
        {
            return new RemoveChatGroupMemberResponse { Success = false };
        }

        var memberExists = await _members.Exists(
            Guid.Parse(request.ChatGroupId), Guid.Parse(request.MemberId),
            context.CancellationToken);
        if ( !memberExists ) return new RemoveChatGroupMemberResponse { Success = false };

        await _members.DeleteAsync(_identityContext.Id, context.CancellationToken);
        return new RemoveChatGroupMemberResponse { Success = true };
    }

    public override async Task<GetChatGroupDetailsResponse> GetChatGroupDetails(GetChatGroupDetailsRequest request,
        ServerCallContext context)
    {
        var group = await _groups.GetAsync(Guid.Parse(request.ChatGroupId), context.CancellationToken);
        if ( group is null ) return new GetChatGroupDetailsResponse { Success = false };

        var isMember = await _members.Exists(Guid.Parse(request.ChatGroupId), _identityContext.Id,
            context.CancellationToken);
        if ( !isMember ) return new GetChatGroupDetailsResponse { Success = false };

        return new GetChatGroupDetailsResponse
        {
            Success = true, ChatGroupDetails = new ChatGroupDetailsModel
            {
                ChatGroup = new ChatGroupModel
                {
                    Id = group.Id.ToString(),
                    About = group.About,
                    Name = group.Name,
                    CreatedAt = Timestamp.FromDateTimeOffset(group.CreatedAt),
                    AdminIds = { group.AdminIds.Select(id => id.ToString()) },
                    CreatorId = group.CreatorId.ToString(),
                    Metadata = { group.Metadata },
                    ProfilePicture = group.Picture is { }
                        ? new Media
                        {
                            Id = group.Picture.Id.ToString(),
                            FileName = group.Picture.FileName,
                            MediaUrl = group.Picture.MediaUrl,
                            Type = group.Picture.Type
                        }
                        : default,
                    UpdatedAt =
                        group.UpdatedAt.HasValue ? Timestamp.FromDateTimeOffset(group.UpdatedAt.Value) : default,
                    PinnedMessages =
                    {
                        group.PinnedMessages.Select(m => new PinnedMessage
                        {
                            Id = m.MessageId.ToString(), CreatedAt = Timestamp.FromDateTimeOffset(m.CreatedAt),
                            PinnerId = m.PinnerId.ToString()
                        })
                    }
                }
            }
        };
    }

    public override async Task<GetChatGroupMembershipDetailsResponse> GetChatGroupMembershipDetails(
        GetChatGroupMembershipDetailsRequest request, ServerCallContext context)
    {
        var isMember = await _members.Exists(Guid.Parse(request.ChatGroupId), _identityContext.Id,
            context.CancellationToken);
        if ( !isMember ) return new GetChatGroupMembershipDetailsResponse { Success = false };

        var membership = await _members
            .ByGroupAndUser(Guid.Parse(request.ChatGroupId), Guid.Parse(request.UserId), context.CancellationToken);

        return membership is not null
            ? new GetChatGroupMembershipDetailsResponse
            {
                Success = true,
                ChatGroupMembership = new ChatGroupMembershipModel
                {
                    Member = new ChatGroupMemberModel
                    {
                        Id = membership.Id.ToString(),
                        UserId = membership.UserId.ToString(),
                        CreatedAt = Timestamp.FromDateTimeOffset(membership.CreatedAt),
                        MembershipType = ( MembershipType )membership.MembershipType + 1,
                        ChatGroupId = membership.ChatGroupId.ToString(),
                        Username = membership.Username
                    }
                }
            }
            : new GetChatGroupMembershipDetailsResponse { Success = false };
    }

    public override async Task<GetChatGroupMemberIdsResponse> GetChatGroupMemberIds(
        GetChatGroupMemberIdsRequest request,
        ServerCallContext context)
    {
        var group = await _groups.GetAsync(
            Guid.Parse(request.ChatGroupId),
            context.CancellationToken);
        if ( group is null ) return new GetChatGroupMemberIdsResponse { Success = false };

        var isMember = await _members.Exists(
            group.Id, group.CreatorId, context.CancellationToken);
        if ( !isMember ) return new GetChatGroupMemberIdsResponse { Success = false };

        var memberIds = await _members
            .UserIdsByGroup(group.Id, context.CancellationToken);

        return memberIds is { }
            ? new GetChatGroupMemberIdsResponse
                { Success = true, MemberIds = { memberIds.Select(id => id.ToString()) } }
            : new GetChatGroupMemberIdsResponse { Success = false };
    }

    public override async Task<GetChatGroupSharedAttachmentsResponse> GetChatGroupSharedAttachments(
        GetChatGroupSharedAttachmentsRequest request, ServerCallContext context)
    {
        var isGroupMember = await _members.Exists(
            Guid.Parse(request.ChatGroupId),
            _identityContext.Id, context.CancellationToken);
        if ( !isGroupMember ) return new GetChatGroupSharedAttachmentsResponse { Success = false };

        var attachments = await _attachments.GetPaginatedAttachmentsByGroupAsync(
            Guid.Parse(request.ChatGroupId),
            request.PageSize,
            request.PagingCursor, context.CancellationToken);

        return new GetChatGroupSharedAttachmentsResponse
        {
            Success = true,
            Attachments = new ChatGroupAttachmentsModel
            {
                PageSize = attachments.PageSize,
                PagingCursor = attachments.PagingCursor,
                Total = attachments.Total,
                HasMore = attachments.HasMore,
                Items =
                {
                    attachments.Select(a => new ChatGroupAttachmentModel
                    {
                        AttachmentId = a.AttachmentId.ToString(),
                        ChatGroupId = a.ChatGroupId.ToString(),
                        CreatedAt = Timestamp.FromDateTime(a.CreatedAt),
                        Username = a.Username,
                        UserId = a.UserId.ToString(),
                        Media = new Media(),
                        UpdatedAt = a.UpdatedAt.HasValue ? Timestamp.FromDateTime(a.UpdatedAt.Value) : default
                    })
                }
            }
        };
    }
}