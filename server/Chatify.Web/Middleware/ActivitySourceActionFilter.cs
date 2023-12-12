using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chatify.Web.Middleware;

public class ActivitySourceActionFilter : IAsyncActionFilter
{
    private const string ActivitySourceName = "Chatify";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        using var activity = GetActivity();
        activity?.SetTag("request.path", context.HttpContext.Request.Path);

        await next();
    }

    private static Activity? GetActivity()
    {
        var source = new ActivitySource(ActivitySourceName);
        return source.StartActivity();
    }
}