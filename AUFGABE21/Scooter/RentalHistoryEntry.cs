using System;

namespace Fbs.Lg2;

/// <summary>A completed rental: links a customer and scooter for a time span with a computed cost.</summary>
public sealed class RentalHistoryEntry
{
    private static int _nextId = 1;

    private readonly int _id;
    private readonly Customer _customer;
    private readonly Scooter _scooter;
    private readonly DateTime _startTime;
    private readonly DateTime _endTime;
    private readonly decimal _cost;

    public int RentalId => _id;
    public Customer Customer => _customer;
    public Scooter Scooter => _scooter;
    public DateTime StartTime => _startTime;
    public DateTime EndTime => _endTime;
    public TimeSpan Duration => _endTime - _startTime;
    public decimal Cost => _cost;

    public RentalHistoryEntry(Customer customer, Scooter scooter, DateTime startTime, DateTime endTime, decimal cost)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(scooter);
        if (endTime < startTime)
        {
            throw new ArgumentException("End time must not be before start time.", nameof(endTime));
        }
        if (cost < 0m)
        {
            throw new ArgumentException("Cost must not be negative.", nameof(cost));
        }

        _id = _nextId++;
        _customer = customer;
        _scooter = scooter;
        _startTime = startTime;
        _endTime = endTime;
        _cost = cost;
    }
}