using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Domain.Entities;
using Chatify.Services.Shared.ChatGroups;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using LanguageExt.Common;
using OneOf;
using AddChatGroupAdminRequest = Chatify.Application.ChatGroups.Contracts.AddChatGroupAdminRequest;
using AddChatGroupMemberResponse = Chatify.Services.Shared.ChatGroups.AddChatGroupMemberResponse;
using AddChatGroupMembersRequest = Chatify.Application.ChatGroups.Contracts.AddChatGroupMembersRequest;
using ChatGroupsServicer = Chatify.Services.Shared.ChatGroups.ChatGroupsServicer;
using CreateChatGroupRequest = Chatify.Application.ChatGroups.Contracts.CreateChatGroupRequest;
using GetChatGroupMembershipDetailsRequest =
    Chatify.Application.ChatGroups.Contracts.GetChatGroupMembershipDetailsRequest;
using LeaveChatGroupRequest = Chatify.Application.ChatGroups.Contracts.LeaveChatGroupRequest;
using Media = Chatify.Services.Shared.ChatGroups.Media;
using Models_MembershipType = Chatify.Services.Shared.Models.MembershipType;
using RemoveChatGroupAdminRequest = Chatify.Application.ChatGroups.Contracts.RemoveChatGroupAdminRequest;
using RemoveChatGroupMemberRequest = Chatify.Application.ChatGroups.Contracts.RemoveChatGroupMemberRequest;
using UpdateChatGroupDetailsRequest = Chatify.Application.ChatGroups.Contracts.UpdateChatGroupDetailsRequest;

namespace Chatify.Infrastructure.Services.External.ChatGroups;

public sealed class ChatGroupsService(ChatGroupsServicer.ChatGroupsServicerClient client) : IChatGroupsService
{
    public async Task<OneOf<Error, Guid>> CreateChatGroupAsync(
        CreateChatGroupRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.CreateChatGroupRequest
        {
            Name = req.Name,
            About = req.About,
            GroupPicture = req.Media is not null
                ? new Media
                {
                    Id = req.Media.Id.ToString(),
                    Type = req.Media.Type,
                    FileName = req.Media.FileName,
                    MediaUrl = req.Media.MediaUrl
                }
                : default,
        };
        request.MemberIds.AddRange(req.MemberIds?.Select(_ => _.ToString()));

        var response = await client.CreateChatGroupAsync(request, cancellationToken: cancellationToken);
        return response.Success && Guid.TryParse(response.ChatGroupId, out var id)
            ? id
            : Error.New(
                string.Join(", ", response.ErrorDetails.Errors.Select(e => e.Message)));
    }

    public async Task<List<OneOf<Error, Guid>>> AddChatGroupMembersAsync(AddChatGroupMembersRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.AddChatGroupMembersRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            Members =
            {
                req.AddChatGroupMembers.Select(m => new AddChatGroupMember
                {
                    Username = m.Username,
                    UserId = m.UserId.ToString(),
                    MembershipType = ( Models_MembershipType )( m.MembershipType + 1 )
                })
            }
        };

        var response = await client.AddChatGroupMembersAsync(request, cancellationToken: cancellationToken);
        return response.Results
            .Select<AddChatGroupMemberResponse, OneOf<Error, Guid>>(res =>
                res.Success ? res.MemberId : Error.New(res.ErrorDetails.Errors[0].Message))
            .ToList();
    }

    public async Task<OneOf<Error, Unit>> AddChatGroupAdminAsync(
        AddChatGroupAdminRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.AddChatGroupAdminRequest
        {
            AdminId = req.AdminId.ToString(),
            ChatGroupId = req.ChatGroupId.ToString()
        };
        var response = await client.AddChatGroupAdminAsync(request, cancellationToken: cancellationToken);

        return response.Success
            ? Unit.Default
            : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, Unit>> UpdateChatGroupDetailsAsync(UpdateChatGroupDetailsRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.UpdateChatGroupDetailsRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            GroupPicture = req.Picture is not null
                ? new Media
                {
                    Id = req.Picture.Id.ToString(),
                    Type = req.Picture.Type,
                    FileName = req.Picture.FileName,
                    MediaUrl = req.Picture.MediaUrl
                }
                : default,
            About = req.About,
            Name = req.Name
        };
        var response = await client.UpdateChatGroupDetailsAsync(request, cancellationToken: cancellationToken);
        return response.Success
            ? Unit.Default
            : Error.Many(response.ErrorDetails.Errors.Select(e => Error.New(e.Message)).ToArray());
    }

    public async Task<OneOf<Error, Unit>> LeaveChatGroupAsync(LeaveChatGroupRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.LeaveChatGroupRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            Reason = req.Reason
        };

        var response = await client.LeaveChatGroupAsync(request, cancellationToken: cancellationToken);
        return response.Success ? Unit.Default : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, Unit>> RemoveChatGroupAdminAsync(RemoveChatGroupAdminRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.RemoveChatGroupAdminRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            AdminId = req.AdminId.ToString()
        };
        var response = await client.RemoveChatGroupAdminAsync(request, cancellationToken: cancellationToken);
        return response.Success ? Unit.Default : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, Unit>> RemoveChatGroupMemberAsync(RemoveChatGroupMemberRequest req,
        CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.RemoveChatGroupMemberRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            MemberId = req.MemberId.ToString()
        };
        var response = await client.RemoveChatGroupMemberAsync(request, cancellationToken: cancellationToken);
        return response.Success ? Unit.Default : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, ChatGroup>> GetChatGroupDetails(Guid chatGroupId,
        CancellationToken cancellationToken)
    {
        var response = await client.GetChatGroupDetailsAsync(
            new GetChatGroupDetailsRequest { ChatGroupId = chatGroupId.ToString() },
            cancellationToken: cancellationToken);

        var responseChatGroup = response.ChatGroupDetails.ChatGroup;
        return response.Success
            ? new ChatGroup
            {
                Id = Guid.Parse(responseChatGroup.Id),
                Name = responseChatGroup.Name,
                About = responseChatGroup.About,
                AdminIds = responseChatGroup.AdminIds.Select(Guid.Parse).ToHashSet(),
                CreatedAt = responseChatGroup.CreatedAt.ToDateTimeOffset(),
                UpdatedAt = responseChatGroup.UpdatedAt.ToDateTimeOffset(),
                Picture = new Domain.Entities.Media
                {
                    Id = Guid.Parse(responseChatGroup.ProfilePicture.Id),
                    FileName = responseChatGroup.ProfilePicture.FileName,
                    MediaUrl = responseChatGroup.ProfilePicture.MediaUrl,
                    Type = responseChatGroup.ProfilePicture.Type
                },
                CreatorId = Guid.Parse(responseChatGroup.CreatorId),
                Metadata = responseChatGroup.Metadata.ToDictionary(),
                PinnedMessages = responseChatGroup.PinnedMessages.Select(m =>
                        new PinnedMessage(Guid.Parse(m.Id), m.CreatedAt.ToDateTime(), Guid.Parse(m.PinnerId)))
                    .ToHashSet()
            }
            : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, ChatGroupMember>> GetChatGroupMembershipDetailsAsync(
        GetChatGroupMembershipDetailsRequest req, CancellationToken cancellationToken)
    {
        var request = new Chatify.Services.Shared.ChatGroups.GetChatGroupMembershipDetailsRequest()
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            UserId = req.UserId.ToString()
        };
        var response = await client.GetChatGroupMembershipDetailsAsync(request, cancellationToken: cancellationToken);

        var member = response.ChatGroupMembership.Member;
        return response.Success
            ? new ChatGroupMember
            {
                Id = Guid.Parse(member.Id),
                Username = member.Username,
                ChatGroupId = Guid.Parse(member.ChatGroupId),
                UserId = Guid.Parse(member.UserId),
                CreatedAt = member.CreatedAt.ToDateTimeOffset(),
                MembershipType = ( sbyte )( member.MembershipType - 1 )
            }
            : Error.New(response.ErrorDetails.Errors[0].Message);
    }

    public async Task<OneOf<Error, List<Guid>>> GetChatGroupMemberIdsAsync(Guid chatGroupId,
        CancellationToken cancellationToken)
    {
        var response = await client.GetChatGroupMemberIdsAsync(
            new GetChatGroupMemberIdsRequest { ChatGroupId = chatGroupId.ToString() },
            cancellationToken: cancellationToken);

        return response.Success
            ? Error.New(response.ErrorDetails.Errors[0].Message)
            : response.MemberIds.Select(Guid.Parse).ToList();
    }

    public async Task<CursorPaged<ChatGroupAttachment>> GetChatGroupSharedAttachments(
        GetChatGroupSharedAttachments request, CancellationToken cancellationToken)
    {
        var response = await client.GetChatGroupSharedAttachmentsAsync(new GetChatGroupSharedAttachmentsRequest()
        {
            ChatGroupId = request.GroupId.ToString(),
            PagingCursor = request.PagingCursor,
            PageSize = request.PageSize
        }, cancellationToken: cancellationToken);

        return response.Success
            ? response.Attachments.Items!.Select(a => new ChatGroupAttachment()
                {
                    AttachmentId = Guid.Parse(a.AttachmentId),
                    ChatGroupId = Guid.Parse(a.ChatGroupId),
                    CreatedAt = a.CreatedAt.ToDateTime(),
                    Username = a.Username,
                    UserId = Guid.Parse(a.UserId),
                    MediaInfo = new Domain.Entities.Media()
                    {
                        Id = Guid.Parse(a.Media.Id),
                        FileName = a.Media.FileName,
                        MediaUrl = a.Media.MediaUrl,
                        Type = a.Media.Type
                    },
                    UpdatedAt = a.UpdatedAt.ToDateTime()
                })
                .ToCursorPaged(response.Attachments.PagingCursor, response.Attachments.HasMore,
                    response.Attachments.Total)
            : default!;
    }
}