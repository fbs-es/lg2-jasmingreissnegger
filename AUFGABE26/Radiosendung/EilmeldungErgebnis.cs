namespace Fbs.Lg2;

/// <summary>
/// Result of inserting breaking news. Breaking news is always inserted, even if the show then
/// runs over its time limit - in that case <see cref="Warnung"/> tells the moderator clearly.
/// </summary>
public sealed class EilmeldungErgebnis
{
    public bool ZeitlimitUeberschritten { get; }
    public TimeSpan Ueberziehung { get; }
    public string? Warnung { get; }

    internal EilmeldungErgebnis(bool zeitlimitUeberschritten, TimeSpan ueberziehung, string? warnung)
    {
        ZeitlimitUeberschritten = zeitlimitUeberschritten;
        Ueberziehung = ueberziehung;
        Warnung = warnung;
    }
}
