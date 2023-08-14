using System.Net;
using Chatify.Application.Common.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace Chatify.Web.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToBadRequest(this Seq<Error> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Type = null!,
            Title = "Bad Request",
            Status = ( int? )HttpStatusCode.BadRequest,
            Detail = "One or more errors occurred.",
            Extensions = { { "errors", errors.Select(e => e.Message) } }
        };
        return new BadRequestObjectResult(problemDetails);
    }

    public static IActionResult ToBadRequest(this BaseError error)
        => new BadRequestObjectResult(new { Error = error.Message });
    
    public static IResult ToBadRequestResult(this BaseError error)
        => TypedResults.BadRequest(new { Error = error.Message });

    public static IResult ToBadRequestResult(this Seq<Error> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Type = null!,
            Title = "Bad Request",
            Status = ( int? )HttpStatusCode.BadRequest,
            Detail = "One or more errors occurred.",
            Extensions = { { "errors", errors.Select(e => e.Message) } }
        };
        return TypedResults.BadRequest(problemDetails);
    }

    public static IActionResult ToBadRequest(this Error error)
        => ToBadRequest(new Seq<Error>(new[] { error }));
    
    public static IResult ToBadRequestResult(this Error error)
        => ToBadRequestResult(new Seq<Error>(new[] { error }));
}

public static class OneOfExtensions
{
    
    public static async Task<TResult> MatchAsync<TOneOf, T0, T1, TResult>(
        this Task<TOneOf> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1) where TOneOf : OneOfBase<T0, T1>
    {
        var result = await task;
        return result.Match(f0, f1);
    }
    
    public static async Task<TResult> MatchAsync<TOneOf, T0, T1, T2, TResult>(
        this Task<TOneOf> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2
        ) where TOneOf : OneOfBase<T0, T1, T2>
    {
        var result = await task;
        return result.Match(f0, f1, f2);
    }
    
    public static async Task<TResult> MatchAsync<TOneOf, T0, T1, T2, T3, TResult>(
        this Task<TOneOf> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2,
        Func<T3, TResult> f3
        ) where TOneOf : OneOfBase<T0, T1, T2, T3>
    {
        var result = await task;
        return result.Match(f0, f1, f2, f3);
    }
    
    public static async Task<TResult> MatchAsync<T0, T1, TResult>(
        this Task<OneOfBase<T0, T1>> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1)
    {
        var result = await task;
        return result.Match(f0, f1);
    }

    public static async Task<TResult> MatchAsync<T0, T1, T2, TResult>(
        this Task<OneOfBase<T0, T1, T2>> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2
    )
    {
        var result = await task;
        return result.Match(f0, f1, f2);
    }

    public static async Task<TResult> MatchAsync<T0, T1, T2, T3, TResult>(
        this Task<OneOf<T0, T1, T2, T3>> task,
        Func<T0, TResult> f0,
        Func<T1, TResult> f1,
        Func<T2, TResult> f2,
        Func<T3, TResult> f3
    )
    {
        var result = await task;
        return result.Match(f0, f1, f2, f3);
    }
}