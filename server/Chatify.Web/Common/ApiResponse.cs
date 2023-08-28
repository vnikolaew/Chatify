using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Common;

public class ApiResponse<T> where T : notnull
{
    public ApiResponseStatus Status { get; set; }

    public string[]? Errors { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public static ApiResponse<T> Success(
        T data,
        string? message = default,
        DateTime? timestamp = default)
        => new()
        {
            Data = data,
            Message = message,
            Timestamp = timestamp ?? DateTime.Now,
            Status = ApiResponseStatus.Success
        };

    public static ApiResponse<T> Failure(
        string?[] errors,
        string? message = default,
        DateTime? timestamp = default)
        => new()
        {
            Message = message,
            Errors = errors.ToArray(),
            Timestamp = timestamp ?? DateTime.Now,
            Status = ApiResponseStatus.Failure
        };
}

public enum ApiResponseStatus : sbyte
{
    Success,
    Failure,
}

public static class ApiResponseExtensions
{
    public static IActionResult Ok<T>(this ApiResponse<T> response)
        => new OkObjectResult(response);
}