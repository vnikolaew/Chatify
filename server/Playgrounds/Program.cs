using System.Text;
using Cassandra;
using Cassandra.Mapping;
using Castle.DynamicProxy;
using Playgrounds;

public class KeyValuePair : Mappings
{
    public string Key { get; set; }

    public string Value { get; set; }

    public KeyValuePair()
        => For<KeyValuePair>()
            .PartitionKey(_ => _.Key)
            .Column(_ => _.Value, cm => cm.WithName("value"))
            .Column(_ => _.Key, cm => cm.WithName("key"));
}

internal class Program
{
    public static async Task Main(string[] args)
    {
        var person = new Person().Test();
        person.PropertyChanged += (sender, args) =>
        {
            var value = sender!
                .GetType()
                .GetProperty(args.PropertyName!)!
                .GetValue(sender);
            Console.WriteLine($"{args.PropertyName} => {value}");
        };
        person.FirstName = "sdfsdf";
        
        return;

        var cluster = Cluster.Builder()
            .AddContactPoint("localhost")
            .WithPort(9043)
            .WithCompression(CompressionType.LZ4)
            .WithDefaultKeyspace("chatify")
            .Build();

        var session = await cluster.ConnectAsync();

        MappingConfiguration.Global.Define<KeyValuePair>();
        var mapper = new Mapper(session);

        var kvs = await mapper.FetchPageAsync<KeyValuePair>(
            10, null!, "SELECT * FROM kv_pairs;", null!);
        while ( kvs.PagingState != null && kvs.Count > 0 )
        {
            kvs = await mapper.FetchPageAsync<KeyValuePair>(
                10, kvs.PagingState!, "SELECT * FROM kv_pairs;", null!);
            Console.WriteLine(
                $"Paging state: {Encoding.UTF8.GetString(kvs.PagingState)}. Length in bytes: {kvs.PagingState.Length}");

            Console.WriteLine($"Fetched next {kvs.Count} records.");
        }
    }
}