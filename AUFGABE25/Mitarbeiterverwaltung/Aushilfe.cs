using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>Flexible temporary worker: paid by the hour, only for hours that have not yet been billed.</summary>
public sealed class Aushilfe : Mitarbeiter
{
    private readonly List<Arbeitseinsatz> _arbeitseinsaetze = new();
    private decimal _stundensatz;

    public decimal Stundensatz => _stundensatz;
    public int ArbeitseinsatzCount => _arbeitseinsaetze.Count;
    public IReadOnlyList<Arbeitseinsatz> Arbeitseinsaetze => _arbeitseinsaetze.AsReadOnly();

    /// <summary>Unbilled working time in hours (e.g. 90 minutes = 1.5).</summary>
    public decimal UnbilledHours => UnbilledMinutes() / 60m;

    public override string Kategorie => "Aushilfe";

    public Aushilfe(string vorname, string nachname, decimal stundensatz)
        : base(vorname, nachname)
    {
        if (stundensatz < 0m)
        {
            throw new InvalidSalaryException($"Hourly rate ({stundensatz:C}) must be >= 0.");
        }
        _stundensatz = RundeAufCent(stundensatz);
    }

    public void SetzeStundensatz(decimal neuerStundensatz)
    {
        if (neuerStundensatz < 0m)
        {
            throw new InvalidSalaryException($"Hourly rate ({neuerStundensatz:C}) must be >= 0.");
        }
        _stundensatz = RundeAufCent(neuerStundensatz);
    }

    /// <summary>Records a new work session. The entry is created here, so only the Aushilfe can change it.</summary>
    public Arbeitseinsatz ErfasseArbeitseinsatz(DateTime datum, int minuten)
    {
        var einsatz = new Arbeitseinsatz(datum, minuten);
        _arbeitseinsaetze.Add(einsatz);
        return einsatz;
    }

    /// <summary>Temp raise: the hourly rate is raised by the percentage (rounded to cents).</summary>
    public override void PasseGehaltAn(decimal prozent)
    {
        PruefeProzent(prozent);
        _stundensatz = RundeAufCent(_stundensatz * (1m + prozent / 100m));
    }

    /// <summary>Amount for all unbilled hours, rounded to cents. Does not change anything.</summary>
    public override decimal BerechneGehalt()
    {
        return RundeAufCent(_stundensatz * UnbilledMinutes() / 60m);
    }

    /// <summary>Bills all open work sessions: returns the amount and marks the sessions as billed.</summary>
    public decimal RechneAb()
    {
        decimal betrag = BerechneGehalt();
        foreach (var e in _arbeitseinsaetze)
        {
            if (!e.Billed)
            {
                e.MarkAsBilled();
            }
        }
        return betrag;
    }

    private int UnbilledMinutes()
    {
        int total = 0;
        foreach (var e in _arbeitseinsaetze)
        {
            if (!e.Billed)
            {
                total += e.Minutes;
            }
        }
        return total;
    }
}