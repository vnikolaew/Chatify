using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chatify.Web.Middleware;

public class ActivitySourceActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var source = new ActivitySource("Chatify");
        using var activity = source.StartActivity();
        
        activity.SetTag("request.path", context.HttpContext.Request.Path);

        await next();
    }
}