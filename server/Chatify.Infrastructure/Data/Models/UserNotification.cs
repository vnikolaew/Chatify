using AutoMapper;
using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Data.Models;

using Metadata = Dictionary<string, string>;

public class UserNotification : IMapFrom<Domain.Entities.UserNotification>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public sbyte Type { get; set; }

    public Metadata Metadata { get; set; } = new();

    public string? Summary { get; set; }

    public bool Read { get; set; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<UserNotification, Domain.Entities.UserNotification>()
            .ReverseMap();
}