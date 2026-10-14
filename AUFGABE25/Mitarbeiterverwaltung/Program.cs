using Fbs.Lg2;

Console.WriteLine("=== ABC GmbH Mitarbeiterverwaltung Demo ===\n");

Festangestellter anna = new("Anna", "Mueller", grundgehalt: 3000m, einstellungsjahr: 2018, urlaubsanspruchTage: 25);
Console.WriteLine($"Festangestellter: {anna.VollerName}, ID #{anna.MitarbeiterId}");
Console.WriteLine($"  Grundgehalt: {anna.Grundgehalt:C}, Urlaub: {anna.UrlaubsanspruchTage} Tage");
Console.WriteLine($"  Betriebszugehoerigkeit: {anna.BerechneBetriebszugehoerigkeit()} Jahre");
Console.WriteLine($"  Berechnetes Gehalt: {anna.BerechneGehalt():C}");
anna.SetzeGehalt(3200m);
Console.WriteLine($"  Nach Gehaltsaenderung: {anna.BerechneGehalt():C}\n");

Auszubildender bob = new("Bob", "Schmidt", grundgehalt: 2000m, ausbildungsberuf: "Fachinformatiker", ausbildungsjahr: 2);
Console.WriteLine($"Auszubildender: {bob.VollerName}, ID #{bob.MitarbeiterId}");
Console.WriteLine($"  Ausbildungsberuf: {bob.Ausbildungsberuf}, Lehrjahr: {bob.Ausbildungsjahr}");
Console.WriteLine($"  Berechnetes Gehalt (Jahr {bob.Ausbildungsjahr}): {bob.BerechneGehalt():C}");
bob.SetzeAusbildungsjahr(3);
Console.WriteLine($"  Nach Befoerderung ins {bob.Ausbildungsjahr}. Lehrjahr: {bob.BerechneGehalt():C}\n");

Aushilfe clara = new("Clara", "Weber", stundensatz: 25m);
Console.WriteLine($"Aushilfe: {clara.VollerName}, ID #{clara.MitarbeiterId}");
Console.WriteLine($"  Stundensatz: {clara.Grundgehalt:C}");

DateTime today = DateTime.UtcNow.Date;
DateTime yesterday = today.AddDays(-1);
DateTime lastWeek = today.AddDays(-7);

clara.ErfasseArbeitseinsatz(new Arbeitseinsatz(lastWeek, 240));
clara.ErfasseArbeitseinsatz(new Arbeitseinsatz(yesterday, 480));
Console.WriteLine($"  Erfasste Einsaetze: {clara.ArbeitseinsatzCount}, davon unberechnet: {clara.UnbilledHours} min");
Console.WriteLine($"  Berechnetes Gehalt: {clara.BerechneGehalt():C}");

var letzter = new Arbeitseinsatz(today.AddDays(-1), 60);
clara.ErfasseArbeitseinsatz(letzter);
Console.WriteLine($"  Erfasste Einsaetze: {clara.ArbeitseinsatzCount}, davon unberechnet: {clara.UnbilledHours} min");
letzter.MarkAsBilled();
Console.WriteLine($"  Nach Markierung des letzten (60 min) als berechnet: {clara.UnbilledHours} min unberechnet");
Console.WriteLine($"  Berechnetes Gehalt: {clara.BerechneGehalt():C}");

Console.WriteLine("\n=== Demo done ===");
return 0;