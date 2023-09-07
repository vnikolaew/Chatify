using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

public sealed class BaseFriendshipsGroup : SubGroup<BaseGroup>
{
    public BaseFriendshipsGroup() =>
        Configure("friendships",
            ep => ep.Description(x => x.WithTags("friendships")));
}

public abstract class BaseFriendshipsEndpoint<TRequest, TResponse>
    : BaseEndpoint<TRequest, TResponse> where TRequest : notnull
{
    protected BaseFriendshipsEndpoint()
        => Group<BaseFriendshipsGroup>();
}