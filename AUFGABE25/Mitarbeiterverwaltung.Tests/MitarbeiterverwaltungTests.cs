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
    public void BerechneGehalt_IsTariffSalary_VacationDoesNotChangeIt()
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2018, 20);
        f.BerechneGehalt().ShouldBe(2000m);
    }

    [Fact]
    public void PasseGehaltAn_RaisesTariffSalary_RoundedToCents()
    {
        var f = new Festangestellter("Anna", "M", 2345.67m, 2018, 20);
        f.PasseGehaltAn(3m);
        // 2345.67 * 1.03 = 2416.0401 -> 2416.04
        f.Grundgehalt.ShouldBe(2416.04m);
        f.BerechneGehalt().ShouldBe(2416.04m);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-150)]
    public void PasseGehaltAn_MinusHundredPercentOrLess_Throws(int prozent)
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2018, 20);
        Should.Throw<InvalidSalaryException>(() => f.PasseGehaltAn(prozent));
        f.Grundgehalt.ShouldBe(2000m);
    }

    [Fact]
    public void BerechneGehalt_AfterSalaryChange_ReflectsNewValue()
    {
        var f = new Festangestellter("Anna", "M", 2000m, 2018, 20);
        f.SetzeGehalt(3000m);
        f.BerechneGehalt().ShouldBe(3000m);
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

    [Fact]
    public void BerechneGehalt_IsRoundedToCents()
    {
        var a = new Auszubildender("Bob", "S", 1234.57m, "FISI", 2);
        // 1234.57 * 0.65 = 802.4705 -> 802.47
        a.BerechneGehalt().ShouldBe(802.47m);
    }

    [Fact]
    public void PasseGehaltAn_RaisesTariffBase_PayStillDependsOnYear()
    {
        var a = new Auszubildender("Bob", "S", 2000m, "FISI", 1);
        a.PasseGehaltAn(10m);
        a.Grundgehalt.ShouldBe(2200m);
        a.BerechneGehalt().ShouldBe(1100m);
    }
}

public class AushilfeTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesTempWorker()
    {
        var a = new Aushilfe("Clara", "Weber", 25m);
        a.Vorname.ShouldBe("Clara");
        a.Stundensatz.ShouldBe(25m);
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
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120);
        a.ArbeitseinsatzCount.ShouldBe(2);
        a.Arbeitseinsaetze.Count.ShouldBe(2);
        a.Arbeitseinsaetze.ShouldNotBeAssignableTo<System.Collections.Generic.List<Arbeitseinsatz>>();
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
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120);
        a.BerechneGehalt().ShouldBe(20m * (60m / 60m) + 20m * (120m / 60m));
    }

    [Fact]
    public void BerechneGehalt_BilledEntries_AreExcluded()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        a.RechneAb();
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 120);
        a.BerechneGehalt().ShouldBe(20m * 2m);
    }

    [Fact]
    public void UnbilledHours_SumsOnlyUnbilled()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        a.RechneAb();
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 180);
        a.UnbilledHours.ShouldBe(3m);
    }

    [Fact]
    public void UnbilledHours_PartialHours_AreDecimal()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 90);
        a.UnbilledHours.ShouldBe(1.5m);
    }

    [Fact]
    public void ErfasseArbeitseinsatz_InvalidMinutes_Throws_AndAddsNothing()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        Should.Throw<InvalidRangeException>(() => a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 0));
        a.ArbeitseinsatzCount.ShouldBe(0);
    }

    [Fact]
    public void RechneAb_ReturnsAmount_MarksAllBilled_SecondCallReturnsZero()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        var e1 = a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        var e2 = a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-2), 30);

        a.RechneAb().ShouldBe(30m);

        e1.Billed.ShouldBeTrue();
        e2.Billed.ShouldBeTrue();
        a.UnbilledHours.ShouldBe(0m);
        a.RechneAb().ShouldBe(0m);
    }

    [Fact]
    public void BerechneGehalt_IsRoundedToCents()
    {
        var a = new Aushilfe("Clara", "Weber", 12.50m);
        a.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 7);
        // 12.50 * 7 / 60 = 1.458333... -> 1.46
        a.BerechneGehalt().ShouldBe(1.46m);
    }

    [Fact]
    public void PasseGehaltAn_RaisesHourlyRate()
    {
        var a = new Aushilfe("Clara", "Weber", 20m);
        a.PasseGehaltAn(5m);
        a.Stundensatz.ShouldBe(21m);
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
        aushilfe.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);

        decimal beforeFest = fest.BerechneGehalt();
        decimal beforeAzubi = azubi.BerechneGehalt();
        decimal beforeAushilfe = aushilfe.BerechneGehalt();

        fest.SetzeGehalt(3000m);
        azubi.SetzeGehalt(3000m);
        aushilfe.SetzeStundensatz(40m);

        fest.BerechneGehalt().ShouldNotBe(beforeFest);
        azubi.BerechneGehalt().ShouldNotBe(beforeAzubi);
        aushilfe.BerechneGehalt().ShouldNotBe(beforeAushilfe);
    }

    [Fact]
    public void PasseGehaltAn_IsPolymorphic_ViaBaseClassReference()
    {
        var aushilfe = new Aushilfe("E", "F", 20m);
        aushilfe.ErfasseArbeitseinsatz(DateTime.UtcNow.Date.AddDays(-1), 60);
        Mitarbeiter[] alle =
        {
            new Festangestellter("A", "B", 2000m, 2020, 25),
            new Auszubildender("C", "D", 2000m, "FISI", 1),
            aushilfe,
        };

        foreach (var m in alle)
        {
            m.PasseGehaltAn(10m);
        }

        alle[0].BerechneGehalt().ShouldBe(2200m);
        alle[1].BerechneGehalt().ShouldBe(1100m);
        alle[2].BerechneGehalt().ShouldBe(22m);
    }

    [Fact]
    public void Aushilfe_IsNotATarifMitarbeiter()
    {
        new Aushilfe("E", "F", 20m).ShouldNotBeAssignableTo<TarifMitarbeiter>();
        new Festangestellter("A", "B", 2000m, 2020, 25).ShouldBeAssignableTo<TarifMitarbeiter>();
        new Auszubildender("C", "D", 2000m, "FISI", 1).ShouldBeAssignableTo<TarifMitarbeiter>();
    }
}