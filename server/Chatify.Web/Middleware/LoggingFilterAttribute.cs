using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chatify.Web.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LoggingFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var loggerType = typeof(ILogger<>).MakeGenericType(context.Controller.GetType());
        
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        var logger = context.HttpContext.RequestServices.GetRequiredService(loggerType) as ILogger;
        logger!.LogInformation("Executing method `{MethodName}` of controller {ControllerName}. ",
            actionDescriptor!.ActionName,
            context.Controller.GetType().Name);
        
        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var loggerType = typeof(ILogger<>).MakeGenericType(context.Controller.GetType());
        
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        var logger = context.HttpContext.RequestServices.GetRequiredService(loggerType) as ILogger;
        logger!.LogInformation("Executed method `{MethodName}` of controller {ControllerName}. ",
            actionDescriptor!.ActionName,
            context.Controller.GetType().Name);
        
        base.OnActionExecuted(context);
    }
}