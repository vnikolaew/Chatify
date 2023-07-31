using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chatify.Web.Middleware;

internal sealed class GlobalExceptionFilter : IAsyncExceptionFilter
{
    private readonly bool _isDevelopment;

    public GlobalExceptionFilter(IHostEnvironment environment)
        => _isDevelopment = environment.IsDevelopment();

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GlobalExceptionFilter>>();
        var errorLines = string.Join(Environment.NewLine,
            context.Exception.StackTrace!.Split(Environment.NewLine).Take(4));
        logger.LogError(errorLines);

        context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
        return HandleGenericException(context);
        
    }

    private Task HandleGenericException(ExceptionContext context)
    {
        var errorMessage = _isDevelopment ? context.Exception.Message : "An unexpected error occurred.";

        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Result = new BadRequestObjectResult(new { Error = errorMessage });

        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}