namespace Fbs.Lg2;

public sealed class NoActiveRentalException : DomainException
{
    public NoActiveRentalException(string message) : base(message) { }
}
