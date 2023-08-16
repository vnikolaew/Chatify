using AutoMapper;
using Cassandra;
using Chatify.Application.Common.Mappings;
using Chatify.Infrastructure.Common.Mappings;
using Humanizer;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public record Media  : IMapFrom<Domain.Entities.Media>
{
    public Guid Id { get; set; }
    
    public string MediaUrl { get; init; } = default!;
    
    public string? FileName { get; init; }
    
    public string? Type { get; init; }

    public void Mapping(Profile profile)
        => profile
            .CreateMap<Media, Domain.Entities.Media>()
            .ReverseMap();

    public static readonly UdtMap<Media> UdtMap = Cassandra.UdtMap
        .For<Media>(nameof(Media).Underscore())
        .Map(m => m.MediaUrl, nameof(MediaUrl).Underscore())
        .Map(m => m.Id, nameof(Id).Underscore())
        .Map(m => m.FileName, nameof(FileName).Underscore())
        .Map(m => m.Type, nameof(Type).Underscore());
};