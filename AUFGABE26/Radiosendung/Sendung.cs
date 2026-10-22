using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A radio show: a fixed time block within a radio program, managed as a schedule of contributions.</summary>
public sealed class Sendung
{
    private readonly List<Beitrag> _beitraege = new();
    private int _aktuellerIndex;

    public string Name { get; }
    public string Moderator { get; }
    public TimeSpan StartUhrzeit { get; }
    public TimeSpan MaxDauer { get; }

    public IReadOnlyList<Beitrag> Beitraege => _beitraege.AsReadOnly();

    /// <summary>The contribution that is currently on air / scheduled next, or null if the show is over or empty.</summary>
    public Beitrag? AktuellerBeitrag => _aktuellerIndex < _beitraege.Count ? _beitraege[_aktuellerIndex] : null;

    public int AnzahlBeitraege => _beitraege.Count;

    public TimeSpan GeplanteGesamtzeit
    {
        get
        {
            int total = 0;
            foreach (var b in _beitraege)
            {
                total += b.DauerSekunden;
            }
            return TimeSpan.FromSeconds(total);
        }
    }

    public TimeSpan VerbleibendeRestzeit => MaxDauer - GeplanteGesamtzeit;

    public Sendung(string name, string moderator, TimeSpan startUhrzeit, TimeSpan maxDauer)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Show name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(moderator))
        {
            throw new InvalidNameException("Moderator name must not be null, empty, or whitespace.");
        }
        if (maxDauer <= TimeSpan.Zero)
        {
            throw new InvalidDurationException(
                $"Max duration must be greater than 0 (received: {maxDauer}).");
        }

        Name = name.Trim();
        Moderator = moderator.Trim();
        StartUhrzeit = startUhrzeit;
        MaxDauer = maxDauer;
    }

    public TimeSpan StartzeitVon(Beitrag beitrag)
    {
        ArgumentNullException.ThrowIfNull(beitrag);
        int offset = 0;
        foreach (var b in _beitraege)
        {
            if (b == beitrag)
            {
                return StartUhrzeit + TimeSpan.FromSeconds(offset);
            }
            offset += b.DauerSekunden;
        }
        throw new BeitragMembershipException(
            $"Contribution '{beitrag.Titel}' is not part of show '{Name}'.");
    }

    public void BelegeNeuePositionEin(Beitrag beitrag)
    {
        ArgumentNullException.ThrowIfNull(beitrag);
        PruefeNichtEnthalten(beitrag);
        if (GeplanteGesamtzeit + TimeSpan.FromSeconds(beitrag.DauerSekunden) > MaxDauer)
        {
            throw new ShowOverCapacityException(
                $"Adding '{beitrag.Titel}' ({beitrag.DauerSekunden} s) would exceed max duration of show '{Name}' " +
                $"({GeplanteGesamtzeit} planned + {beitrag.DauerSekunden} s = {GeplanteGesamtzeit + TimeSpan.FromSeconds(beitrag.DauerSekunden)} > {MaxDauer}).");
        }
        _beitraege.Add(beitrag);
    }

    public void EntferneBeitrag(Beitrag beitrag)
    {
        ArgumentNullException.ThrowIfNull(beitrag);
        int index = IndexVon(beitrag);
        _beitraege.RemoveAt(index);
        if (index < _aktuellerIndex)
        {
            _aktuellerIndex--;
        }
    }

    /// <summary>Shortens a contribution of this show. Making it longer is not allowed (that could break the time limit).</summary>
    public void KuerzeBeitrag(Beitrag beitrag, int neueDauerSekunden)
    {
        ArgumentNullException.ThrowIfNull(beitrag);
        IndexVon(beitrag);
        if (neueDauerSekunden >= beitrag.DauerSekunden)
        {
            throw new InvalidDurationException(
                $"New duration ({neueDauerSekunden} s) must be shorter than the current duration ({beitrag.DauerSekunden} s).");
        }
        beitrag.AendereDauer(neueDauerSekunden);
    }

    /// <summary>The current contribution has finished: the next one becomes the current one.</summary>
    public void WechsleZumNaechstenBeitrag()
    {
        if (AktuellerBeitrag is null)
        {
            throw new InvalidOperationException($"Show '{Name}' has no current contribution any more.");
        }
        _aktuellerIndex++;
    }

    /// <summary>
    /// Inserts breaking news directly before the currently scheduled contribution; later contributions move back.
    /// Breaking news is always inserted, even if the show then exceeds its time limit:
    /// in that case the result contains a warning for the moderator.
    /// </summary>
    public EilmeldungErgebnis FuegeEilmeldungHinzu(Eilmeldung eilmeldung)
    {
        ArgumentNullException.ThrowIfNull(eilmeldung);
        PruefeNichtEnthalten(eilmeldung);

        _beitraege.Insert(_aktuellerIndex, eilmeldung);
        return ErstelleErgebnis(eilmeldung);
    }

    /// <summary>
    /// Optional advanced: inserts breaking news at the current play position of the current contribution.
    /// The current contribution is split into two parts:
    ///   - the first part (what was already played) stays in the schedule
    ///   - the remaining rest is added as a NEW contribution right after the breaking news.
    /// If nothing was played yet, it is inserted before the current contribution;
    /// if the current contribution was already fully played, it is inserted right after it.
    /// Like <see cref="FuegeEilmeldungHinzu"/> it never throws because of the time limit, it warns instead.
    /// </summary>
    public EilmeldungErgebnis FuegeEilmeldungMittenImProgrammHinzu(Eilmeldung eilmeldung, int bereitsAusgestrahlteSekunden)
    {
        ArgumentNullException.ThrowIfNull(eilmeldung);
        if (bereitsAusgestrahlteSekunden < 0)
        {
            throw new InvalidDurationException("Elapsed seconds must be >= 0.");
        }
        PruefeNichtEnthalten(eilmeldung);

        var aktuell = AktuellerBeitrag;
        if (aktuell is null || bereitsAusgestrahlteSekunden == 0)
        {
            return FuegeEilmeldungHinzu(eilmeldung);
        }

        int verbleibend = aktuell.DauerSekunden - bereitsAusgestrahlteSekunden;
        if (verbleibend <= 0)
        {
            // Current contribution is over: breaking news comes right after it and becomes the current one.
            _aktuellerIndex++;
            _beitraege.Insert(_aktuellerIndex, eilmeldung);
            return ErstelleErgebnis(eilmeldung);
        }

        // Create the rest first (may validate), then change the schedule.
        var restBeitrag = SplitteBeitrag(aktuell, verbleibend);
        aktuell.AendereDauer(bereitsAusgestrahlteSekunden);
        _beitraege.Insert(_aktuellerIndex + 1, eilmeldung);
        _beitraege.Insert(_aktuellerIndex + 2, restBeitrag);
        _aktuellerIndex++;

        return ErstelleErgebnis(eilmeldung);
    }

    private EilmeldungErgebnis ErstelleErgebnis(Eilmeldung eilmeldung)
    {
        TimeSpan ueberziehung = GeplanteGesamtzeit - MaxDauer;
        if (ueberziehung <= TimeSpan.Zero)
        {
            return new EilmeldungErgebnis(false, TimeSpan.Zero, null);
        }
        string warnung =
            $"ACHTUNG {Moderator}: Durch die Eilmeldung '{eilmeldung.Titel}' ueberschreitet die Sendung '{Name}' " +
            $"ihr Zeitlimit um {ueberziehung.TotalSeconds:0} s ({GeplanteGesamtzeit} geplant, max. {MaxDauer}).";
        return new EilmeldungErgebnis(true, ueberziehung, warnung);
    }

    private int IndexVon(Beitrag beitrag)
    {
        int index = _beitraege.IndexOf(beitrag);
        if (index < 0)
        {
            throw new BeitragMembershipException(
                $"Contribution '{beitrag.Titel}' is not part of show '{Name}'.");
        }
        return index;
    }

    private void PruefeNichtEnthalten(Beitrag beitrag)
    {
        if (_beitraege.Contains(beitrag))
        {
            throw new BeitragMembershipException(
                $"Contribution '{beitrag.Titel}' is already part of show '{Name}'.");
        }
    }

    private static Beitrag SplitteBeitrag(Beitrag original, int verbleibendeSekunden)
    {
        string titel = original.Titel + " (Fortsetzung)";
        return original switch
        {
            Musikbeitrag m => new Musikbeitrag(titel, verbleibendeSekunden, m.Kuenstler, m.Album, m.Musikgenre),
            Werbung w => new Werbung(titel, verbleibendeSekunden, w.Kundenname, w.Budget),
            Moderation mo => new Moderation(titel, verbleibendeSekunden, mo.Sprechertext),
            Eilmeldung e => new Eilmeldung(titel, verbleibendeSekunden, e.Dringlichkeit, e.BetroffeneRegion),
            _ => throw new InvariantViolationException(
                $"Unknown contribution subtype: {original.GetType().Name}."),
        };
    }
}
