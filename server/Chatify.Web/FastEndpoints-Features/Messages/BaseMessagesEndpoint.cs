using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

namespace Chatify.Web.FastEndpoints_Features.Messages;

public sealed class BaseMessagesGroup : SubGroup<BaseGroup>
{
    public BaseMessagesGroup() =>
        Configure("messages",
            ep => ep.Description(x => x.WithTags("messages")));
}

public abstract class BaseMessagesEndpoint<TRequest, TResponse>
    : BaseEndpoint<TRequest, TResponse> where TRequest : notnull
{
    protected BaseMessagesEndpoint()
        => Group<BaseMessagesGroup>();
}