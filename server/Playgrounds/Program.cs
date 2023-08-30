using System.Text.Json;
using Chatify.Domain.Entities;
using UserNotification = Chatify.Infrastructure.Data.Models.UserNotification;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var media = JsonSerializer.Serialize(new Media()
        {
            Id = Guid.NewGuid(),
            MediaUrl = "dfsfdsfdjA;"
        });
        var notification = new UserNotification
        {
            Type = ( sbyte )UserNotificationType.IncomingFriendInvite,
            Summary = "sdewwefsdf",
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.Now,
            Metadata = new Dictionary<string, string>
            {
                { "user_media", media },
                { "invite_id", Guid.NewGuid().ToString() },
            },
        };

        // var profile = new MappingProfile(typeof(UserNotification).Assembly);

        // var mapper = new MapperConfiguration(cfg =>
        //         cfg.AddProfile(profile))
        //     .CreateMapper();
        // var x = mapper.Map<Chatify.Domain.Entities.UserNotification>(notification);
    }
}