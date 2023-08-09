using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.Common.Models;

namespace Chatify.Web.Features.ChatGroups.Models;

public static class Models
{
    public record CreateChatGroupRequest(
        string? About,
        string Name,
        IFormFile? File)
    {
        public CreateChatGroup ToCommand()
            => new(About, Name, File is not null
                ? new InputFile { Data = File.OpenReadStream(), FileName = File.FileName }
                : default);
    }
    
    public record GetChatGroupSharedAttachmentsRequest(
        Guid GroupId,
        int PageSize,
        string PagingCursor)
    {
        public GetChatGroupSharedAttachments ToCommand()
            => new(GroupId, PageSize, PagingCursor);
    }

    public record EditChatGroupDetailsRequest(
        Guid ChatGroupId,
        string? Name,
        string? About,
        IFormFile? File
    )
    {
        public EditChatGroupDetails ToCommand()
            => new(ChatGroupId, Name, About, File is not null
                ? new InputFile
                {
                    FileName = File.FileName,
                    Data = File.OpenReadStream()
                }
                : default);
    }
}