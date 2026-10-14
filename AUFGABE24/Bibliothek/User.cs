using System;

namespace Fbs.Lg2;

/// <summary>A registered library user who can borrow books.</summary>
public sealed class User
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _email;
    private DateTime _registrationDate;

    public int UserId => _id;
    public string Name => _name;
    public string Email => _email;
    public DateTime RegistrationDate => _registrationDate;

    public User(string name, string email, DateTime registrationDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("User name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new InvalidEmailException($"User email ('{email}') is not a valid address.");
        }

        _id = _nextId++;
        _name = name.Trim();
        _email = email.Trim();
        _registrationDate = registrationDate;

        CheckInvariants();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("User name must not be null, empty, or whitespace.");
        }
        _name = newName.Trim();
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: user id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvariantViolationException(
                "Invariant violated: user name must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_email))
        {
            throw new InvariantViolationException(
                "Invariant violated: user email must not be empty.");
        }
    }
}