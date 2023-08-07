namespace Chatify.Shared.Abstractions.Time;

public interface IClock
{
    DateTime Now { get; }
}