namespace Fbs.Lg2;

/// <summary>Base class for every contribution in a radio show. Cannot be instantiated directly.</summary>
public abstract class Beitrag
{
    private string _titel;
    private int _dauerSekunden;

    public string Titel => _titel;
    public int DauerSekunden => _dauerSekunden;

    public abstract string Kategorie { get; }

    protected Beitrag(string titel, int dauerSekunden)
    {
        if (string.IsNullOrWhiteSpace(titel))
        {
            throw new InvalidNameException("Title must not be null, empty, or whitespace.");
        }
        if (dauerSekunden <= 0)
        {
            throw new InvalidDurationException(
                $"Duration must be greater than 0 (received: {dauerSekunden} s).");
        }

        _titel = titel.Trim();
        _dauerSekunden = dauerSekunden;
    }

    // internal: only Sendung may change the duration, so the show can check its rules.
    internal void AendereDauer(int neueDauerSekunden)
    {
        if (neueDauerSekunden <= 0)
        {
            throw new InvalidDurationException(
                $"Duration must be greater than 0 (received: {neueDauerSekunden} s).");
        }
        _dauerSekunden = neueDauerSekunden;
    }

    public abstract string DetailBeschreibung();
}
