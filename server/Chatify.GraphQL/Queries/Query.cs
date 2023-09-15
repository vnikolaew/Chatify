using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.GraphQL.Queries;

public sealed class Query
{
    [Authorize]
    public PersonalInfo MyClaims([FromServices] IHttpContextAccessor accessor)
        => new PersonalInfo
        {
            Claims = accessor.HttpContext?.User.Claims
                .DistinctBy(c => c.Type)
                .ToDictionary(
                    c => c.Type.Split("/", StringSplitOptions.RemoveEmptyEntries).Last(),
                    c => c.Value)!
        };

    public class PersonalInfo
    {
        public Dictionary<string, string> Claims { get; set; } = new();
    }
}