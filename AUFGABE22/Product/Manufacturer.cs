using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>
/// A manufacturer that produces one or more products.
/// </summary>
/// <remarks>
/// Invariants:
///   * ManufacturerId &gt; 0
///   * Name is never null or whitespace
///   * SupportEmail is a valid e-mail address (contains '@' with non-empty local/domain; domain has a '.')
///   * Products is never null
/// </remarks>
public sealed class Manufacturer
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _supportEmail;
    private readonly List<Product> _products = new();

    /// <summary>Unique manufacturer id (auto-assigned).</summary>
    public int ManufacturerId => _id;

    /// <summary>Manufacturer's display name (never null or whitespace).</summary>
    public string Name => _name;

    /// <summary>Contact e-mail for support inquiries (always a valid address).</summary>
    public string SupportEmail => _supportEmail;

    /// <summary>Products produced by this manufacturer (read-only snapshot).</summary>
    public IReadOnlyList<Product> Products => _products;

    /// <summary>
    /// Creates a new manufacturer.
    /// </summary>
    /// <param name="name">Manufacturer name; must not be null, empty or whitespace.</param>
    /// <param name="supportEmail">Support e-mail; must contain '@' with non-empty local and domain parts, and the domain must include a '.'.</param>
    /// <exception cref="InvalidNameException">Precondition violated: name is null, empty or whitespace.</exception>
    /// <exception cref="InvalidEmailException">Precondition violated: supportEmail is not a valid address.</exception>
    public Manufacturer(string name, string supportEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException("Manufacturer name must not be null, empty, or whitespace.");
        if (!IsValidEmail(supportEmail))
            throw new InvalidEmailException($"Manufacturer email ('{supportEmail}') is not a valid address.");

        _id = _nextId++;
        _name = name.Trim();
        _supportEmail = supportEmail.Trim();
        CheckInvariants();
    }

    /// <summary>
    /// Updates the manufacturer name.
    /// </summary>
    /// <param name="newName">New name; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidNameException">Precondition violated: newName is null, empty or whitespace.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidNameException("Manufacturer name must not be null, empty, or whitespace.");
        _name = newName.Trim();
        CheckInvariants();
    }

    /// <summary>
    /// Updates the support e-mail.
    /// </summary>
    /// <param name="newEmail">New support e-mail; must be a valid address.</param>
    /// <exception cref="InvalidEmailException">Precondition violated: newEmail is not a valid address.</exception>
    public void UpdateSupportEmail(string newEmail)
    {
        if (!IsValidEmail(newEmail))
            throw new InvalidEmailException($"Manufacturer email ('{newEmail}') is not a valid address.");
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
            throw new InvariantViolationException(
                $"Invariant violated: manufacturer id ({_id}) must be positive.");
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvariantViolationException(
                "Invariant violated: manufacturer name must not be empty.");
        if (!IsValidEmail(_supportEmail))
            throw new InvariantViolationException(
                "Invariant violated: manufacturer email must be a valid address.");
    }

    private static bool IsValidEmail(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        int at = s.IndexOf('@');
        if (at <= 0 || at == s.Length - 1) return false;
        string domain = s[(at + 1)..];
        if (!domain.Contains('.')) return false;
        if (domain.StartsWith('.') || domain.EndsWith('.')) return false;
        return true;
    }
}
