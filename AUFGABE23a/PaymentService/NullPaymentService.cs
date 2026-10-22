namespace Fbs.Lg2.Payment;

/// <summary>No-op payment service. Used as the default when no real payment provider is wired in.</summary>
public sealed class NullPaymentService : IPaymentService
{
    /// <summary>Always returns Approved. Does not actually charge anything.</summary>
    public PaymentResult Charge(decimal amount, string description)
    {
        return PaymentResult.Approved("NULL-PAYMENT");
    }

    /// <summary>Always returns Approved. Does not actually refund anything.</summary>
    public PaymentResult Refund(string transactionId, decimal amount)
    {
        return PaymentResult.Approved("NULL-REFUND");
    }
}
