using System;

namespace Fbs.Lg2;

/// <summary>One line item within an order, with quantity and the unit price valid at the time of ordering.</summary>
public sealed class OrderPosition
{
    private int _positionNumber;
    private int _quantity;
    private decimal _historicalUnitPrice;
    private readonly Product _product;
    private readonly Order _order;
    private string? _paymentTransactionId;
    private bool _refunded;

    public int PositionNumber => _positionNumber;
    public int Quantity => _quantity;
    public decimal HistoricalUnitPrice => _historicalUnitPrice;
    public decimal LineTotal => _quantity * _historicalUnitPrice;
    public Product Product => _product;
    public Order Order => _order;
    public string? PaymentTransactionId => _paymentTransactionId;
    public bool IsRefunded => _refunded;

    internal OrderPosition(Order order, Product product, int quantity, int positionNumber)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0)
        {
            throw new InvalidQuantityException(
                $"Order position quantity must be greater than 0 (received: {quantity}).");
        }
        if (positionNumber <= 0)
        {
            throw new InvalidQuantityException(
                $"Order position number must be greater than 0 (received: {positionNumber}).");
        }

        _order = order;
        _product = product;
        _quantity = quantity;
        _positionNumber = positionNumber;
        _historicalUnitPrice = product.Price;
    }

    internal void SetPaymentTransactionId(string? transactionId)
    {
        _paymentTransactionId = transactionId;
    }

    internal void MarkRefunded()
    {
        _refunded = true;
    }

    internal void IncreaseQuantity(int additional)
    {
        if (additional <= 0)
        {
            throw new InvalidQuantityException(
                $"Additional quantity must be greater than 0 (received: {additional}).");
        }
        _quantity += additional;
    }
}