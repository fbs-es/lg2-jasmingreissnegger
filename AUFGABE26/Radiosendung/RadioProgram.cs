using System.Collections.Generic;

namespace Fbs.Lg2;

/// <summary>A daily radio program: the time-ordered list of shows broadcast in one day.</summary>
public sealed class RadioProgram
{
    private readonly List<Sendung> _sendungen = new();

    /// <summary>All shows, always ordered by start time.</summary>
    public IReadOnlyList<Sendung> Sendungen => _sendungen.AsReadOnly();

    public int SendungCount => _sendungen.Count;

    /// <summary>Adds a show at the right place (ordered by start time). Overlapping shows are rejected.</summary>
    public void FuegeSendungHinzu(Sendung sendung)
    {
        ArgumentNullException.ThrowIfNull(sendung);
        TimeSpan neuesEnde = sendung.StartUhrzeit + sendung.MaxDauer;

        int einfuegeIndex = _sendungen.Count;
        for (int i = 0; i < _sendungen.Count; i++)
        {
            var s = _sendungen[i];
            if (s == sendung)
            {
                throw new ShowOverlapException($"Show '{sendung.Name}' is already part of the program.");
            }
            TimeSpan ende = s.StartUhrzeit + s.MaxDauer;
            if (sendung.StartUhrzeit < ende && s.StartUhrzeit < neuesEnde)
            {
                throw new ShowOverlapException(
                    $"Show '{sendung.Name}' ({sendung.StartUhrzeit}-{neuesEnde}) overlaps with '{s.Name}' ({s.StartUhrzeit}-{ende}).");
            }
            if (einfuegeIndex == _sendungen.Count && sendung.StartUhrzeit < s.StartUhrzeit)
            {
                einfuegeIndex = i;
            }
        }
        _sendungen.Insert(einfuegeIndex, sendung);
    }

    public Sendung? FindeSendungZuUhrzeit(TimeSpan uhrzeit)
    {
        foreach (var s in _sendungen)
        {
            TimeSpan ende = s.StartUhrzeit + s.MaxDauer;
            if (uhrzeit >= s.StartUhrzeit && uhrzeit < ende)
            {
                return s;
            }
        }
        return null;
    }
}
