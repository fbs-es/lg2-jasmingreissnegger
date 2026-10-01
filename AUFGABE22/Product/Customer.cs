using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>
/// A buyer who can submit reviews for products.
/// </summary>
/// <remarks>
/// Invariants:
///   * CustomerId &gt; 0
///   * Name is never null or whitespace
///   * Reviews is never null
/// </remarks>
public sealed class Customer
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private readonly List<Review> _reviews = new();

    /// <summary>Unique customer id (auto-assigned, never reused).</summary>
    public int CustomerId => _id;

    /// <summary>Customer's full name (never null or whitespace).</summary>
    public string Name => _name;

    /// <summary>Reviews authored by this customer (read-only snapshot).</summary>
    public IReadOnlyList<Review> Reviews => _reviews;

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="name">Full name; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidNameException">Precondition violated: name is null, empty or whitespace.</exception>
    public Customer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");

        _id = _nextId++;
        _name = name.Trim();

        CheckInvariants();
    }

    /// <summary>
    /// Updates the customer's name.
    /// </summary>
    /// <param name="newName">New name; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidNameException">Precondition violated: newName is null, empty or whitespace.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");

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
            throw new InvariantViolationException(
                $"Invariant violated: customer id ({_id}) must be positive.");
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvariantViolationException(
                "Invariant violated: customer name must not be empty.");
    }
}
