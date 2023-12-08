using Chatify.Shared.Abstractions.Time;
using SystemClock = Polly.Utilities.SystemClock;

namespace Chatify.Shared.Infrastructure.Time;

public class UtcClock : IClock
{
    public DateTime Now => SystemClock.UtcNow();
}