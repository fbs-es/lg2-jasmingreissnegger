using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A buyer who can submit reviews for products.</summary>
public sealed class Customer
{
    private static int _nextId = 1;
    private readonly int _id;
    private string _name;
    private readonly List<Review> _reviews = new();

    public int CustomerId => _id;
    public string Name => _name;
    public int ReviewCount => _reviews.Count;
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();

    public bool HasReviewFor(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        foreach (var r in _reviews)
        {
            if (r.Product.ProductId == product.ProductId)
            {
                return true;
            }
        }
        return false;
    }

    public Customer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _name = name.Trim();

        CheckInvariants();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");
        }

        _name = newName.Trim();

        CheckInvariants();
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
                $"Invariant violated: customer id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: customer name must not be empty.");
        }
    }
}