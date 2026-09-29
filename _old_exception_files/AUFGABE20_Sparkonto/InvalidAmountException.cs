namespace Fbs.Lg2;

public sealed class InvalidAmountException : SparkontoException
{
    public InvalidAmountException(string message) : base(message) { }
}
