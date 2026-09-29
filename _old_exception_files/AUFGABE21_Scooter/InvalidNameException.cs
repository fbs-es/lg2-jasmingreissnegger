namespace Fbs.Lg2;

public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}
