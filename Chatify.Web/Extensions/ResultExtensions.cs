using System.Net;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToBadRequest(this Seq<Error> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Type = null!,
            Title = "Bad Request",
            Status = (int?) HttpStatusCode.BadRequest,
            Detail = "One or more errors occurred.",
            Extensions = { { "errors", errors.Select(e => e.Message) } }
        };
        return new BadRequestObjectResult(problemDetails);
    }

    public static IActionResult ToBadRequest(this Error error)
        => ToBadRequest(new Seq<Error>(new [] { error }));
}