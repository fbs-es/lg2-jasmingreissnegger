using System;

namespace Fbs.Lg2;

/// <summary>A single recorded work session for a temporary employee (Aushilfe).</summary>
public sealed class Arbeitseinsatz
{
    private readonly DateTime _date;
    private readonly int _minutes;
    private bool _billed;

    public DateTime Date => _date;
    public int Minutes => _minutes;
    public bool Billed => _billed;

    internal Arbeitseinsatz(DateTime date, int minutes)
    {
        if (date > DateTime.UtcNow.Date)
        {
            throw new InvalidDateException($"Work record date ({date:yyyy-MM-dd}) must not be in the future.");
        }
        if (minutes <= 0)
        {
            throw new InvalidRangeException($"Work minutes must be greater than 0 (received: {minutes}).");
        }

        _date = date;
        _minutes = minutes;
        _billed = false;
    }

    internal void MarkAsBilled()
    {
        _billed = true;
    }
}