using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A buyer who places orders in the online shop.</summary>
public sealed class Customer
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _email;
    private readonly List<Order> _orders = new();

    public int CustomerId => _id;
    public string Name => _name;
    public string Email => _email;
    public IReadOnlyList<Order> Orders => _orders;

    public Customer(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new InvalidEmailException($"Customer email ('{email}') is not a valid address.");
        }

        _id = _nextId++;
        _name = name.Trim();
        _email = email.Trim();

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

    internal void AttachOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _orders.Add(order);
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
        if (string.IsNullOrWhiteSpace(_email))
        {
            throw new InvariantViolationException(
                "Invariant violated: customer email must not be empty.");
        }
    }
}