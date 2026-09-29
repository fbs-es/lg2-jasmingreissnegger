namespace Fbs.Lg2;

public sealed class CustomerAlreadyRentingException : DomainException
{
    public CustomerAlreadyRentingException(string message) : base(message) { }
}
