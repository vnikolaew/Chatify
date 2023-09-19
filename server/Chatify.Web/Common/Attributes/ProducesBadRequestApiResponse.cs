using System.Net;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ProducesBadRequestApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesBadRequestApiResponseAttribute()
        : base(typeof(ApiResponse<Unit>), ( int )HttpStatusCode.BadRequest)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesCreatedAtApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesCreatedAtApiResponseAttribute()
        : base(typeof(ApiResponse<object>), ( int )HttpStatusCode.Created)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesNotFoundApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesNotFoundApiResponseAttribute()
        : base(( int )HttpStatusCode.NotFound)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesNoContentApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesNoContentApiResponseAttribute()
        : base(( int )HttpStatusCode.NoContent)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesOkApiResponseAttribute<TData> : ProducesResponseTypeAttribute where TData : notnull
{
    public ProducesOkApiResponseAttribute()
        : base(typeof(ApiResponse<TData>), ( int )HttpStatusCode.OK)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesAcceptedApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesAcceptedApiResponseAttribute()
        : base(( int )HttpStatusCode.Accepted)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesAcceptedApiResponseAttribute<TData> : ProducesResponseTypeAttribute
{
    public ProducesAcceptedApiResponseAttribute()
        : base(typeof(TData), ( int )HttpStatusCode.Accepted)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ProducesRedirectApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesRedirectApiResponseAttribute()
        : base(( int )HttpStatusCode.Redirect)
    {
    }
}