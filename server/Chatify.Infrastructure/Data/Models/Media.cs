using AutoMapper;
using Cassandra;
using Chatify.Application.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Humanizer;
using Redis.OM.Modeling;

namespace Chatify.Infrastructure.Data.Models;

public record Media : IMapFrom<Domain.Entities.Media>
{
    [Indexed]
    public Guid Id { get; set; }

    [Indexed]
    public string MediaUrl { get; init; } = default!;

    [Indexed]
    public string? FileName { get; init; }

    [Indexed]
    public string? Type { get; init; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<Media, Domain.Entities.Media>()
            .ReverseMap();

    public static readonly UdtMap<Media> UdtMap = Cassandra.UdtMap
        .For<Media>(nameof(Media).Underscore())
        .UnderscoreColumn(m => m.MediaUrl)
        .UnderscoreColumn(m => m.Id)
        .UnderscoreColumn(m => m.FileName)
        .UnderscoreColumn(m => m.Type);
};