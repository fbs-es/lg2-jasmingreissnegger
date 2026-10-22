namespace Fbs.Lg2;

/// <summary>Base type for every domain exception thrown by the radio station model.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when a required string is null, empty, or whitespace.</summary>
public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}

/// <summary>Thrown when a duration is outside the allowed range (zero or negative).</summary>
public sealed class InvalidDurationException : DomainException
{
    public InvalidDurationException(string message) : base(message) { }
}

/// <summary>Thrown when a budget is negative.</summary>
public sealed class InvalidBudgetException : DomainException
{
    public InvalidBudgetException(string message) : base(message) { }
}

/// <summary>Thrown when the urgency level is outside the allowed range (1-5).</summary>
public sealed class InvalidUrgencyException : DomainException
{
    public InvalidUrgencyException(string message) : base(message) { }
}

/// <summary>Thrown when adding a contribution would exceed the show's maximum duration.</summary>
public sealed class ShowOverCapacityException : DomainException
{
    public ShowOverCapacityException(string message) : base(message) { }
}

/// <summary>Thrown when a contribution is not part of the show (or is already part of it).</summary>
public sealed class BeitragMembershipException : DomainException
{
    public BeitragMembershipException(string message) : base(message) { }
}

/// <summary>Thrown when a new show overlaps with an existing show in the program.</summary>
public sealed class ShowOverlapException : DomainException
{
    public ShowOverlapException(string message) : base(message) { }
}

/// <summary>Thrown when an internal invariant has been violated - indicates a programming error.</summary>
public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}
