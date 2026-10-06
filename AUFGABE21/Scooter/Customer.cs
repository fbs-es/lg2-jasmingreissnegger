using System;
using System.Collections.Generic;

namespace Fbs.Lg2;

public sealed class Customer
{
    private const decimal MinInitialBalance = 10.00m;
    private const decimal MinStartBalance = 1.00m;
    private const decimal CostPerMinute = 0.20m;
    private const int MinBatteryPercent = 15;

    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _email;
    private decimal _balance;
    private bool _isBlocked;
    private bool _hasOpenDunning;
    private Scooter? _currentRental;
    private DateTime? _currentRentalStartTime;
    private readonly List<string> _sentDunningEmails = new();
    private readonly List<RentalHistoryEntry> _rentalHistory = new();

    public int CustomerId => _id;
    public string Name => _name;
    public string Email => _email;
    public decimal Balance => _balance;
    public bool IsBlocked => _isBlocked;
    public bool HasOpenDunning => _hasOpenDunning;
    public Scooter? CurrentRental => _currentRental;
    public IReadOnlyList<string> SentDunningEmails => _sentDunningEmails;
    public IReadOnlyList<RentalHistoryEntry> RentalHistory => _rentalHistory;

    public Customer(string name, string email, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new InvalidEmailException($"Customer email ('{email}') is not a valid address.");
        }
        if (initialBalance < MinInitialBalance)
        {
            throw new InvalidBalanceException(
                $"Initial balance ({initialBalance:C}) must be at least {MinInitialBalance:C}.");
        }

        _id = _nextId++;
        _name = name.Trim();
        _email = email.Trim();
        _balance = initialBalance;
        _isBlocked = false;
        _hasOpenDunning = false;

        CheckInvariants();
    }

    public void StartRental(Scooter scooter, DateTime startTime)
    {
        ArgumentNullException.ThrowIfNull(scooter);

        if (_currentRental is not null)
        {
            throw new CustomerAlreadyRentingException(
                $"Customer {_id} already has scooter {_currentRental.ScooterId} rented.");
        }
        if (!scooter.IsLocked)
        {
            throw new ScooterNotAvailableException(
                $"Scooter {scooter.ScooterId} is not available (already unlocked).");
        }
        if (scooter.BatteryLevel <= MinBatteryPercent)
        {
            throw new InsufficientBatteryException(
                $"Scooter {scooter.ScooterId} battery ({scooter.BatteryLevel}%) must be above {MinBatteryPercent}%.");
        }
        if (_isBlocked)
        {
            throw new CustomerBlockedException(
                $"Customer {_id} is blocked and cannot start a new rental.");
        }
        if (_balance < MinStartBalance)
        {
            throw new InsufficientBalanceException(
                $"Customer {_id} balance ({_balance:C}) must be at least {MinStartBalance:C} to start a rental.");
        }

        scooter.Unlock();
        _currentRental = scooter;
        _currentRentalStartTime = startTime;

        CheckInvariants();
    }

    public void StartRental(Scooter scooter)
    {
        StartRental(scooter, DateTime.UtcNow);
    }

    public void EndRental(DateTime endTime)
    {
        if (_currentRental is null || _currentRentalStartTime is null)
        {
            throw new NoActiveRentalException(
                $"Customer {_id} has no active rental to end.");
        }
        if (endTime < _currentRentalStartTime)
        {
            throw new InvalidRentalDurationException(
                $"End time ({endTime:O}) must not be before start time ({_currentRentalStartTime:O}).");
        }

        Scooter scooter = _currentRental;
        DateTime startTime = _currentRentalStartTime.Value;
        TimeSpan duration = endTime - startTime;
        int minutes = (int)Math.Ceiling(duration.TotalMinutes);
        if (minutes <= 0)
        {
            minutes = 1;
        }
        decimal cost = CostPerMinute * minutes;

        _balance -= cost;
        scooter.DrainBattery(minutes);
        scooter.Lock();

        var entry = new RentalHistoryEntry(this, scooter, startTime, endTime, cost);
        _rentalHistory.Add(entry);
        scooter.AttachRentalEntry(entry);

        _currentRental = null;
        _currentRentalStartTime = null;

        if (_balance < 0m)
        {
            _isBlocked = true;
            _hasOpenDunning = true;
            _sentDunningEmails.Add(
                $"[Dunning] To: {_email} | Subject: Konto ueberzogen | Body: Stand {_balance:C}, bitte ausgleichen.");
        }

        CheckInvariants();
    }

    public void TopUp(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new InvalidBalanceException(
                $"Top-up amount must be greater than 0 (received: {amount}).");
        }

        _balance += amount;

        if (_balance > 0m && (_isBlocked || _hasOpenDunning))
        {
            _isBlocked = false;
            _hasOpenDunning = false;
        }

        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new CustomerInvariantViolationException(
                $"Invariant violated: customer id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new CustomerInvariantViolationException(
                "Invariant violated: customer name must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_email))
        {
            throw new CustomerInvariantViolationException(
                "Invariant violated: customer email must not be empty.");
        }
    }
}