namespace Chatify.Shared.Abstractions;

public record AppInfo(string Name, Version Version)
{
    public override string ToString() => $"{Name} {Version}";
}