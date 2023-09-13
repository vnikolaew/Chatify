using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Commands;

namespace Chatify.Web.Features.Profile.Models;

public static class Requests
{
    public record EditUserDetailsRequest(
        string? Username,
        [MinLength(3), MaxLength(50)] string? DisplayName,
        IFormFile? ProfilePicture,
        HashSet<string>? PhoneNumbers
    )
    {
        public EditUserDetails ToCommand()
            => new(Username, DisplayName, ProfilePicture is not null
                ? new InputFile
                {
                    FileName = ProfilePicture!.FileName,
                    Data = ProfilePicture.OpenReadStream()
                }
                : default, PhoneNumbers);
    }
}