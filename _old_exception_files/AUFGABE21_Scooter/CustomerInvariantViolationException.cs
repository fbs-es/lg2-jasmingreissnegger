namespace Fbs.Lg2;

public sealed class CustomerInvariantViolationException : DomainException
{
    public CustomerInvariantViolationException(string message) : base(message) { }
}
