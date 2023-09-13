namespace Chatify.GraphQL;

public sealed class ChatifyErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        return error.WithMessage(error.Exception?.Message ?? string.Empty);
    }
}