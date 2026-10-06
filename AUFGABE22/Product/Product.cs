using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A product sold through the e-commerce portal.</summary>
public sealed class Product
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private decimal _price;
    private Manufacturer _manufacturer;
    private readonly List<Review> _reviews = new();

    public int ProductId => _id;
    public string Name => _name;
    public decimal Price => _price;
    public Manufacturer Manufacturer => _manufacturer;
    public IReadOnlyList<Review> Reviews => _reviews;

    public Product(string name, decimal price, Manufacturer manufacturer)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Product name must not be null, empty, or whitespace.");
        }
        if (price < 0m)
        {
            throw new InvalidPriceException($"Product price ({price:C}) must be >= 0.");
        }
        ArgumentNullException.ThrowIfNull(manufacturer);

        _id = _nextId++;
        _name = name.Trim();
        _price = price;
        _manufacturer = manufacturer;
        _manufacturer.AttachProduct(this);

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

    public decimal AverageStars()
    {
        if (_reviews.Count == 0)
        {
            return 0m;
        }

        decimal sum = 0m;
        foreach (var r in _reviews)
        {
            sum += r.Stars;
        }

        return sum / _reviews.Count;
    }

    internal void AttachReview(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        _reviews.Add(review);
        CheckInvariants();
    }

    internal void DetachReview(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        _reviews.Remove(review);
        CheckInvariants();
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
        if (_manufacturer is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: product must have a manufacturer.");
        }
    }
}