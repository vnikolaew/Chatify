using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Services.External.Common;

public sealed class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.StartReceiveCall(MethodType.Unary, context.Method);
        try
        {
            return await continuation(request, context);
        }
        catch ( Exception ex )
        {
            _logger.OnError(context.Method, ex.Message);
            throw;
        }
    }
}

public static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Starting receiving an unary call. Type/Method: {Type} / {Method}")]
    public static partial void StartReceiveCall(
        this ILogger logger,
        MethodType type,
        string method);

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Error,
        Message = "Error thrown by {Method} - {Exception}")]
    public static partial void OnError(
        this ILogger logger,
        string method, string exception);
}