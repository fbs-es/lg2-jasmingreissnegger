using System;
using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A stationary library that owns books and registered users.</summary>
public sealed class Library
{
    private string _name;
    private string _address;
    private readonly List<Book> _books = new();
    private readonly List<User> _users = new();
    private readonly List<Loan> _loans = new();

    public string Name => _name;
    public string Address => _address;
    public int BookCount => _books.Count;
    public int UserCount => _users.Count;
    public int LoanCount => _loans.Count;
    public int ActiveLoanCount
    {
        get
        {
            int n = 0;
            foreach (var l in _loans)
            {
                if (l.Status == LoanStatus.Aktiv)
                {
                    n++;
                }
            }
            return n;
        }
    }

    public bool HasBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        foreach (var b in _books)
        {
            if (b.BookId == book.BookId)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        foreach (var u in _users)
        {
            if (u.UserId == user.UserId)
            {
                return true;
            }
        }
        return false;
    }

    public Library(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Library name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidNameException("Library address must not be null, empty, or whitespace.");
        }

        _name = name.Trim();
        _address = address.Trim();

        CheckInvariants();
    }

    public void UpdateAddress(string newAddress)
    {
        if (string.IsNullOrWhiteSpace(newAddress))
        {
            throw new InvalidNameException("Library address must not be null, empty, or whitespace.");
        }
        _address = newAddress.Trim();
        CheckInvariants();
    }

    public void AddBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _books.Add(book);
    }

    public void RegisterUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _users.Add(user);
    }

    public Loan StartLoan(User user, Book book, DateTime loanDate, DateTime plannedReturnDate)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(book);
        if (!_users.Contains(user))
        {
            throw new InvalidLoanDateException($"User {user.UserId} is not registered at this library.");
        }
        if (!_books.Contains(book))
        {
            throw new InvalidLoanDateException($"Book {book.BookId} is not in this library's inventory.");
        }
        if (book.Availability == BookAvailability.Ausgeliehen)
        {
            throw new BookNotAvailableException(
                $"Book '{book.Title}' is currently borrowed and cannot be loaned out.");
        }
        if (plannedReturnDate <= loanDate)
        {
            throw new InvalidLoanDateException(
                $"Planned return date ({plannedReturnDate:yyyy-MM-dd}) must be after loan date ({loanDate:yyyy-MM-dd}).");
        }

        var loan = new Loan(user, book, loanDate, plannedReturnDate);
        _loans.Add(loan);
        book.MarkAsBorrowed();
        return loan;
    }

    private void CheckInvariants()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: library name must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_address))
        {
            throw new InvariantViolationException(
                "Invariant violated: library address must not be empty.");
        }
    }
}