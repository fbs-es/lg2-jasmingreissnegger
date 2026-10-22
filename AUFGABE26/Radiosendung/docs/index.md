# AUFGABE 26 - Radiosendung API

Auto-generated API documentation for the **Cloud-FM radio station management system**.

Generated from the XML doc comments in the source code using [DocFX](https://dotnet.github.io/docfx/).

## Contents

- [API Reference](api/Fbs.Lg2.html) — full class index
- [RadioSender](api/Fbs.Lg2.RadioSender.html) — the radio station
- [RadioProgram](api/Fbs.Lg2.RadioProgram.html) — daily show list
- [Sendung](api/Fbs.Lg2.Sendung.html) — a single radio show with schedule
- [Beitrag](api/Fbs.Lg2.Beitrag.html) — base class for contributions
- [Musikbeitrag](api/Fbs.Lg2.Musikbeitrag.html) — a music track
- [Werbung](api/Fbs.Lg2.Werbung.html) — an advertising slot
- [Eilmeldung](api/Fbs.Lg2.Eilmeldung.html) — an emergency announcement

## How this was generated

1. Source code has `<summary>` XML doc comments above public classes and methods.
2. `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the .csproj causes the compiler to emit an XML file alongside the DLL on every `dotnet build`.
3. `docfx metadata` reads the XML and converts it to a YAML API model.
4. `docfx build` renders the YAML into a static HTML site in the `_site/` folder.
