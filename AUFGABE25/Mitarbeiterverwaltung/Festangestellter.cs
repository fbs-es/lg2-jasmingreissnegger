using System;

namespace Fbs.Lg2;

/// <summary>Permanent employee: receives a fixed monthly salary plus vacation entitlement.</summary>
public sealed class Festangestellter : Mitarbeiter
{
    private const decimal VacationBonusPerDay = 25m;
    private const int MinYear = 1900;

    private readonly int _einstellungsjahr;
    private int _urlaubsanspruchTage;

    public int Einstellungsjahr => _einstellungsjahr;
    public int UrlaubsanspruchTage => _urlaubsanspruchTage;

    public override string Kategorie => "Festangestellter";

    public Festangestellter(string vorname, string nachname, decimal grundgehalt, int einstellungsjahr, int urlaubsanspruchTage)
        : base(vorname, nachname, grundgehalt)
    {
        if (einstellungsjahr < MinYear || einstellungsjahr > DateTime.UtcNow.Year)
        {
            throw new InvalidYearException(
                $"Hiring year ({einstellungsjahr}) must be between {MinYear} and {DateTime.UtcNow.Year}.");
        }
        if (urlaubsanspruchTage < 0 || urlaubsanspruchTage > 365)
        {
            throw new InvalidRangeException(
                $"Vacation days ({urlaubsanspruchTage}) must be between 0 and 365.");
        }

        _einstellungsjahr = einstellungsjahr;
        _urlaubsanspruchTage = urlaubsanspruchTage;
        CheckInvariants();
    }

    public void SetzeUrlaubsanspruch(int neueTage)
    {
        if (neueTage < 0 || neueTage > 365)
        {
            throw new InvalidRangeException(
                $"Vacation days ({neueTage}) must be between 0 and 365.");
        }
        _urlaubsanspruchTage = neueTage;
        CheckInvariants();
    }

    public override decimal BerechneGehalt()
    {
        return Grundgehalt + (_urlaubsanspruchTage * VacationBonusPerDay);
    }

    public int BerechneBetriebszugehoerigkeit()
    {
        return DateTime.UtcNow.Year - _einstellungsjahr;
    }

    private void CheckInvariants()
    {
        if (_urlaubsanspruchTage < 0 || _urlaubsanspruchTage > 365)
        {
            throw new InvariantViolationException(
                $"Invariant violated: vacation days ({_urlaubsanspruchTage}) must be in [0, 365].");
        }
    }
}