namespace Fbs.Lg2;

/// <summary>A moderation: text the moderator speaks live.</summary>
public sealed class Moderation : Beitrag
{
    private string _sprechertext;

    public string Sprechertext => _sprechertext;

    public override string Kategorie => "Moderation";

    public Moderation(string titel, int dauerSekunden, string sprechertext)
        : base(titel, dauerSekunden)
    {
        if (string.IsNullOrWhiteSpace(sprechertext))
        {
            throw new InvalidNameException("Speaker text must not be null, empty, or whitespace.");
        }
        _sprechertext = sprechertext.Trim();
    }

    public override string DetailBeschreibung()
        => $"{Titel} (Moderation: {_sprechertext})";
}
