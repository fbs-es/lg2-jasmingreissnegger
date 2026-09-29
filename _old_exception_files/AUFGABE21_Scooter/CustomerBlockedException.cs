namespace Fbs.Lg2;

public sealed class CustomerBlockedException : DomainException
{
    public CustomerBlockedException(string message) : base(message) { }
}
