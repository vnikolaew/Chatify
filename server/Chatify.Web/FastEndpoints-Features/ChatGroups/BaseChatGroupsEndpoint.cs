using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

public sealed class BaseChatGroupsGroup : SubGroup<BaseGroup>
{
    public BaseChatGroupsGroup() =>
        Configure("chatgroups",
            ep => ep.Description(x => x.WithTags("chatgroups")));
}

public abstract class BaseChatGroupsEndpoint<TRequest, TResponse>
    : BaseEndpoint<TRequest, TResponse> where TRequest : notnull
{
    protected BaseChatGroupsEndpoint() => Group<BaseChatGroupsGroup>();
}