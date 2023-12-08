using System.Linq.Expressions;
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
        using var client = new HttpClient()
        {
            BaseAddress = new Uri("https://images.chesscomfiles.com")
            // 
        };

        char[] colors = { 'w', 'b' };
        char[] pieces = { 'n', 'q', 'k', 'b', 'r' };

        var coloredPieces = colors.SelectMany(c => pieces.Select(p => $"{c}{p}"));
        if ( !Directory.Exists("pieces") ) Directory.CreateDirectory("pieces");
        
        foreach ( var coloredPiece in coloredPieces )
        {
            var imageBytes = await client.GetByteArrayAsync($"/chess-themes/pieces/classic/150/{coloredPiece}.png");
            await File.WriteAllBytesAsync($"pieces/{coloredPiece}.png", imageBytes);
        }
    }
}