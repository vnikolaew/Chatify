using Chatify.Application.Friendships.Queries;
using Chatify.Web.Extensions;
using FastEndpoints;

using GetIncomingInvitationsResult = OneOf. OneOf<Chatify.Application.Common.Models.BaseError, System.Collections.Generic.List<Chatify.Domain.Entities.FriendInvitation>>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpGet("incoming")]
public sealed class GetIncomingInvitationsEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var result = await QueryAsync<GetIncomingInvitations, GetIncomingInvitationsResult>(
            new GetIncomingInvitations(),
            ct);
        return result.Match(
            err => err.ToBadRequestResult(),
            Ok);
    }
}