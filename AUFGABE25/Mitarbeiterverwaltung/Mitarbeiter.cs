namespace Fbs.Lg2;

/// <summary>Base class for every employee in the company. Cannot be instantiated directly.</summary>
public abstract class Mitarbeiter
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _vorname;
    private string _nachname;

    public int MitarbeiterId => _id;
    public string Vorname => _vorname;
    public string Nachname => _nachname;

    public string VollerName => $"{_vorname} {_nachname}";

    public abstract string Kategorie { get; }
    public abstract decimal BerechneGehalt();

    /// <summary>
    /// Salary adjustment by a percentage (e.g. 3.5 = +3.5 %).
    /// Every category implements this differently (polymorphism).
    /// </summary>
    public abstract void PasseGehaltAn(decimal prozent);

    protected Mitarbeiter(string vorname, string nachname)
    {
        if (string.IsNullOrWhiteSpace(vorname))
        {
            throw new InvalidNameException("First name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(nachname))
        {
            throw new InvalidNameException("Last name must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _vorname = vorname.Trim();
        _nachname = nachname.Trim();

        CheckInvariants();
    }

    /// <summary>Rounds a money amount to whole cents (commercial rounding).</summary>
    protected static decimal RundeAufCent(decimal betrag)
        => Math.Round(betrag, 2, MidpointRounding.AwayFromZero);

    /// <summary>Precondition shared by all salary adjustments: the result must not become negative.</summary>
    protected static void PruefeProzent(decimal prozent)
    {
        if (prozent <= -100m)
        {
            throw new InvalidSalaryException($"Adjustment ({prozent} %) must be greater than -100 %.");
        }
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: employee id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_vorname))
        {
            throw new InvariantViolationException(
                "Invariant violated: first name must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_nachname))
        {
            throw new InvariantViolationException(
                "Invariant violated: last name must not be empty.");
        }
    }
}