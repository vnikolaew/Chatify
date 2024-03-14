using Chatify.Application.ChatGroups.Contracts;
using LanguageExt;
using LanguageExt.Common;
using OneOf;
using AddChatGroupAdminRequest = Chatify.Application.ChatGroups.Contracts.AddChatGroupAdminRequest;
using AddChatGroupMemberResponse = Chatify.Services.Shared.ChatGroups.AddChatGroupMemberResponse;
using AddChatGroupMembersRequest = Chatify.Application.ChatGroups.Contracts.AddChatGroupMembersRequest;
using ChatGroupsServicer = Chatify.Services.Shared.ChatGroups.ChatGroupsServicer;
using CreateChatGroupRequest = Chatify.Application.ChatGroups.Contracts.CreateChatGroupRequest;
using Media = Chatify.Services.Shared.ChatGroups.Media;
using MembershipType = Chatify.Services.Shared.ChatGroups.MembershipType;
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
                req.AddChatGroupMembers.Select(m => new Chatify.Services.Shared.ChatGroups.AddChatGroupMember
                {
                    Username = m.Username,
                    UserId = m.UserId.ToString(),
                    MembershipType = ( MembershipType )( m.MembershipType + 1 )
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
}