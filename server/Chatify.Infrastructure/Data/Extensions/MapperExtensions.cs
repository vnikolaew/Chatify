using System.Linq.Expressions;
using System.Reflection;
using Cassandra.Mapping;

namespace Chatify.Infrastructure.Data.Extensions;

public static class MapperExtensions
{
    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper)
        => ( await mapper.FetchAsync<T>() ).ToList();

    public static async Task<List<T>> FetchListAsync<T>(
        this IMapper mapper,
        Expression<Func<T, object>> selection)
    {
        var selectedColumns = GetSelectedColumns(selection);
        var selectAllColumns = new List<string> { "*" };
        
        var tableName = MappingConfiguration.Global.Get<T>().TableName;
        var idColumn = MappingConfiguration.Global.Get<T>().PartitionKeys[0];

        var cql =
            $"SELECT {string.Join(", ", selectedColumns ?? selectAllColumns)} FROM {tableName} WHERE {idColumn} = ? ALLOW FILTERING;";
        return ( await mapper.FetchAsync<T>(cql) ).ToList();
    }

    private static List<string>? GetSelectedColumns<T>(
        Expression<Func<T, object>> selection)
    {
        if ( selection.Body is not NewExpression newExpression ) return default;
        var tableConfig = MappingConfiguration.Global.Get<T>();

        var columnNames = newExpression
            .Arguments
            .OfType<MemberExpression>()
            .Select(e => tableConfig
                .GetColumnDefinition(e.Member as PropertyInfo)
                ?.ColumnName)
            .Where(_ => _ is not null)
            .ToList();

        return columnNames;
    }

    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper,
        string cql,
        params object[] args)
        => ( await mapper.FetchAsync<T>(cql, args) ).ToList();

    public static async Task<List<T>> FetchListAsync<T>(this IMapper mapper,
        Cql cql)
        => ( await mapper.FetchAsync<T>(cql) ).ToList();

    public static async Task<bool> AnyAsync<T>(this IMapper mapper)
    {
        var config = MappingConfiguration.Global.Get<T>();
        var tableName = config.TableName!;

        return await mapper.FirstOrDefaultAsync<long>(
            $"SELECT COUNT(*) FROM {tableName};") > 0;
    }

    public static Cql WithArguments(this Cql cql,
        object?[] arguments)
    {
        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, arguments);

        return cql;
    }
}