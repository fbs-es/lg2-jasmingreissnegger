using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>
/// A product sold through the e-commerce portal.
/// </summary>
/// <remarks>
/// Invariants:
///   * ProductId &gt; 0
///   * Name is never null or whitespace
///   * Price is always &gt;= 0
///   * Manufacturer is never null (set at construction, never reassigned)
///   * Reviews is never null
/// </remarks>
public sealed class Product
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private decimal _price;
    private Manufacturer _manufacturer;
    private readonly List<Review> _reviews = new();

    /// <summary>Unique product id (auto-assigned).</summary>
    public int ProductId => _id;

    /// <summary>Product's display name (never null or whitespace).</summary>
    public string Name => _name;

    /// <summary>Current price in EUR; always &gt;= 0.</summary>
    public decimal Price => _price;

    /// <summary>Manufacturer that produces this product (set once at construction, never reassigned).</summary>
    public Manufacturer Manufacturer => _manufacturer;

    /// <summary>Reviews submitted for this product (read-only snapshot).</summary>
    public IReadOnlyList<Review> Reviews => _reviews;

    /// <summary>
    /// Creates a new product and registers it with the manufacturer.
    /// </summary>
    /// <param name="name">Product name; must not be null, empty or whitespace.</param>
    /// <param name="price">Product price in EUR; must be &gt;= 0.</param>
    /// <param name="manufacturer">Manufacturer; must not be null.</param>
    /// <exception cref="InvalidNameException">Precondition violated: name is null, empty or whitespace.</exception>
    /// <exception cref="InvalidPriceException">Precondition violated: price is negative.</exception>
    /// <exception cref="ArgumentNullException">Precondition violated: manufacturer is null.</exception>
    public Product(string name, decimal price, Manufacturer manufacturer)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException("Product name must not be null, empty, or whitespace.");
        if (price < 0m)
            throw new InvalidPriceException($"Product price ({price:C}) must be >= 0.");
        ArgumentNullException.ThrowIfNull(manufacturer);

        _id = _nextId++;
        _name = name.Trim();
        _price = price;
        _manufacturer = manufacturer;
        _manufacturer.AttachProduct(this);

        CheckInvariants();
    }

    /// <summary>
    /// Updates the product name.
    /// </summary>
    /// <param name="newName">New name; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidNameException">Precondition violated: newName is null, empty or whitespace.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidNameException("Product name must not be null, empty, or whitespace.");
        _name = newName.Trim();
        CheckInvariants();
    }

    /// <summary>
    /// Updates the product price.
    /// </summary>
    /// <param name="newPrice">New price in EUR; must be &gt;= 0.</param>
    /// <exception cref="InvalidPriceException">Precondition violated: newPrice is negative.</exception>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0m)
            throw new InvalidPriceException($"Product price ({newPrice:C}) must be >= 0.");
        _price = newPrice;
        CheckInvariants();
    }

    /// <summary>
    /// Computes the arithmetic mean of all review star ratings.
    /// </summary>
    /// <returns>
    /// The mean of all Stars values, in the range 1.0 - 5.0. Returns 0 if the product has no reviews.
    /// </returns>
    public decimal AverageStars()
    {
        if (_reviews.Count == 0)
            return 0m;

        decimal sum = 0m;
        foreach (var r in _reviews)
            sum += r.Stars;

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
            throw new InvariantViolationException(
                $"Invariant violated: product id ({_id}) must be positive.");
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvariantViolationException(
                "Invariant violated: product name must not be empty.");
        if (_price < 0m)
            throw new InvariantViolationException(
                $"Invariant violated: product price ({_price}) must be >= 0.");
        if (_manufacturer is null)
            throw new InvariantViolationException(
                "Invariant violated: product must have a manufacturer.");
    }
}
