namespace Fbs.Lg2;

public sealed class InsufficientBatteryException : DomainException
{
    public InsufficientBatteryException(string message) : base(message) { }
}
