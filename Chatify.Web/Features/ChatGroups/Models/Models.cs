using Chatify.Application.ChatGroups.Commands;
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
}