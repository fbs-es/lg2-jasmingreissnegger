namespace Fbs.Lg2;

public sealed class InsufficientFundsException : SparkontoException
{
    public decimal Requested { get; }
    public decimal Available { get; }

    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Withdrawal rejected: requested {requested:C}, available {available:C}.")
    {
        Requested = requested;
        Available = available;
    }
}
