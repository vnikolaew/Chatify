using Chatify.Application.ChatGroups.Contracts;
using Chatify.Infrastructure.Services.External.ChatGroupsService;
using LanguageExt.Common;
using OneOf;
using AddChatGroupMembersRequest = Chatify.Application.ChatGroups.Contracts.AddChatGroupMembersRequest;
using CreateChatGroupRequest = Chatify.Application.ChatGroups.Contracts.CreateChatGroupRequest;

namespace Chatify.Infrastructure.Services.External.ChatGroups;

public sealed class ChatGroupsService(ChatGroupsServicer.ChatGroupsServicerClient client) : IChatGroupsService
{
    public async Task<OneOf<Error, Guid>> CreateChatGroupAsync(
        CreateChatGroupRequest req,
        CancellationToken cancellationToken)
    {
        var request = new External.ChatGroupsService.CreateChatGroupRequest
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
        var request = new External.ChatGroupsService.AddChatGroupMembersRequest
        {
            ChatGroupId = req.ChatGroupId.ToString(),
            Members =
            {
                req.AddChatGroupMembers.Select(m => new AddChatGroupMember()
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
}