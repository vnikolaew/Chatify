namespace Chatify.Infrastructure.Data.Seeding;

public interface ISeeder
{
    public int Priority { get; }
    
    Task SeedAsync(CancellationToken cancellationToken = default);
}