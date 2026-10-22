namespace Fbs.Lg2;

/// <summary>A music contribution: a song with artist, album, and music genre.</summary>
public sealed class Musikbeitrag : Beitrag
{
    private string _kuenstler;
    private string _album;
    private string _musikgenre;

    public string Kuenstler => _kuenstler;
    public string Album => _album;
    public string Musikgenre => _musikgenre;

    public override string Kategorie => "Musik";

    public Musikbeitrag(string titel, int dauerSekunden, string kuenstler, string album, string musikgenre)
        : base(titel, dauerSekunden)
    {
        if (string.IsNullOrWhiteSpace(kuenstler))
        {
            throw new InvalidNameException("Artist name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(album))
        {
            throw new InvalidNameException("Album name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(musikgenre))
        {
            throw new InvalidNameException("Music genre must not be null, empty, or whitespace.");
        }

        _kuenstler = kuenstler.Trim();
        _album = album.Trim();
        _musikgenre = musikgenre.Trim();
    }

    public override string DetailBeschreibung()
        => $"{Titel} - {_kuenstler} (Album: {_album}, Genre: {_musikgenre})";
}
