using System;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class ArbeitseinsatzTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesRecord()
    {
        var date = DateTime.UtcNow.Date.AddDays(-1);
        var e = new Arbeitseinsatz(date, 120);

        e.Date.ShouldBe(date);
        e.Minutes.ShouldBe(120);
        e.Billed.ShouldBeFalse();
    }

    [Fact]
    public void Ctor_FutureDate_Throws()
    {
        var future = DateTime.UtcNow.Date.AddDays(1);
        Should.Throw<InvalidDateException>(() => new Arbeitseinsatz(future, 60));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Ctor_NonPositiveMinutes_Throws(int minutes)
    {
        var past = DateTime.UtcNow.Date.AddDays(-1);
        Should.Throw<InvalidRangeException>(() => new Arbeitseinsatz(past, minutes));
    }

    [Fact]
    public void MarkAsBilled_SetsBilledTrue()
    {
        var e = new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        e.MarkAsBilled();
        e.Billed.ShouldBeTrue();
    }
}

public class MitarbeiterBaseTests
{
    [Fact]
    public void VollerName_Concatenates()
    {
        var f = new Festangestellter("Anna", "Mueller", 3000m, 2020, 25);
        f.VollerName.ShouldBe("Anna Mueller");
    }

    [Fact]
    public void Ctor_NegativeSalary_Throws()
    {
        Should.Throw<InvalidSalaryException>(() => new Festangestellter("Anna", "M", -1m, 2020, 25));
    }

    [Fact]
    public void Ctor_EmptyFirstName_Throws()
    {
        Should.Throw<InvalidNameException>(() => new Festangestellter("", "Mueller", 3000m, 2020, 25));
    }

    [Fact]
    public void Ctor_WhitespaceLastName_Throws()
    {
        Should.Throw<InvalidNameException>(() => new Festangestellter("Anna", "   ", 3000m, 2020, 25));
    }

    [Fact]
    public void SetzeGehalt_NegativeValue_Throws()
    {
        var f = new Festangestellter("Anna", "M", 3000m, 2020, 25);
        Should.Throw<InvalidSalaryException>(() => f.SetzeGehalt(-1m));
        f.Grundgehalt.ShouldBe(3000m);
    }

    [Fact]
    public void Ctor_TrimsNames()
    {
        var f = new Festangestellter("  Anna  ", "  Mueller  ", 3000m, 2020, 25);
        f.Vorname.ShouldBe("Anna");
        f.Nachname.ShouldBe("Mueller");
    }

    [Fact]
    public void DistinctEmployees_HaveDistinctIds()
    {
        var a = new Festangestellter("A", "A", 100m, 2020, 0);
        var b = new Festangestellter("B", "B", 100m, 2020, 0);
        a.MitarbeiterId.ShouldNotBe(b.MitarbeiterId);
    }
}

public class FestangestellterTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesEmployee()
    {
        var f = new Festangestellter("Anna", "Mueller", 3000m, 2018, 25);

        f.Vorname.ShouldBe("Anna");
        f.Nachname.ShouldBe("Mueller");
        f.Grundgehalt.ShouldBe(3000m);
        f.Einstellungsjahr.ShouldBe(2018);
        f.UrlaubsanspruchTage.ShouldBe(25);
        f.Kategorie.ShouldBe("Festangestellter");
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(3000)]
    public void Ctor_InvalidHiringYear_Throws(int year)
    {
        Should.Throw<InvalidYearException>(() => new Festangestellter("A", "B", 100m, year, 25));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(366)]
    public void Ctor_InvalidVacationDays_Throws(int days)
    {
        Should.Throw<InvalidRangeException>(() => new Festangestellter("A", "B", 100m, 2020, days));
    }

    [Fact]
    public void BerechneGehalt_AddsVacationBonus()
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2018, 20);
        f.BerechneGehalt().ShouldBe(2000m + 20m * 25m);
    }

    [Fact]
    public void BerechneGehalt_AfterSalaryChange_ReflectsNewValue()
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2018, 20);
        f.SetzeGehalt(3000m);
        f.BerechneGehalt().ShouldBe(3000m + 20m * 25m);
    }

    [Fact]
    public void BerechneBetriebszugehoerigkeit_ComputesYearsSinceHiring()
    {
        int currentYear = DateTime.UtcNow.Year;
        var f = new Festangestellter("Anna", "M", 2000m, currentYear - 5, 20);
        f.BerechneBetriebszugehoerigkeit().ShouldBe(5);
    }

    [Fact]
    public void SetzeUrlaubsanspruch_ValidValue_Changes()
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2020, 20);
        f.SetzeUrlaubsanspruch(30);
        f.UrlaubsanspruchTage.ShouldBe(30);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(400)]
    public void SetzeUrlaubsanspruch_Invalid_Throws(int days)
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2020, 20);
        Should.Throw<InvalidRangeException>(() => f.SetzeUrlaubsanspruch(days));
    }
}

public class AuszubildenderTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesApprentice()
    {
        var a = new Auszubildender("Bob", "Schmidt", 2000m, "Fachinformatiker", 2);

        a.Vorname.ShouldBe("Bob");
        a.Ausbildungsberuf.ShouldBe("Fachinformatiker");
        a.Ausbildungsjahr.ShouldBe(2);
        a.Kategorie.ShouldBe("Auszubildender");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidProfession_Throws(string? beruf)
    {
        Should.Throw<InvalidNameException>(() => new Auszubildender("Bob", "S", 2000m, beruf!, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void Ctor_InvalidTrainingYear_Throws(int year)
    {
        Should.Throw<InvalidRangeException>(() => new Auszubildender("Bob", "S", 2000m, "FISI", year));
    }

    [Theory]
    [InlineData(1, 0.50)]
    [InlineData(2, 0.65)]
    [InlineData(3, 0.80)]
    [InlineData(4, 0.90)]
    public void BerechneGehalt_ScalesByTrainingYear(int year, decimal expectedFactor)
    {
        var a = new Auszubildender("Bob", "S", 2000m, "FISI", year);
        a.BerechneGehalt().ShouldBe(2000m * expectedFactor);
    }

    [Fact]
    public void SetzeAusbildungsjahr_ValidValue_Changes()
    {
        var a = new Auszubildender("Bob", "S", 2000m, "FISI", 1);
        a.SetzeAusbildungsjahr(3);
        a.Ausbildungsjahr.ShouldBe(3);
        a.BerechneGehalt().ShouldBe(2000m * 0.80m);
    }
}

public class AushilfeTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesTempWorker()
    {
        var a = new Aushilfe("Clara", "Weber", 25m);
        a.Vorname.ShouldBe("Clara");
        a.Grundgehalt.ShouldBe(25m);
        a.Kategorie.ShouldBe("Aushilfe");
        a.ArbeitseinsatzCount.ShouldBe(0);
    }

    [Fact]
    public void Ctor_NegativeHourlyRate_Throws()
    {
        Should.Throw<InvalidSalaryException>(() => new Aushilfe("Clara", "Weber", -1m));
    }

    [Fact]
    public void ErfasseArbeitseinsatz_AppendsToHistory()
    {
        var a = new Aushilfe("Clara", "Weber", 25m);
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60));
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120));
        a.ArbeitseinsatzCount.ShouldBe(2);
    }

    [Fact]
    public void BerechneGehalt_NoEntries_ReturnsZero()
    {
        var a = new Aushilfe("Clara", "Weber", 25m);
        a.BerechneGehalt().ShouldBe(0m);
    }

    [Fact]
    public void BerechneGehalt_SumsUnbilledHoursTimesRate()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60));
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120));
        a.BerechneGehalt().ShouldBe(20m * (60m / 60m) + 20m * (120m / 60m));
    }

    [Fact]
    public void BerechneGehalt_BilledEntries_AreExcluded()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        var billed = new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        billed.MarkAsBilled();
        a.ErfasseArbeitseinsatz(billed);
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120));
        a.BerechneGehalt().ShouldBe(20m * 2m);
    }

    [Fact]
    public void UnbilledHours_SumsOnlyUnbilled()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        var billed = new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        billed.MarkAsBilled();
        a.ErfasseArbeitseinsatz(billed);
        a.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 180));
        a.UnbilledHours.ShouldBe(180);
    }

    [Fact]
    public void ErfasseArbeitseinsatz_Null_Throws()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        Should.Throw<ArgumentNullException>(() => a.ErfasseArbeitseinsatz(null!));
    }
}

public class GehaltsberechnungPolymorphismTests
{
    [Fact]
    public void DifferentCategories_ProduceDifferentSalaries()
    {
        var fest = new Festangestellter("A", "B", 3000m, 2020, 25);
        var azubi = new Auszubildender("C", "D", 3000m, "FISI", 2);
        var aushilfe = new Aushilfe("E", "F", 25m);

        fest.BerechneGehalt().ShouldNotBe(azubi.BerechneGehalt());
        azubi.BerechneGehalt().ShouldNotBe(aushilfe.BerechneGehalt());
        fest.BerechneGehalt().ShouldNotBe(aushilfe.BerechneGehalt());
    }

    [Fact]
    public void SalaryChange_PropagatesToCalculations()
    {
        var fest = new Festangestellter("A", "B", 2000m, 2020, 0);
        var azubi = new Auszubildender("C", "D", 2000m, "FISI", 2);
        var aushilfe = new Aushilfe("E", "F", 20m);
        aushilfe.ErfasseArbeitseinsatz(new Arbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60));

        decimal beforeFest = fest.BerechneGehalt();
        decimal beforeAzubi = azubi.BerechneGehalt();
        decimal beforeAushilfe = aushilfe.BerechneGehalt();

        fest.SetzeGehalt(3000m);
        azubi.SetzeGehalt(3000m);
        aushilfe.SetzeGehalt(40m);

        fest.BerechneGehalt().ShouldNotBe(beforeFest);
        azubi.BerechneGehalt().ShouldNotBe(beforeAzubi);
        aushilfe.BerechneGehalt().ShouldNotBe(beforeAushilfe);
    }
}