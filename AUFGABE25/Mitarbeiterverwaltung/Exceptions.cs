namespace Fbs.Lg2;

/// <summary>Base type for every domain-specific exception thrown by the employee management model.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when a name is null, empty, or whitespace.</summary>
public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}

/// <summary>Thrown when a salary is negative.</summary>
public sealed class InvalidSalaryException : DomainException
{
    public InvalidSalaryException(string message) : base(message) { }
}

/// <summary>Thrown when a year value is outside the plausible employment range.</summary>
public sealed class InvalidYearException : DomainException
{
    public InvalidYearException(string message) : base(message) { }
}

/// <summary>Thrown when vacation days, training year, or hours are out of range.</summary>
public sealed class InvalidRangeException : DomainException
{
    public InvalidRangeException(string message) : base(message) { }
}

/// <summary>Thrown when the document's date is in the future.</summary>
public sealed class InvalidDateException : DomainException
{
    public InvalidDateException(string message) : base(message) { }
}

/// <summary>Thrown when an internal invariant has been violated - indicates a programming error.</summary>
public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}