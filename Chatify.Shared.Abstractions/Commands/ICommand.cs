namespace Chatify.Shared.Abstractions.Commands;

public interface ICommand
{
}

public interface ICommand<TResult> : ICommand
{
}
