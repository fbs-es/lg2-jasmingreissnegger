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

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(string message) : base(message) { }
}

public sealed class InvalidStarsException : DomainException
{
    public InvalidStarsException(string message) : base(message) { }
}

public sealed class InvalidCommentException : DomainException
{
    public InvalidCommentException(string message) : base(message) { }
}

public sealed class InvalidDateException : DomainException
{
    public InvalidDateException(string message) : base(message) { }
}

public sealed class ReviewAlreadyRepliedException : DomainException
{
    public ReviewAlreadyRepliedException(string message) : base(message) { }
}

public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}