# AUFGABE 25 - Mitarbeiterverwaltung API

Auto-generated API documentation for the **ABC GmbH Employee Management** system.

Generated from the XML doc comments in the source code using [DocFX](https://dotnet.github.io/docfx/).

## Contents

- [API Reference](api/Fbs.Lg2.html) — full class index
- [Mitarbeiter](api/Fbs.Lg2.Mitarbeiter.html) — abstract base class
- [Festangestellter](api/Fbs.Lg2.Festangestellter.html) — permanent employee
- [Auszubildender](api/Fbs.Lg2.Auszubildender.html) — apprentice
- [Aushilfe](api/Fbs.Lg2.Aushilfe.html) — temporary worker
- [Arbeitseinsatz](api/Fbs.Lg2.Arbeitseinsatz.html) — work record for temp workers

## How this was generated

1. Source code has `<summary>` XML doc comments above public classes and methods.
2. `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the .csproj causes the compiler to emit a `Mitarbeiterverwaltung.xml` file alongside the DLL on every `dotnet build`.
3. `docfx metadata` reads the XML and converts it to a YAML API model.
4. `docfx build` renders the YAML into a static HTML site in the `_site/` folder.
