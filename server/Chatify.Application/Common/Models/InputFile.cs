using System.ComponentModel.DataAnnotations;

namespace Chatify.Application.Common.Models;

public class InputFile
{
    [MaxSize(1024 * 1024 * 50L)] public Stream Data { get; set; } = default!;

    public string FileName { get; set; } = default!;

    public long SizeInBytes => Data.Length;
}

public class MaxSizeAttribute : ValidationAttribute
{
    public MaxSizeAttribute(long limit)
        : base($"File size cannot exceed {limit} bytes.")
        => Limit = limit;

    public long Limit { get; set; }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
        => value switch
        {
            Stream stream when stream.Length > Limit => new ValidationResult(ErrorMessage),
            long @long when @long > Limit => new ValidationResult(ErrorMessage),
            _ => ValidationResult.Success
        };
}