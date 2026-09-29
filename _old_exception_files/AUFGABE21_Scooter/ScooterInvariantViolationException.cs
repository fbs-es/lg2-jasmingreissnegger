namespace Fbs.Lg2;

public sealed class ScooterInvariantViolationException : DomainException
{
    public ScooterInvariantViolationException(string message) : base(message) { }
}
