using System.Collections.Generic;

namespace Fbs.Lg2.Payment;

/// <summary>In-memory payment service for development and testing. Not for production use.</summary>
public sealed class InMemoryPaymentService : IPaymentService
{
    private readonly Dictionary<string, (decimal Amount, string Description)> _ledger = new();
    private int _nextTxId = 1;

    /// <summary>Charges the given amount and remembers it for later refund.</summary>
    public PaymentResult Charge(decimal amount, string description)
    {
        if (amount <= 0m)
        {
            return PaymentResult.Declined($"Amount must be > 0 (received: {amount}).");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            return PaymentResult.Declined("Description must not be empty.");
        }

        string txId = $"TX-{_nextTxId++:D6}";
        _ledger[txId] = (amount, description);
        return PaymentResult.Approved(txId);
    }

    /// <summary>Refunds a previously-charged transaction. Refund amount may not exceed the original charge.</summary>
    public PaymentResult Refund(string transactionId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return PaymentResult.Declined("Transaction id must not be empty.");
        }
        if (!_ledger.TryGetValue(transactionId, out var entry))
        {
            return PaymentResult.Declined($"Transaction {transactionId} not found.");
        }
        if (amount <= 0m)
        {
            return PaymentResult.Declined($"Refund amount must be > 0 (received: {amount}).");
        }
        if (amount > entry.Amount)
        {
            return PaymentResult.Declined(
                $"Refund amount ({amount}) exceeds original charge ({entry.Amount}).");
        }

        _ledger.Remove(transactionId);
        return PaymentResult.Approved($"{transactionId}-REFUND");
    }
}
