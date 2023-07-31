using System.Reflection;

namespace Chatify.Application.Common.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
        => ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        const string mappingMethod = nameof(IMapFrom<object>.Mapping);
        const string mapFromInterface = "IMapFrom`1";

        var types = assembly
            .GetExportedTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType
                          && i.GetGenericTypeDefinition()
                          == typeof(IMapFrom<>)))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var method = type.GetMethod(mappingMethod)
                         ?? type.GetInterface(mapFromInterface)?
                             .GetMethod(mappingMethod);

            method!.Invoke(instance, new object[] { this });
        }
    }
}