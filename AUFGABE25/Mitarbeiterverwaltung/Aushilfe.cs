using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>Flexible temporary worker: paid by the hour, only for hours that have not yet been billed.</summary>
public sealed class Aushilfe : Mitarbeiter
{
    private readonly List<Arbeitseinsatz> _arbeitseinsaetze = new();

    public int ArbeitseinsatzCount => _arbeitseinsaetze.Count;
    public int UnbilledHours
    {
        get
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

    public override string Kategorie => "Aushilfe";

    public Aushilfe(string vorname, string nachname, decimal stundensatz)
        : base(vorname, nachname, stundensatz)
    {
    }

    public void ErfasseArbeitseinsatz(Arbeitseinsatz einsatz)
    {
        ArgumentNullException.ThrowIfNull(einsatz);
        _arbeitseinsaetze.Add(einsatz);
    }

    public override decimal BerechneGehalt()
    {
        decimal sum = 0m;
        foreach (var e in _arbeitseinsaetze)
        {
            if (!e.Billed)
            {
                decimal stunden = e.Minutes / 60m;
                sum += Grundgehalt * stunden;
            }
        }
        return sum;
    }
}