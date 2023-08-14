using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;

namespace Chatify.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; }

        public Email(string email)
        {
            Validate(email);
            Value = email;
        }

        private static void Validate(string email)
        {
            var valid = new EmailAddressAttribute().IsValid(email);
            if ( !valid ) throw new InvalidEmailException();
        }

        public static implicit operator string(Email email)
            => email.Value;

        public static implicit operator Email(string email)
            => new(email);

        public override string ToString() => Value;
    }
}

public class InvalidEmailException : Exception
{
    public InvalidEmailException() : base("The provided email was invalid") { }
}