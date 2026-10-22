namespace Fbs.Lg2;

/// <summary>An advertising contribution: client name and budget.</summary>
public sealed class Werbung : Beitrag
{
    private string _kundenname;
    private decimal _budget;

    public string Kundenname => _kundenname;
    public decimal Budget => _budget;

    public override string Kategorie => "Werbung";

    public Werbung(string titel, int dauerSekunden, string kundenname, decimal verfuegbaresBudget)
        : base(titel, dauerSekunden)
    {
        if (string.IsNullOrWhiteSpace(kundenname))
        {
            throw new InvalidNameException("Customer name must not be null, empty, or whitespace.");
        }
        if (verfuegbaresBudget < 0m)
        {
            throw new InvalidBudgetException(
                $"Available budget must be >= 0 (received: {verfuegbaresBudget:C}).");
        }

        _kundenname = kundenname.Trim();
        _budget = verfuegbaresBudget;
    }

    public override string DetailBeschreibung()
        => $"{Titel} (Kunde: {_kundenname}, Budget: {_budget:C})";
}
