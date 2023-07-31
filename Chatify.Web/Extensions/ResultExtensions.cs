using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToBadRequest(this Seq<Error> errors)
        => new BadRequestObjectResult(new
        {
            Errors = errors.Select(e => e.Message)
        });

    public static IActionResult ToBadRequest(this Error error)
        => new BadRequestObjectResult(new
        {
            Errors = new[] { error }.Select(e => e.Message)
        });
}