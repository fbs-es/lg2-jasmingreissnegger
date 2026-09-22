# csharp-lehrgang-1

## Neue Aufgabe

```bash
./neu.sh
```

Fragt Ordnername, Beschreibung und Klassenname ab. Ergebnis:

```
AUFGABE01/
  Aufgabe01/          dein Code (.cs-Dateien hier anlegen)
  Aufgabe01.Tests/    deine Tests (.cs-Dateien hier anlegen)
```

## Testen

```bash
./test.sh              # alle Aufgaben
./test.sh AUFGABE01    # eine Aufgabe
```

Ohne installiertes .NET 10 SDK laufen beide Skripte automatisch im Devcontainer (Docker nötig).
