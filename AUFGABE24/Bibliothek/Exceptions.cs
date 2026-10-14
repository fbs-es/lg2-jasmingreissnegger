namespace Fbs.Lg2;

public enum LoanStatus
{
    Aktiv,
    Zurueckgegeben,
    Ueberzogen
}

public enum BookAvailability
{
    Verfuegbar,
    Ausgeliehen
}

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

public sealed class InvalidIsbnException : DomainException
{
    public InvalidIsbnException(string message) : base(message) { }
}

public sealed class InvalidNameException : DomainException
{
    public InvalidNameException(string message) : base(message) { }
}

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) : base(message) { }
}

public sealed class InvalidLoanDateException : DomainException
{
    public InvalidLoanDateException(string message) : base(message) { }
}

public sealed class BookNotAvailableException : DomainException
{
    public BookNotAvailableException(string message) : base(message) { }
}

public sealed class LoanExtensionException : DomainException
{
    public LoanExtensionException(string message) : base(message) { }
}

public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message) { }
}