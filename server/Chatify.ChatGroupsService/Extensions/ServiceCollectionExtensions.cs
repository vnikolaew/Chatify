using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Infrastructure;
using Chatify.Services.Shared.ChatGroups;
using Chatify.Services.Shared.Models;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Media = Chatify.Domain.Entities.Media;
using PinnedMessage = Chatify.Domain.Entities.PinnedMessage;

namespace Chatify.ChatGroupsService.Extensions;

public static class ServiceCollectionExtensions
{
    private sealed class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ChatGroup, ChatGroupModel>()
                .ForMember(x => x.About,
                    x => x.NullSubstitute(string.Empty));

            CreateMap<ChatGroupModel, ChatGroup>();

            CreateMap<string?, string>()
                .ConvertUsing(x => x ?? string.Empty);

            CreateMap<ChatGroupMember, ChatGroupMemberModel>()
                .ReverseMap();

            CreateMap<ChatGroupAttachment, ChatGroupAttachmentModel>()
                .ReverseMap();
        }
    }

    public static IServiceCollection AddMappings(this IServiceCollection services)
        => services.AddMappers(config =>
        {
            config.AddProfile<MappingProfile>();

            config.CreateMap<string, Guid>()
                .ConvertUsing(s => Guid.Parse(s));

            config.CreateMap<MembershipType, sbyte>()
                .ConvertUsing(t => ( sbyte )( t - 1 ));

            config.CreateMap<sbyte, MembershipType>()
                .ConvertUsing(t => ( MembershipType )( t + 1 ));

            config.CreateMap<Guid, string>()
                .ConvertUsing(guid => guid.ToString());

            config.CreateMap<Timestamp, DateTime>()
                .ConvertUsing(t => t.ToDateTime());

            config.CreateMap<DateTime, Timestamp>()
                .ConvertUsing(dt => Timestamp.FromDateTime(dt));

            config.CreateMap<Timestamp, DateTimeOffset>()
                .ConvertUsing(t => t.ToDateTimeOffset());

            config.CreateMap<DateTimeOffset, Timestamp>()
                .ConvertUsing(dt => Timestamp.FromDateTimeOffset(dt));

            config.CreateMap<ISet<Guid>, RepeatedField<string>>()
                .ConvertUsing(guids => new RepeatedField<string> { guids.Select(id => id.ToString()) });

            config.CreateMap<List<Guid>, RepeatedField<string>>()
                .ConvertUsing(guids => new RepeatedField<string> { guids.Select(id => id.ToString()) });

            config.CreateMap<IDictionary<string, string>, MapField<string, string>>()
                .ConvertUsing(map => new MapField<string, string> { map });

            config.CreateMap<Media, Chatify.Services.Shared.Models.Media>()
                .ConvertUsing(m => new Chatify.Services.Shared.Models.Media
                {
                    Id = m.Id.ToString(),
                    FileName = m.FileName,
                    MediaUrl = m.MediaUrl,
                    Type = m.Type
                });

            config.CreateMap<DateTime, DateTimeOffset>()
                .ConvertUsing(dt => new DateTimeOffset(dt));

            config.CreateMap<DateTimeOffset, DateTime>()
                .ConvertUsing(dto => dto.DateTime);

            config.CreateMap<PinnedMessage, Chatify.Services.Shared.Models.PinnedMessage>()
                .ConvertUsing(pm => new Chatify.Services.Shared.Models.PinnedMessage
                {
                    Id = pm.MessageId.ToString(), CreatedAt = Timestamp.FromDateTimeOffset(pm.CreatedAt),
                    PinnerId = pm.PinnerId.ToString()
                });
        });
}