namespace Fbs.Lg2;

public enum OrderStatus
{
    Offen,
    Versendet,
    Storniert
}

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) : base(message) { }
}

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(string message) : base(message) { }
}

public sealed class InvalidStockException : DomainException
{
    public InvalidStockException(string message) : base(message) { }
}

public sealed class InvalidQuantityException : DomainException
{
    public InvalidQuantityException(string message) : base(message) { }
}

public sealed class OrderNotCancellableException : DomainException
{
    public OrderNotCancellableException(string message) : base(message) { }
}

public sealed class OrderNotModifiableException : DomainException
{
    public OrderNotModifiableException(string message) : base(message) { }
}

public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}