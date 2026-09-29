namespace Fbs.Lg2;

public sealed class InvalidRentalDurationException : DomainException
{
    public InvalidRentalDurationException(string message) : base(message) { }
}
