namespace Fbs.Lg2;

/// <summary>Breaking news: urgency level (1-5) and affected region. May be inserted into a running show.</summary>
public sealed class Eilmeldung : Beitrag
{
    private const int MinUrgency = 1;
    private const int MaxUrgency = 5;

    private int _dringlichkeit;
    private string _betroffeneRegion;

    public int Dringlichkeit => _dringlichkeit;
    public string BetroffeneRegion => _betroffeneRegion;

    public override string Kategorie => "Eilmeldung";

    public Eilmeldung(string titel, int dauerSekunden, int dringlichkeit, string betroffeneRegion)
        : base(titel, dauerSekunden)
    {
        if (dringlichkeit < MinUrgency || dringlichkeit > MaxUrgency)
        {
            throw new InvalidUrgencyException(
                $"Urgency must be between {MinUrgency} and {MaxUrgency} (received: {dringlichkeit}).");
        }
        if (string.IsNullOrWhiteSpace(betroffeneRegion))
        {
            throw new InvalidNameException("Affected region must not be null, empty, or whitespace.");
        }

        _dringlichkeit = dringlichkeit;
        _betroffeneRegion = betroffeneRegion.Trim();
    }

    public override string DetailBeschreibung()
        => $"{Titel} (Dringlichkeit {_dringlichkeit}/5, Region: {_betroffeneRegion})";
}
