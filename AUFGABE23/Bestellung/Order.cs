using System;
using System.Collections.Generic;

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

    public int OrderId => _id;
    public DateTime OrderDate => _orderDate;
    public OrderStatus Status => _status;
    public Customer Customer => _customer;
    public IReadOnlyList<OrderPosition> Positions => _positions;
    public DateTime? CancelledAt => _cancelledAt;
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

    public Order(Customer customer, DateTime orderDate)
    {
        ArgumentNullException.ThrowIfNull(customer);

        _id = _nextId++;
        _customer = customer;
        _orderDate = orderDate;
        _status = OrderStatus.Offen;
        _customer.AttachOrder(this);

        CheckInvariants();
    }

    public OrderPosition AddPosition(Product product, int quantity)
    {
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

        int positionNumber = _positions.Count + 1;
        var position = new OrderPosition(this, product, quantity, positionNumber);
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

        foreach (var position in _positions)
        {
            position.Product.IncreaseStock(position.Quantity);
        }

        ProcessRefund();
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
        _status = OrderStatus.Versendet;
        CheckInvariants();
    }

    private void ProcessRefund()
    {
        // In a real system, this would call a payment gateway.
        // For our model, the refund is implicit: the stock has been returned
        // and the order is marked Storniert. A separate payment service
        // (out of scope) would observe the status change and refund the customer.
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
        if (_status == OrderStatus.Storniert && _cancelledAt is null)
        {
            throw new InvariantViolationException(
                $"Invariant violated: cancelled order {_id} must have a cancellation timestamp.");
        }
    }
}