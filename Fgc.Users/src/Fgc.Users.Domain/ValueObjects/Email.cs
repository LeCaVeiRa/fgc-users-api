using System;
using System.Diagnostics.CodeAnalysis;

namespace Fgc.Users.Domain.ValueObjects
{
    public sealed class Email
    {
        public required string Value { get; init; }
        private Email() { }

        [SetsRequiredMembers]
        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (!email.Contains("@"))
                throw new ArgumentException("Invalid email format.");

            return new Email(email.Trim().ToLower());
        }
        public override string ToString() => Value;

        public override bool Equals(object? obj)
            => obj is Email other && Value == other.Value;
        public override int GetHashCode()
            => Value.GetHashCode();

    }
}
