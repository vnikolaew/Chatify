using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Chatify.Web.Middleware;

internal sealed class AuthorizationResultMiddlewareHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _handler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if ( authorizeResult.Challenged )
        {
            context.Response.StatusCode = ( int )HttpStatusCode.Unauthorized;
            var errorResponse = new ErrorResponse
            {
                StatusCode = ( int )HttpStatusCode.Unauthorized,
                Message = "Unauthorized. Access is denied."
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            await context.Response.CompleteAsync();
        }
        else if ( authorizeResult.Forbidden )
        {
            context.Response.StatusCode = ( int )HttpStatusCode.Forbidden;
            var errorResponse = new ErrorResponse
            {
                StatusCode = ( int )HttpStatusCode.Forbidden,
                Message = "Unauthorized. You don't have the required permissions."
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            await context.Response.CompleteAsync();
        }
        else
        {
            await _handler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
}