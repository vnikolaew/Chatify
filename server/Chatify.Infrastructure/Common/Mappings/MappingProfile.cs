﻿using System.Reflection;
using AutoMapper;
using Chatify.Application.Common.Mappings;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
        => ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());

    public MappingProfile(Assembly assembly)
        => ApplyMappingsFromAssembly(assembly);

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

        foreach ( var type in types )
        {
            var instance = Activator.CreateInstance(type);

            var methods = type.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic);
            if ( ExtendsMultipleInterfaces(type) )
            {
                var mappingMethods = methods
                    .Where(m => m.Name.Contains(mappingMethod));

                foreach ( var methodInfo in mappingMethods )
                {
                    methodInfo.Invoke(instance, new object?[] { this });
                }
                
                continue;
            }
            
            var method = type.GetMethod(mappingMethod)
                         ?? type.GetInterface(mapFromInterface)?
                             .GetMethod(mappingMethod);

            method!.Invoke(instance, new object[] { this });
        }
    }

    private static bool ExtendsMultipleInterfaces(Type type)
        => type
            .GetInterfaces()
            .Count(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)) > 1;
}