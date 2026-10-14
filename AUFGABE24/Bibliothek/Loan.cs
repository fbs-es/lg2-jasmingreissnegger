using System;

namespace Fbs.Lg2;

/// <summary>A loan of one book by one user, with a planned return date and optional extension.</summary>
public sealed class Loan
{
    private const decimal LateFeePerDay = 0.50m;

    private readonly User _user;
    private readonly Book _book;
    private readonly DateTime _loanDate;
    private DateTime _plannedReturnDate;
    private DateTime? _actualReturnDate;
    private bool _extended;
    private LoanStatus _status;
    private decimal _lateFee;

    public User User => _user;
    public Book Book => _book;
    public DateTime LoanDate => _loanDate;
    public DateTime PlannedReturnDate => _plannedReturnDate;
    public DateTime? ActualReturnDate => _actualReturnDate;
    public bool Extended => _extended;
    public LoanStatus Status => _status;
    public decimal LateFee => _lateFee;

    internal Loan(User user, Book book, DateTime loanDate, DateTime plannedReturnDate)
    {
        _user = user;
        _book = book;
        _loanDate = loanDate;
        _plannedReturnDate = plannedReturnDate;
        _status = LoanStatus.Aktiv;
        _extended = false;
        _lateFee = 0m;

        CheckInvariants();
    }

    public void ExtendLoan(DateTime newPlannedReturnDate)
    {
        if (_status != LoanStatus.Aktiv)
        {
            throw new LoanExtensionException(
                $"Loan can only be extended while active (current status: {_status}).");
        }
        if (_extended)
        {
            throw new LoanExtensionException(
                $"Loan has already been extended once and cannot be extended again.");
        }
        if (newPlannedReturnDate <= _plannedReturnDate)
        {
            throw new LoanExtensionException(
                $"New planned return date ({newPlannedReturnDate:yyyy-MM-dd}) must be after the current planned return date ({_plannedReturnDate:yyyy-MM-dd}).");
        }

        _plannedReturnDate = newPlannedReturnDate;
        _extended = true;
        CheckInvariants();
    }

    public decimal ProcessReturn(DateTime returnDate)
    {
        if (_status != LoanStatus.Aktiv)
        {
            throw new LoanExtensionException(
                $"Loan has already been processed (current status: {_status}).");
        }
        if (returnDate < _loanDate)
        {
            throw new LoanExtensionException(
                $"Return date ({returnDate:yyyy-MM-dd}) must not be before loan date ({_loanDate:yyyy-MM-dd}).");
        }

        _actualReturnDate = returnDate;
        _book.MarkAsAvailable();

        if (returnDate > _plannedReturnDate)
        {
            int overdueDays = (int)Math.Ceiling((returnDate - _plannedReturnDate).TotalDays);
            _lateFee = overdueDays * LateFeePerDay;
            _status = LoanStatus.Ueberzogen;
        }
        else
        {
            _status = LoanStatus.Zurueckgegeben;
        }

        CheckInvariants();
        return _lateFee;
    }

    private void CheckInvariants()
    {
        if (_user is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: loan must reference a user.");
        }
        if (_book is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: loan must reference a book.");
        }
        if (_actualReturnDate is not null && _actualReturnDate < _loanDate)
        {
            throw new InvariantViolationException(
                "Invariant violated: return date must not be before loan date.");
        }
        if (_status == LoanStatus.Aktiv && _actualReturnDate is not null)
        {
            throw new InvariantViolationException(
                "Invariant violated: active loan must not have an actual return date.");
        }
        if (_lateFee < 0m)
        {
            throw new InvariantViolationException(
                $"Invariant violated: late fee ({_lateFee}) must be >= 0.");
        }
    }
}