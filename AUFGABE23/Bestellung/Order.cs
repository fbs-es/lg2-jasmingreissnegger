using System;
using System.Collections.Generic;
using Fbs.Lg2.Payment;

namespace Fbs.Lg2;

/// <summary>A customer order with one or more positions and a status (open, shipped, cancelled).</summary>
public sealed class Order
{
    private static int _nextId = 1;

    private readonly int _id;
    private DateTime _orderDate;
    private OrderStatus _status;
    private readonly Customer _customer;
    private readonly List<OrderPosition> _positions = new();
    private DateTime? _cancelledAt;
    private readonly IPaymentService _paymentService;
    private decimal _paidAmount;

    public int OrderId => _id;
    public DateTime OrderDate => _orderDate;
    public OrderStatus Status => _status;
    public Customer Customer => _customer;
    public int PositionCount => _positions.Count;
    public DateTime? CancelledAt => _cancelledAt;
    public IReadOnlyList<OrderPosition> Positions => _positions.AsReadOnly();
    public IReadOnlyList<string> PaymentTransactionIds
    {
        get
        {
            var ids = new List<string>();
            foreach (var p in _positions)
            {
                if (p.PaymentTransactionId is not null)
                {
                    ids.Add(p.PaymentTransactionId);
                }
            }
            return ids.AsReadOnly();
        }
    }
    public decimal PaidAmount => _paidAmount;
    public decimal TotalAmount
    {
        get
        {
            decimal total = 0m;
            foreach (var p in _positions)
            {
                total += p.LineTotal;
            }
            return total;
        }
    }

    public bool ContainsProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        foreach (var p in _positions)
        {
            if (p.Product.ProductId == product.ProductId)
            {
                return true;
            }
        }
        return false;
    }

    public Order(Customer customer, DateTime orderDate)
        : this(customer, orderDate, new NullPaymentService())
    {
    }

    public Order(Customer customer, DateTime orderDate, IPaymentService paymentService)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(paymentService);

        _id = _nextId++;
        _customer = customer;
        _orderDate = orderDate;
        _status = OrderStatus.Offen;
        _paymentService = paymentService;
        _customer.AttachOrder(this);

        CheckInvariants();
    }

    public OrderPosition AddPosition(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (_status != OrderStatus.Offen)
        {
            throw new OrderNotModifiableException(
                $"Order {_id} is in status {_status} and can no longer be modified.");
        }
        if (product.Stock < quantity)
        {
            throw new InvalidStockException(
                $"Product '{product.Name}' has only {product.Stock} on hand, but {quantity} were requested.");
        }

        // 1. Validate and build the position (no side effects yet).
        int positionNumber = _positions.Count + 1;
        var position = new OrderPosition(this, product, quantity, positionNumber);

        // 2. Charge first: if the payment is declined, nothing has been changed yet (atomic).
        ChargeForAddedPosition(position);

        // 3. Only now change the state.
        _positions.Add(position);
        product.AttachOrderPosition(position);
        product.DecreaseStock(quantity);

        CheckInvariants();
        return position;
    }

    public void Cancel()
    {
        if (_status != OrderStatus.Offen)
        {
            throw new OrderNotCancellableException(
                $"Order {_id} cannot be cancelled in status {_status}.");
        }

        // Order: 1. refund, 2. restore stock, 3. set status.
        // If a refund fails, the exception leaves stock and status unchanged.
        ProcessRefund();

        foreach (var position in _positions)
        {
            position.Product.IncreaseStock(position.Quantity);
        }

        _cancelledAt = DateTime.UtcNow;
        _status = OrderStatus.Storniert;

        CheckInvariants();
    }

    public void MarkShipped()
    {
        if (_status != OrderStatus.Offen)
        {
            throw new OrderNotModifiableException(
                $"Order {_id} can only be marked shipped from status Offen (current: {_status}).");
        }
        if (_positions.Count == 0)
        {
            throw new EmptyOrderException(
                $"Order {_id} has no positions and cannot be shipped (an order needs at least one position).");
        }
        _status = OrderStatus.Versendet;
        CheckInvariants();
    }

    private void ChargeForAddedPosition(OrderPosition position)
    {
        var result = _paymentService.Charge(position.LineTotal, $"Order {_id} - {position.Product.Name}");
        if (!result.Success)
        {
            throw new InvalidOrderPaymentException(
                $"Payment failed for position {position.PositionNumber}: {result.FailureReason}");
        }
        position.SetPaymentTransactionId(result.TransactionId);
        _paidAmount += position.LineTotal;
    }

    /// <summary>Refunds every charged position with its own transaction id. Already refunded positions are skipped.</summary>
    private void ProcessRefund()
    {
        foreach (var position in _positions)
        {
            if (position.PaymentTransactionId is null || position.IsRefunded)
            {
                continue;
            }
            var result = _paymentService.Refund(position.PaymentTransactionId, position.LineTotal);
            if (!result.Success)
            {
                throw new InvalidOrderPaymentException(
                    $"Refund failed for order {_id}, position {position.PositionNumber}: {result.FailureReason}");
            }
            position.MarkRefunded();
            _paidAmount -= position.LineTotal;
        }
    }

    internal OrderPosition? GetPosition(int positionNumber)
    {
        foreach (var p in _positions)
        {
            if (p.PositionNumber == positionNumber)
            {
                return p;
            }
        }
        return null;
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: order id ({_id}) must be positive.");
        }
        if (_customer is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: order must reference a customer.");
        }
        if (_status == OrderStatus.Versendet && _positions.Count == 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: shipped order {_id} must have at least one position.");
        }
        if (_status == OrderStatus.Storniert && _cancelledAt is null)
        {
            throw new InvariantViolationException(
                $"Invariant violated: cancelled order {_id} must have a cancellation timestamp.");
        }
    }
}

/// <summary>Thrown when the payment service fails to charge or refund.</summary>
public sealed class InvalidOrderPaymentException : DomainException
{
    public InvalidOrderPaymentException(string message) : base(message) { }
}
