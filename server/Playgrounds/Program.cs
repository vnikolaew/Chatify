﻿using System.Linq.Expressions;
using System.Reflection;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Mappings;
using Chatify.Infrastructure.Data.Models;
using Playgrounds;

internal class Program
{
    private static string TranslateSelectToCql<T>(
        Expression<Func<T, object>> selection)
    {
        if ( selection.Body is not NewExpression newExpression ) return string.Empty;
        var tableConfig = MappingConfiguration.Global.Get<T>();

        var columnNames = newExpression
            .Arguments
            .OfType<MemberExpression>()
            .Select(e => tableConfig
                .GetColumnDefinition(e.Member as PropertyInfo)
                ?.ColumnName)
            .Where(_ => _ is not null)
            .ToList();

        return $"SELECT {string.Join(", ", columnNames)} FROM {tableConfig.TableName} ALLOW FILTERING;";
    }

    public static async Task Main(string[] args)
    {
        await TelegramPlayground.Run();
    }
}