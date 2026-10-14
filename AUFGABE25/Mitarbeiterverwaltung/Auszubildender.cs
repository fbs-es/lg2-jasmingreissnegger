using System;

namespace Fbs.Lg2;

/// <summary>Apprentice: salary scales with the current training year.</summary>
public sealed class Auszubildender : Mitarbeiter
{
    private const int MinTrainingYear = 1;
    private const int MaxTrainingYear = 4;

    private readonly string _ausbildungsberuf;
    private int _ausbildungsjahr;

    public string Ausbildungsberuf => _ausbildungsberuf;
    public int Ausbildungsjahr => _ausbildungsjahr;

    public override string Kategorie => "Auszubildender";

    public Auszubildender(string vorname, string nachname, decimal grundgehalt, string ausbildungsberuf, int ausbildungsjahr)
        : base(vorname, nachname, grundgehalt)
    {
        if (string.IsNullOrWhiteSpace(ausbildungsberuf))
        {
            throw new InvalidNameException("Profession must not be null, empty, or whitespace.");
        }
        if (ausbildungsjahr < MinTrainingYear || ausbildungsjahr > MaxTrainingYear)
        {
            throw new InvalidRangeException(
                $"Training year ({ausbildungsjahr}) must be between {MinTrainingYear} and {MaxTrainingYear}.");
        }

        _ausbildungsberuf = ausbildungsberuf.Trim();
        _ausbildungsjahr = ausbildungsjahr;
        CheckInvariants();
    }

    public void SetzeAusbildungsjahr(int neuesJahr)
    {
        if (neuesJahr < MinTrainingYear || neuesJahr > MaxTrainingYear)
        {
            throw new InvalidRangeException(
                $"Training year ({neuesJahr}) must be between {MinTrainingYear} and {MaxTrainingYear}.");
        }
        _ausbildungsjahr = neuesJahr;
        CheckInvariants();
    }

    public override decimal BerechneGehalt()
    {
        decimal faktor = _ausbildungsjahr switch
        {
            1 => 0.50m,
            2 => 0.65m,
            3 => 0.80m,
            4 => 0.90m,
            _ => 0.50m
        };
        return Grundgehalt * faktor;
    }

    private void CheckInvariants()
    {
        if (string.IsNullOrWhiteSpace(_ausbildungsberuf))
        {
            throw new InvariantViolationException(
                "Invariant violated: profession must not be empty.");
        }
        if (_ausbildungsjahr < MinTrainingYear || _ausbildungsjahr > MaxTrainingYear)
        {
            throw new InvariantViolationException(
                $"Invariant violated: training year ({_ausbildungsjahr}) must be in [{MinTrainingYear}, {MaxTrainingYear}].");
        }
    }
}