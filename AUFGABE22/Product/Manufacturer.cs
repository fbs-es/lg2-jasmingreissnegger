using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A manufacturer that produces one or more products.</summary>
public sealed class Manufacturer
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _supportEmail;
    private readonly List<Product> _products = new();

    public int ManufacturerId => _id;
    public string Name => _name;
    public string SupportEmail => _supportEmail;
    public IReadOnlyList<Product> Products => _products;

    public Manufacturer(string name, string supportEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Manufacturer name must not be null, empty, or whitespace.");
        }
        if (!IsValidEmail(supportEmail))
        {
            throw new InvalidEmailException($"Manufacturer email ('{supportEmail}') is not a valid address.");
        }

        _id = _nextId++;
        _name = name.Trim();
        _supportEmail = supportEmail.Trim();
        CheckInvariants();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("Manufacturer name must not be null, empty, or whitespace.");
        }
        _name = newName.Trim();
        CheckInvariants();
    }

    public void UpdateSupportEmail(string newEmail)
    {
        if (!IsValidEmail(newEmail))
        {
            throw new InvalidEmailException($"Manufacturer email ('{newEmail}') is not a valid address.");
        }
        _supportEmail = newEmail.Trim();
        CheckInvariants();
    }

    internal void AttachProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Add(product);
        CheckInvariants();
    }

    internal void DetachProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Remove(product);
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: manufacturer id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: manufacturer name must not be empty.");
        }
        if (!IsValidEmail(_supportEmail))
        {
            throw new InvariantViolationException(
                "Invariant violated: manufacturer email must be a valid address.");
        }
    }

    private static bool IsValidEmail(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }
        int at = s.IndexOf('@');
        if (at <= 0 || at == s.Length - 1)
        {
            return false;
        }
        string domain = s[(at + 1)..];
        if (!domain.Contains('.'))
        {
            return false;
        }
        if (domain.StartsWith('.') || domain.EndsWith('.'))
        {
            return false;
        }
        return true;
    }
}