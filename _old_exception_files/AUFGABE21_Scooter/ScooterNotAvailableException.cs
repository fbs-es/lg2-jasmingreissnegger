namespace Fbs.Lg2;

public sealed class ScooterNotAvailableException : DomainException
{
    public ScooterNotAvailableException(string message) : base(message) { }
}
