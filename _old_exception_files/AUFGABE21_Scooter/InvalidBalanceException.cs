namespace Fbs.Lg2;

public sealed class InvalidBalanceException : DomainException
{
    public InvalidBalanceException(string message) : base(message) { }
}
