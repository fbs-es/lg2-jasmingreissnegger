namespace Fbs.Lg2;

/// <summary>Employee paid according to the collective agreement (Tarif): permanent employees and apprentices.</summary>
public abstract class TarifMitarbeiter : Mitarbeiter
{
    private decimal _grundgehalt;

    /// <summary>Monthly tariff salary (before category-specific calculation).</summary>
    public decimal Grundgehalt => _grundgehalt;

    protected TarifMitarbeiter(string vorname, string nachname, decimal grundgehalt)
        : base(vorname, nachname)
    {
        if (grundgehalt < 0m)
        {
            throw new InvalidSalaryException($"Base salary ({grundgehalt:C}) must be >= 0.");
        }
        _grundgehalt = RundeAufCent(grundgehalt);
        CheckInvariants();
    }

    public void SetzeGehalt(decimal neuesGehalt)
    {
        if (neuesGehalt < 0m)
        {
            throw new InvalidSalaryException($"New salary ({neuesGehalt:C}) must be >= 0.");
        }
        _grundgehalt = RundeAufCent(neuesGehalt);
        CheckInvariants();
    }

    /// <summary>Tariff raise: the tariff base salary is raised by the percentage (rounded to cents).</summary>
    public override void PasseGehaltAn(decimal prozent)
    {
        PruefeProzent(prozent);
        _grundgehalt = RundeAufCent(_grundgehalt * (1m + prozent / 100m));
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_grundgehalt < 0m)
        {
            throw new InvariantViolationException(
                $"Invariant violated: base salary ({_grundgehalt}) must be >= 0.");
        }
    }
}