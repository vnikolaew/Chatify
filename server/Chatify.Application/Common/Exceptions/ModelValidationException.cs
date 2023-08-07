using System.ComponentModel.DataAnnotations;

namespace Chatify.Application.Common.Exceptions;

public class ModelValidationException : Exception
{
    public ModelValidationException()
        : base("One or more validation exceptions have occured")
        => Errors = new List<string>();

    public IList<string> Errors { get; set; }

    public ModelValidationException(IEnumerable<ValidationResult> failures)
        : this()
    {
        Errors = failures
            .Select(f => f.ErrorMessage)
            .Where(e => e is not null)
            .ToList()!;
    }
}