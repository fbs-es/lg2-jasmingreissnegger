namespace Fbs.Lg2;

/// <summary>Base type for every domain-specific exception thrown by the review-portal model.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when a textual argument is null, empty or whitespace.</summary>
public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}

/// <summary>Thrown when an email-like value is missing the basic format markers ('@' or '.' in domain).</summary>
public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) : base(message) { }
}

/// <summary>Thrown when a price argument is negative.</summary>
public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(string message) : base(message) { }
}

/// <summary>Thrown when the star rating is outside the allowed 1..5 range.</summary>
public sealed class InvalidStarsException : DomainException
{
    public InvalidStarsException(string message) : base(message) { }
}

/// <summary>Thrown when a comment-like argument is null, empty or whitespace.</summary>
public sealed class InvalidCommentException : DomainException
{
    public InvalidCommentException(string message) : base(message) { }
}

/// <summary>Thrown when an attempt is made to add a second reply to a review that already has one.</summary>
public sealed class ReviewAlreadyRepliedException : DomainException
{
    public ReviewAlreadyRepliedException(string message) : base(message) { }
}

/// <summary>Thrown when an internal invariant has been violated - indicates a programming error.</summary>
public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}
