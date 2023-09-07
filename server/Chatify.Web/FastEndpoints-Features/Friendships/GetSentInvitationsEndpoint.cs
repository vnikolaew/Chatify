using Chatify.Application.Friendships.Queries;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetSentInvitationsResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Domain.Entities.FriendInvitation>>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpGet("sent")]
public sealed class GetSentInvitationsEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var result = await QueryAsync<GetSentInvitations, GetSentInvitationsResult>(
            new GetSentInvitations(),
            ct);
        return result.Match(
            err => err.ToBadRequestResult(),
            Ok);
    }
}