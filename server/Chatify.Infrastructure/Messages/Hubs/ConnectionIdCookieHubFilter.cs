using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Exception = System.Exception;

namespace Chatify.Infrastructure.Messages.Hubs;

public sealed class ConnectionIdCookieHubFilter : IHubFilter
{
    private void AttachConnectionIdCookie(HubLifetimeContext context)
        => context.Context.GetHttpContext()!.Response.Cookies.Append("Connection-Id", context.Context.ConnectionId,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = false,
                IsEssential = false,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(30)
            });

    private void DeleteConnectionIdCookie(HubLifetimeContext context)
        => context.Context.GetHttpContext()!.Response.Cookies.Delete("Connection-Id");

    public async Task OnConnectedAsync(
        HubLifetimeContext context,
        Func<HubLifetimeContext, Task> next)
    {
        // AttachConnectionIdCookie(context);
        await next(context);
    }

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        // DeleteConnectionIdCookie(context);
        await next(context, exception);
    }
}