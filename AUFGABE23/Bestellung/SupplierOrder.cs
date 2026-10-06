using System;

namespace Fbs.Lg2;

/// <summary>An independent supplier reorder triggered automatically when stock falls at or below the minimum.</summary>
public sealed class SupplierOrder
{
    private static int _nextId = 1;

    private readonly int _id;
    private readonly Product _product;
    private readonly DateTime _orderDate;
    private readonly int _quantity;

    public int SupplierOrderId => _id;
    public Product Product => _product;
    public DateTime OrderDate => _orderDate;
    public int Quantity => _quantity;

    internal SupplierOrder(Product product, DateTime orderDate, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0)
        {
            throw new InvalidQuantityException(
                $"Supplier order quantity must be greater than 0 (received: {quantity}).");
        }

        _id = _nextId++;
        _product = product;
        _orderDate = orderDate;
        _quantity = quantity;
    }
}