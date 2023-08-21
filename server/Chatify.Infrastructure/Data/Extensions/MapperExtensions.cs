using Cassandra.Mapping;

namespace Chatify.Infrastructure.Data.Extensions;

public static class MapperExtensions
{
    
    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper)
        => ( await mapper.FetchAsync<T>() ).ToList();
    
    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper, string cql, params object[] args)
        => ( await mapper.FetchAsync<T>(cql, args) ).ToList();

    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper, Cql cql)
        => ( await mapper.FetchAsync<T>(cql) ).ToList();
}