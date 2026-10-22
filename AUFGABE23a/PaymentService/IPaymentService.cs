namespace Fbs.Lg2.Payment;

/// <summary>Outcome of a payment charge or refund attempt.</summary>
public sealed class PaymentResult
{
    public bool Success { get; }
    public string? TransactionId { get; }
    public string? FailureReason { get; }

    private PaymentResult(bool success, string? transactionId, string? failureReason)
    {
        Success = success;
        TransactionId = transactionId;
        FailureReason = failureReason;
    }

    public static PaymentResult Approved(string transactionId)
        => new(true, transactionId, null);

    public static PaymentResult Declined(string reason)
        => new(false, null, reason);
}

/// <summary>Abstraction over a payment provider. Implementations can be Stripe, PayPal, in-memory test double, etc.</summary>
public interface IPaymentService
{
    /// <summary>Charges the given amount to the customer's payment method.</summary>
    PaymentResult Charge(decimal amount, string description);

    /// <summary>Refunds a previously-successful charge, identified by transactionId.</summary>
    PaymentResult Refund(string transactionId, decimal amount);
}
