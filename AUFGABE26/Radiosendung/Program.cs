using Fbs.Lg2;

Console.WriteLine("=== Cloud-FM Radiosender Demo ===\n");

RadioSender sender = new("Cloud-FM", "100.4 MHz");
Console.WriteLine($"Sender: {sender.Name} auf {sender.UebertragungsFrequenz}");

Sendung morningShow = new(
    "MorgenShow",
    "Anna Berger",
    new TimeSpan(8, 0, 0),
    TimeSpan.FromMinutes(60));
sender.Program.FuegeSendungHinzu(morningShow);
Console.WriteLine($"\nSendung: {morningShow.Name} mit {morningShow.Moderator}");
Console.WriteLine($"  {morningShow.StartUhrzeit} - {morningShow.StartUhrzeit + morningShow.MaxDauer}");

Musikbeitrag song1 = new("Bohemian Rhapsody", 355, "Queen", "A Night at the Opera", "Rock");
Musikbeitrag song2 = new("Imagine", 183, "John Lennon", "Imagine", "Rock");
Werbung spot = new("Frühstücksflakes", 30, "CerealCo", 5000m);
Werbung ad2 = new("Kaffee", 20, "BeanBrand", 2000m);
Moderation intro = new("Begruessung", 40, "Guten Morgen Wien, hier ist Cloud-FM!");

morningShow.BelegeNeuePositionEin(intro);
morningShow.BelegeNeuePositionEin(song1);
morningShow.BelegeNeuePositionEin(spot);
morningShow.BelegeNeuePositionEin(song2);
morningShow.BelegeNeuePositionEin(ad2);

Console.WriteLine($"\nGeplante Beitraege: {morningShow.AnzahlBeitraege}");
Console.WriteLine($"Geplante Gesamtzeit: {morningShow.GeplanteGesamtzeit}");
Console.WriteLine($"Verbleibende Restzeit: {morningShow.VerbleibendeRestzeit}");

Console.WriteLine("\nSendeplan mit Startzeiten:");
foreach (var b in morningShow.Beitraege)
{
    Console.WriteLine($"  {morningShow.StartzeitVon(b):hh\\:mm\\:ss} - {b.Titel} ({b.DauerSekunden} s) [{b.Kategorie}]");
}

Console.WriteLine("\n=== Eilmeldung kommt rein! (Default: direkt vor dem aktuell geplanten Beitrag) ===");
morningShow.WechsleZumNaechstenBeitrag(); // Begruessung ist vorbei, jetzt laeuft song1
Eilmeldung breaking = new("Erdbeben in Tokio", 45, 4, "Welt");
EilmeldungErgebnis ergebnis = morningShow.FuegeEilmeldungHinzu(breaking);
Console.WriteLine(ergebnis.Warnung ?? "Zeitlimit eingehalten.");

Console.WriteLine($"\nAktualisierter Sendeplan ({morningShow.AnzahlBeitraege} Beitraege, {morningShow.GeplanteGesamtzeit}):");
foreach (var b in morningShow.Beitraege)
{
    Console.WriteLine($"  {morningShow.StartzeitVon(b):hh\\:mm\\:ss} - {b.Titel} ({b.DauerSekunden} s) [{b.Kategorie}]");
}

Console.WriteLine("\n=== Sendeung 2: Notfall mitten in der Sendung (Splitting) ===");
Sendung eveningShow = new(
    "AbendShow",
    "Bob Schmidt",
    new TimeSpan(20, 0, 0),
    TimeSpan.FromMinutes(45));
Musikbeitrag songA = new("Stairway to Heaven", 480, "Led Zeppelin", "IV", "Rock");
Musikbeitrag songB = new("Bohemian Rhapsody", 355, "Queen", "A Night at the Opera", "Rock");
Musikbeitrag songC = new("Imagine", 183, "John Lennon", "Imagine", "Rock");
eveningShow.BelegeNeuePositionEin(songA);
eveningShow.BelegeNeuePositionEin(songB);
eveningShow.BelegeNeuePositionEin(songC);

Console.WriteLine("Vorher:");
foreach (var b in eveningShow.Beitraege)
{
    Console.WriteLine($"  {eveningShow.StartzeitVon(b):hh\\:mm\\:ss} - {b.Titel} ({b.DauerSekunden} s)");
}

Console.WriteLine($"\nEilmeldung kommt nach 3 Minuten in '{songA.Titel}' (180s bereits ausgestrahlt)...");
Eilmeldung news = new("Stromausfall in Wien", 2400, 5, "Wien");
EilmeldungErgebnis ergebnis2 = eveningShow.FuegeEilmeldungMittenImProgrammHinzu(news, bereitsAusgestrahlteSekunden: 180);
Console.WriteLine(ergebnis2.Warnung ?? "Zeitlimit eingehalten.");

Console.WriteLine($"\nNachher ({eveningShow.AnzahlBeitraege} Beitraege, {eveningShow.GeplanteGesamtzeit}):");
foreach (var b in eveningShow.Beitraege)
{
    Console.WriteLine($"  {eveningShow.StartzeitVon(b):hh\\:mm\\:ss} - {b.Titel} ({b.DauerSekunden} s) [{b.Kategorie}]");
}

Console.WriteLine("\n=== Demo done ===");
return 0;