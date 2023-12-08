using System.Net;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ProducesBadRequestApiResponseAttribute()
    : ProducesResponseTypeAttribute(typeof(ApiResponse<Unit>), ( int )HttpStatusCode.BadRequest);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesCreatedAtApiResponseAttribute()
    : ProducesResponseTypeAttribute(typeof(ApiResponse<object>), ( int )HttpStatusCode.Created);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesNotFoundApiResponseAttribute() : ProducesResponseTypeAttribute(( int )HttpStatusCode.NotFound);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesNoContentApiResponseAttribute() : ProducesResponseTypeAttribute(( int )HttpStatusCode.NoContent);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesOkApiResponseAttribute<TData>()
    : ProducesResponseTypeAttribute(typeof(ApiResponse<TData>), ( int )HttpStatusCode.OK)
    where TData : notnull;

[AttributeUsage(AttributeTargets.Method)]
public class ProducesAcceptedApiResponseAttribute() : ProducesResponseTypeAttribute(( int )HttpStatusCode.Accepted);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesAcceptedApiResponseAttribute<TData>()
    : ProducesResponseTypeAttribute(typeof(TData), ( int )HttpStatusCode.Accepted);

[AttributeUsage(AttributeTargets.Method)]
public class ProducesRedirectApiResponseAttribute() : ProducesResponseTypeAttribute(( int )HttpStatusCode.Redirect);