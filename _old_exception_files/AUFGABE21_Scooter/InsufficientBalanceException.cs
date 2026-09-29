namespace Fbs.Lg2;

public sealed class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string message) : base(message) { }
}
