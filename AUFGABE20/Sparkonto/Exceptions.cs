namespace Fbs.Lg2;

public abstract class SparkontoException : Exception
{
    protected SparkontoException(string message) : base(message) { }
    protected SparkontoException(string message, Exception inner) : base(message, inner) { }
}

public sealed class InvalidAmountException : SparkontoException
{
    public InvalidAmountException(string message) : base(message) { }
}

public sealed class InvalidNameException : SparkontoException
{
    public InvalidNameException(string message) : base(message) { }
}

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

public sealed class InvariantViolationException : SparkontoException
{
    public InvariantViolationException(string message) : base(message) { }
}

public sealed class SelfTransferException : SparkontoException
{
    public SelfTransferException(string message) : base(message) { }
}