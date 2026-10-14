namespace Fbs.Lg2;

/// <summary>A book in the library inventory.</summary>
public sealed class Book
{
    private static int _nextId = 1;

    private readonly int _id;
    private readonly string _isbn;
    private string _title;
    private string _author;
    private BookAvailability _availability;

    public int BookId => _id;
    public string Isbn => _isbn;
    public string Title => _title;
    public string Author => _author;
    public BookAvailability Availability => _availability;

    public Book(string isbn, string title, string author)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new InvalidIsbnException("ISBN must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidNameException("Book title must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new InvalidNameException("Book author must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _isbn = isbn.Trim();
        _title = title.Trim();
        _author = author.Trim();
        _availability = BookAvailability.Verfuegbar;

        CheckInvariants();
    }

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new InvalidNameException("Book title must not be null, empty, or whitespace.");
        }
        _title = newTitle.Trim();
        CheckInvariants();
    }

    public void MarkAsBorrowed()
    {
        _availability = BookAvailability.Ausgeliehen;
        CheckInvariants();
    }

    public void MarkAsAvailable()
    {
        _availability = BookAvailability.Verfuegbar;
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: book id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_isbn))
        {
            throw new InvariantViolationException(
                "Invariant violated: book ISBN must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_title))
        {
            throw new InvariantViolationException(
                "Invariant violated: book title must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_author))
        {
            throw new InvariantViolationException(
                "Invariant violated: book author must not be empty.");
        }
    }
}