# AUFGABE 23a - Payment Service API

Auto-generated API documentation for the **payment service** module that decouples payment from order business logic.

Generated from the XML doc comments in the source code using [DocFX](https://dotnet.github.io/docfx/).

## Contents

- [API Reference](api/Fbs.Lg2.Payment.html) — full class index
- [IPaymentService](api/Fbs.Lg2.Payment.IPaymentService.html) — the abstraction
- [InMemoryPaymentService](api/Fbs.Lg2.Payment.InMemoryPaymentService.html) — the dev/test implementation
- [NullPaymentService](api/Fbs.Lg2.Payment.NullPaymentService.html) — the no-op default
- [PaymentResult](api/Fbs.Lg2.Payment.PaymentResult.html) — the return value type

## Architecture

The `Order` class in AUFGABE23 (Bestellung) depends on this module via `IPaymentService` constructor injection. The default constructor wires in `NullPaymentService`; production code would supply a real implementation.

```
AUFGABE23 (Bestellung)        AUFGABE23a (PaymentService)
+--------------------+         +----------------------+
|       Order         |  uses  |   IPaymentService    |
+--------------------+  ----->  +----------------------+
| _paymentService     |         |   InMemoryPayment... |
| _paymentTransaction |         |   NullPaymentService |
+--------------------+         +----------------------+
```

## How this was generated

1. Source code has `<summary>` XML doc comments above public classes and methods.
2. `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the .csproj causes the compiler to emit a `PaymentService.xml` file alongside the DLL on every `dotnet build`.
3. `docfx metadata` reads the XML and converts it to a YAML API model.
4. `docfx build` renders the YAML into a static HTML site in the `_site/` folder.
