using System.ComponentModel.DataAnnotations;

namespace Chatify.Application.Common.Exceptions;

public class ModelValidationException() : Exception("One or more validation exceptions have occured")
{
    public IList<string> Errors { get; set; } = new List<string>();

    public ModelValidationException(IEnumerable<ValidationResult> failures)
        : this()
    {
        Errors = failures
            .Select(f => f.ErrorMessage)
            .Where(e => e is not null)
            .ToList()!;
    }
}