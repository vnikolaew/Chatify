using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;

namespace Chatify.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; }

    public PhoneNumber(string phoneNumber)
    {
        Validate(phoneNumber);
        Value = phoneNumber;
    }

    private static void Validate(string phoneNumber)
    {
        var valid = new PhoneAttribute().IsValid(phoneNumber);
        if ( !valid ) throw new InvalidPhoneNumberException();
    }

    public static implicit operator string(PhoneNumber phoneNumber)
        => phoneNumber.Value;

    public static implicit operator PhoneNumber(string phoneNumber)
        => new(phoneNumber);

    public override string ToString() => Value;
}

public sealed class InvalidPhoneNumberException : Exception
{
    public InvalidPhoneNumberException()
        : base("Invalid phone number") { }
}