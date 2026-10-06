using System.Collections.Generic;

namespace Fbs.Lg2;

public sealed class Scooter
{
    private static int _nextId = 1;

    private readonly int _id;
    private int _batteryLevel;
    private bool _isLocked;
    private readonly List<RentalHistoryEntry> _rentalHistory = new();

    public int ScooterId => _id;
    public int BatteryLevel => _batteryLevel;
    public bool IsLocked => _isLocked;
    public IReadOnlyList<RentalHistoryEntry> RentalHistory => _rentalHistory;

    public Scooter()
    {
        _id = _nextId++;
        _batteryLevel = 100;
        _isLocked = true;
        CheckInvariants();
    }

    internal void Unlock()
    {
        _isLocked = false;
        CheckInvariants();
    }

    internal void Lock()
    {
        _isLocked = true;
        CheckInvariants();
    }

    internal void DrainBattery(int percent)
    {
        if (percent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "Drain amount must be non-negative.");
        }

        _batteryLevel = _batteryLevel - percent;
        if (_batteryLevel < 0)
        {
            _batteryLevel = 0;
        }

        CheckInvariants();
    }

    internal void AttachRentalEntry(RentalHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _rentalHistory.Add(entry);
    }

    private void CheckInvariants()
    {
        if (_batteryLevel < 0 || _batteryLevel > 100)
        {
            throw new ScooterInvariantViolationException(
                $"Invariant violated: battery level ({_batteryLevel}%) must be within [0, 100].");
        }
        if (_id <= 0)
        {
            throw new ScooterInvariantViolationException(
                $"Invariant violated: scooter id ({_id}) must be positive.");
        }
    }
}