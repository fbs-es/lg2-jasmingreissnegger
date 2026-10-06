using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A supplier providing one or more products to the shop.</summary>
public sealed class Supplier
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private readonly List<Product> _products = new();
    private readonly List<SupplierOrder> _supplierOrders = new();

    public int SupplierId => _id;
    public string Name => _name;
    public IReadOnlyList<Product> Products => _products;
    public IReadOnlyList<SupplierOrder> SupplierOrders => _supplierOrders;

    public Supplier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Supplier name must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _name = name.Trim();

        CheckInvariants();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("Supplier name must not be null, empty, or whitespace.");
        }
        _name = newName.Trim();
        CheckInvariants();
    }

    internal void AttachProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Add(product);
    }

    internal void AttachSupplierOrder(SupplierOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _supplierOrders.Add(order);
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: supplier id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: supplier name must not be empty.");
        }
    }
}