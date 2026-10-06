using System;
using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A sellable product with stock management and auto-reorder.</summary>
public sealed class Product
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private decimal _price;
    private int _stock;
    private int _minStock;
    private Supplier _supplier;
    private readonly List<OrderPosition> _orderPositions = new();
    private readonly List<SupplierOrder> _supplierOrders = new();

    public int ProductId => _id;
    public string Name => _name;
    public decimal Price => _price;
    public int Stock => _stock;
    public int MinStock => _minStock;
    public Supplier Supplier => _supplier;
    public IReadOnlyList<OrderPosition> OrderPositions => _orderPositions;
    public IReadOnlyList<SupplierOrder> SupplierOrders => _supplierOrders;

    public Product(string name, decimal price, int initialStock, int minStock, Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Product name must not be null, empty, or whitespace.");
        }
        if (price < 0m)
        {
            throw new InvalidPriceException($"Product price ({price:C}) must be >= 0.");
        }
        if (initialStock < 0)
        {
            throw new InvalidStockException($"Initial stock ({initialStock}) must be >= 0.");
        }
        if (minStock < 0)
        {
            throw new InvalidStockException($"Minimum stock ({minStock}) must be >= 0.");
        }
        ArgumentNullException.ThrowIfNull(supplier);

        _id = _nextId++;
        _name = name.Trim();
        _price = price;
        _stock = initialStock;
        _minStock = minStock;
        _supplier = supplier;
        _supplier.AttachProduct(this);

        CheckInvariants();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("Product name must not be null, empty, or whitespace.");
        }
        _name = newName.Trim();
        CheckInvariants();
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0m)
        {
            throw new InvalidPriceException($"Product price ({newPrice:C}) must be >= 0.");
        }
        _price = newPrice;
        CheckInvariants();
    }

    public void UpdateMinStock(int newMin)
    {
        if (newMin < 0)
        {
            throw new InvalidStockException($"Minimum stock ({newMin}) must be >= 0.");
        }
        _minStock = newMin;
        CheckInvariants();
    }

    public void DecreaseStock(int amount)
    {
        if (amount <= 0)
        {
            throw new InvalidQuantityException(
                $"Stock decrease amount must be greater than 0 (received: {amount}).");
        }
        if (amount > _stock)
        {
            throw new InvalidStockException(
                $"Cannot decrease stock by {amount}; only {_stock} on hand.");
        }

        _stock -= amount;
        CheckInvariants();

        if (_stock <= _minStock)
        {
            TriggerReorder();
        }
    }

    public void IncreaseStock(int amount)
    {
        if (amount <= 0)
        {
            throw new InvalidQuantityException(
                $"Stock increase amount must be greater than 0 (received: {amount}).");
        }
        _stock += amount;
        CheckInvariants();
    }

    public bool NeedsReorder() => _stock <= _minStock;

    private void TriggerReorder()
    {
        int reorderQuantity = Math.Max(_minStock, _minStock * 2 - _stock);
        var supplierOrder = new SupplierOrder(this, DateTime.UtcNow, reorderQuantity);
        _supplierOrders.Add(supplierOrder);
        _supplier.AttachSupplierOrder(supplierOrder);
    }

    internal void AttachOrderPosition(OrderPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        _orderPositions.Add(position);
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: product id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: product name must not be empty.");
        }
        if (_price < 0m)
        {
            throw new InvariantViolationException(
                $"Invariant violated: product price ({_price}) must be >= 0.");
        }
        if (_stock < 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: product stock ({_stock}) must be >= 0.");
        }
        if (_minStock < 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: product minimum stock ({_minStock}) must be >= 0.");
        }
        if (_supplier is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: product must have a supplier.");
        }
    }
}