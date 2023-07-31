using Chatify.Shared.Abstractions.Time;

namespace Chatify.Shared.Infrastructure.Time;

public class UtcClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}