namespace Fbs.Lg2;

/// <summary>Base class for every employee in the company. Cannot be instantiated directly.</summary>
public abstract class Mitarbeiter
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _vorname;
    private string _nachname;
    private decimal _grundgehalt;

    public int MitarbeiterId => _id;
    public string Vorname => _vorname;
    public string Nachname => _nachname;
    public decimal Grundgehalt => _grundgehalt;

    public string VollerName => $"{_vorname} {_nachname}";

    public abstract string Kategorie { get; }
    public abstract decimal BerechneGehalt();

    protected Mitarbeiter(string vorname, string nachname, decimal grundgehalt)
    {
        if (string.IsNullOrWhiteSpace(vorname))
        {
            throw new InvalidNameException("First name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(nachname))
        {
            throw new InvalidNameException("Last name must not be null, empty, or whitespace.");
        }
        if (grundgehalt < 0m)
        {
            throw new InvalidSalaryException($"Base salary ({grundgehalt:C}) must be >= 0.");
        }

        _id = _nextId++;
        _vorname = vorname.Trim();
        _nachname = nachname.Trim();
        _grundgehalt = grundgehalt;

        CheckInvariants();
    }

    public void SetzeGehalt(decimal neuesGehalt)
    {
        if (neuesGehalt < 0m)
        {
            throw new InvalidSalaryException($"New salary ({neuesGehalt:C}) must be >= 0.");
        }
        _grundgehalt = neuesGehalt;
        CheckInvariants();
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
        if (_grundgehalt < 0m)
        {
            throw new InvariantViolationException(
                $"Invariant violated: base salary ({_grundgehalt}) must be >= 0.");
        }
    }
}