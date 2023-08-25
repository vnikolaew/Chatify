using AutoMapper;

namespace Chatify.Infrastructure.Data.Extensions;

public static class MappingProfileExtensions
{
    
    public static Profile CreateFor<TS, TD, TConverter>(this Profile profile)
        where TConverter : ITypeConverter<TS, TD>, ITypeConverter<TD, TS>, new()
    {
        profile
            .CreateMap<TS, TD>()
            .ConvertUsing(new TConverter());
        
        profile
            .CreateMap<TD, TS>()
            .ConvertUsing(new TConverter());
        
        return profile;
    }
}