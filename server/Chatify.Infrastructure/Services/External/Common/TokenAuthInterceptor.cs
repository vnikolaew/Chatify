using Chatify.Shared.Abstractions.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;

namespace Chatify.Infrastructure.Services.External.Common;

public sealed class TokenAuthInterceptor(
    IJwtTokenGenerator jwtTokenGenerator,
    IHttpContextAccessor httpContextAccessor)
    : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = jwtTokenGenerator.Generate(httpContextAccessor.HttpContext!.User);

        var headers = new Metadata { new("Authorization", $"Bearer {token}") };
        var newOptions = context.Options.WithHeaders(headers);

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            newOptions);

        return base.AsyncUnaryCall(request, newContext, continuation);
    }
}