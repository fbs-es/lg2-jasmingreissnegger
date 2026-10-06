namespace Fbs.Lg2;

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

public sealed class InvalidBalanceException : DomainException
{
    public InvalidBalanceException(string message) : base(message) { }
}

public sealed class InvalidRentalDurationException : DomainException
{
    public InvalidRentalDurationException(string message) : base(message) { }
}

public sealed class CustomerAlreadyRentingException : DomainException
{
    public CustomerAlreadyRentingException(string message) : base(message) { }
}

public sealed class CustomerBlockedException : DomainException
{
    public CustomerBlockedException(string message) : base(message) { }
}

public sealed class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string message) : base(message) { }
}

public sealed class NoActiveRentalException : DomainException
{
    public NoActiveRentalException(string message) : base(message) { }
}

public sealed class ScooterNotAvailableException : DomainException
{
    public ScooterNotAvailableException(string message) : base(message) { }
}

public sealed class InsufficientBatteryException : DomainException
{
    public InsufficientBatteryException(string message) : base(message) { }
}

public sealed class ScooterInvariantViolationException : DomainException
{
    public ScooterInvariantViolationException(string message) : base(message) { }
}

public sealed class CustomerInvariantViolationException : DomainException
{
    public CustomerInvariantViolationException(string message) : base(message) { }
}