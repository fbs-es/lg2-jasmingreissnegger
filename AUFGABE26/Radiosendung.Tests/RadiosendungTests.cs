using System;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class BeitragBaseTests
{
    [Fact]
    public void Musikbeitrag_Kategorie_Is_Musik()
    {
        var m = new Musikbeitrag("Song", 200, "Artist", "Album", "Genre");
        m.Kategorie.ShouldBe("Musik");
    }

    [Fact]
    public void Werbung_Kategorie_Is_Werbung()
    {
        var w = new Werbung("Ad", 30, "Customer", 1000m);
        w.Kategorie.ShouldBe("Werbung");
    }

    [Fact]
    public void Eilmeldung_Kategorie_Is_Eilmeldung()
    {
        var e = new Eilmeldung("Breaking", 45, 3, "Wien");
        e.Kategorie.ShouldBe("Eilmeldung");
    }

    [Fact]
    public void Musikbeitrag_EmptyTitle_Throws()
    {
        Should.Throw<InvalidNameException>(() =>
            new Musikbeitrag("", 200, "Artist", "Album", "Genre"));
    }

    [Fact]
    public void Musikbeitrag_EmptyArtist_Throws()
    {
        Should.Throw<InvalidNameException>(() =>
            new Musikbeitrag("Song", 200, "", "Album", "Genre"));
    }

    [Fact]
    public void Moderation_Kategorie_Is_Moderation_AndKeepsText()
    {
        var m = new Moderation("Intro", 30, "Guten Morgen!");
        m.Kategorie.ShouldBe("Moderation");
        m.Sprechertext.ShouldBe("Guten Morgen!");
        m.ShouldBeAssignableTo<Beitrag>();
    }

    [Fact]
    public void Moderation_EmptyText_Throws()
    {
        Should.Throw<InvalidNameException>(() => new Moderation("Intro", 30, " "));
    }

    [Fact]
    public void Werbung_EmptyCustomer_Throws()
    {
        Should.Throw<InvalidNameException>(() =>
            new Werbung("Ad", 30, "", 1000m));
    }

    [Fact]
    public void Werbung_NegativeBudget_Throws()
    {
        Should.Throw<InvalidBudgetException>(() =>
            new Werbung("Ad", 30, "Customer", -1m));
    }

    [Fact]
    public void Eilmeldung_IsNotWerbung_AndKeepsUrgencyAndRegion()
    {
        var e = new Eilmeldung("Breaking", 45, 2, "Wien");
        e.ShouldNotBeAssignableTo<Werbung>();
        e.Dringlichkeit.ShouldBe(2);
        e.BetroffeneRegion.ShouldBe("Wien");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Eilmeldung_UrgencyOutOfRange_Throws(int urgency)
    {
        Should.Throw<InvalidUrgencyException>(() => new Eilmeldung("Breaking", 45, urgency, "Wien"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Eilmeldung_UrgencyBoundaries_AreAccepted(int urgency)
    {
        new Eilmeldung("Breaking", 45, urgency, "Wien").Dringlichkeit.ShouldBe(urgency);
    }

    [Fact]
    public void Eilmeldung_EmptyRegion_Throws()
    {
        Should.Throw<InvalidNameException>(() => new Eilmeldung("Breaking", 45, 3, ""));
    }

    [Fact]
    public void Beitrag_ZeroDuration_Throws()
    {
        Should.Throw<InvalidDurationException>(() =>
            new Musikbeitrag("Song", 0, "Artist", "Album", "Genre"));
    }

    [Fact]
    public void DetailBeschreibung_IncludesKeyData()
    {
        var m = new Musikbeitrag("Song", 200, "Artist", "Album", "Genre");
        m.DetailBeschreibung().ShouldContain("Artist");
        m.DetailBeschreibung().ShouldContain("Album");
    }
}

public class SendungScheduleTests
{
    private static Sendung NewShow(int maxDauerMinutes = 60)
        => new("TestShow", "TestMod", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(maxDauerMinutes));

    [Fact]
    public void Ctor_ValidInputs_CreatesShow()
    {
        var s = NewShow();
        s.Name.ShouldBe("TestShow");
        s.Moderator.ShouldBe("TestMod");
        s.MaxDauer.ShouldBe(TimeSpan.FromMinutes(60));
        s.StartUhrzeit.ShouldBe(new TimeSpan(8, 0, 0));
    }

    [Fact]
    public void Ctor_EmptyName_Throws()
    {
        Should.Throw<InvalidNameException>(() =>
            new Sendung("", "Mod", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60)));
    }

    [Fact]
    public void Ctor_ZeroMaxDuration_Throws()
    {
        Should.Throw<InvalidDurationException>(() =>
            new Sendung("X", "M", new TimeSpan(8, 0, 0), TimeSpan.Zero));
    }

    [Fact]
    public void GeplanteGesamtzeit_EmptyShow_IsZero()
    {
        var s = NewShow();
        s.GeplanteGesamtzeit.ShouldBe(TimeSpan.Zero);
        s.VerbleibendeRestzeit.ShouldBe(TimeSpan.FromMinutes(60));
    }

    [Fact]
    public void BelegeNeuePositionEin_AddsToSchedule()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 200, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        s.AnzahlBeitraege.ShouldBe(1);
        s.GeplanteGesamtzeit.ShouldBe(TimeSpan.FromSeconds(200));
    }

    [Fact]
    public void BelegeNeuePositionEin_ExceedsLimit_Throws()
    {
        var s = NewShow(1);
        var m1 = new Musikbeitrag("Song1", 50, "A", "Al", "G");
        var m2 = new Musikbeitrag("Song2", 30, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        Should.Throw<ShowOverCapacityException>(() => s.BelegeNeuePositionEin(m2));
    }

    [Fact]
    public void StartzeitVon_ComputesOffsetFromStart()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("Song1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("Song2", 300, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);

        s.StartzeitVon(m1).ShouldBe(new TimeSpan(8, 0, 0));
        s.StartzeitVon(m2).ShouldBe(new TimeSpan(8, 3, 20));
    }

    [Fact]
    public void StartzeitVon_ContributionNotInShow_Throws()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 200, "A", "Al", "G");
        Should.Throw<BeitragMembershipException>(() => s.StartzeitVon(m));
    }

    [Fact]
    public void EntferneBeitrag_RemovesFromSchedule()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 200, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        s.EntferneBeitrag(m);
        s.AnzahlBeitraege.ShouldBe(0);
    }

    [Fact]
    public void KuerzeBeitrag_AdjustsDuration()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 600, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        s.KuerzeBeitrag(m, 200);
        m.DauerSekunden.ShouldBe(200);
        s.GeplanteGesamtzeit.ShouldBe(TimeSpan.FromSeconds(200));
    }

    [Fact]
    public void KuerzeBeitrag_LongerOrSameDuration_Throws()
    {
        var s = NewShow(1);
        var m = new Musikbeitrag("Song", 50, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);

        Should.Throw<InvalidDurationException>(() => s.KuerzeBeitrag(m, 500));
        Should.Throw<InvalidDurationException>(() => s.KuerzeBeitrag(m, 50));
        m.DauerSekunden.ShouldBe(50);
    }

    [Fact]
    public void KuerzeBeitrag_ZeroDuration_Throws()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 50, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        Should.Throw<InvalidDurationException>(() => s.KuerzeBeitrag(m, 0));
    }

    [Fact]
    public void KuerzeBeitrag_NotInShow_Throws()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 600, "A", "Al", "G");
        Should.Throw<BeitragMembershipException>(() => s.KuerzeBeitrag(m, 100));
        m.DauerSekunden.ShouldBe(600);
    }

    [Fact]
    public void EntferneBeitrag_NotInShow_Throws()
    {
        var s = NewShow();
        Should.Throw<BeitragMembershipException>(() =>
            s.EntferneBeitrag(new Musikbeitrag("Song", 60, "A", "Al", "G")));
    }

    [Fact]
    public void BelegeNeuePositionEin_SameContributionTwice_Throws()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 60, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        Should.Throw<BeitragMembershipException>(() => s.BelegeNeuePositionEin(m));
        s.AnzahlBeitraege.ShouldBe(1);
    }

    [Fact]
    public void Beitraege_AreReadOnly()
    {
        var s = NewShow();
        s.BelegeNeuePositionEin(new Musikbeitrag("Song", 60, "A", "Al", "G"));
        s.Beitraege.ShouldNotBeAssignableTo<System.Collections.Generic.List<Beitrag>>();
    }

    [Fact]
    public void AktuellerBeitrag_StartsAtFirst_AndMovesOn()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);

        s.AktuellerBeitrag.ShouldBe(m1);
        s.WechsleZumNaechstenBeitrag();
        s.AktuellerBeitrag.ShouldBe(m2);
        s.WechsleZumNaechstenBeitrag();
        s.AktuellerBeitrag.ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => s.WechsleZumNaechstenBeitrag());
    }

    [Fact]
    public void EntferneBeitrag_AlreadyPlayed_KeepsCurrentContribution()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        s.WechsleZumNaechstenBeitrag();

        s.EntferneBeitrag(m1);

        s.AktuellerBeitrag.ShouldBe(m2);
    }

    [Fact]
    public void Startzeiten_RecomputeAfterKuerzen()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        s.KuerzeBeitrag(m1, 50);

        s.StartzeitVon(m1).ShouldBe(new TimeSpan(8, 0, 0));
        s.StartzeitVon(m2).ShouldBe(new TimeSpan(8, 0, 50));
    }

    [Fact]
    public void Startzeiten_RecomputeAfterRemove()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        var m3 = new Musikbeitrag("S3", 100, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        s.BelegeNeuePositionEin(m3);
        s.EntferneBeitrag(m2);

        s.StartzeitVon(m1).ShouldBe(new TimeSpan(8, 0, 0));
        s.StartzeitVon(m3).ShouldBe(new TimeSpan(8, 3, 20));
    }
}

public class EilmeldungTests
{
    private static Sendung NewShow(int maxDauerMinutes = 60)
        => new("TestShow", "TestMod", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(maxDauerMinutes));

    [Fact]
    public void FuegeEilmeldungHinzu_PrependsToSchedule()
    {
        var s = NewShow();
        var m = new Musikbeitrag("Song", 200, "A", "Al", "G");
        s.BelegeNeuePositionEin(m);
        var e = new Eilmeldung("Breaking", 45, 3, "Wien");

        var ergebnis = s.FuegeEilmeldungHinzu(e);

        s.AnzahlBeitraege.ShouldBe(2);
        s.Beitraege[0].ShouldBe(e);
        ergebnis.ZeitlimitUeberschritten.ShouldBeFalse();
        ergebnis.Warnung.ShouldBeNull();
    }

    [Fact]
    public void FuegeEilmeldungHinzu_InsertsBeforeCurrentContribution_NotAtFront()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        var m3 = new Musikbeitrag("S3", 100, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        s.BelegeNeuePositionEin(m3);
        s.WechsleZumNaechstenBeitrag(); // m1 played, m2 is current

        var e = new Eilmeldung("Breaking", 60, 5, "Wien");
        s.FuegeEilmeldungHinzu(e);

        s.Beitraege.ShouldBe(new Beitrag[] { m1, e, m2, m3 });
        s.AktuellerBeitrag.ShouldBe(e);
        s.StartzeitVon(e).ShouldBe(new TimeSpan(8, 3, 20));
        s.StartzeitVon(m2).ShouldBe(new TimeSpan(8, 4, 20));
    }

    [Fact]
    public void FuegeEilmeldungHinzu_ExceedsLimit_InsertsAnyway_AndWarnsModerator()
    {
        var s = NewShow(1);
        var m1 = new Musikbeitrag("S1", 30, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 25, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        var e = new Eilmeldung("Breaking", 10, 3, "Wien");

        var ergebnis = s.FuegeEilmeldungHinzu(e);

        s.AnzahlBeitraege.ShouldBe(3);
        s.Beitraege[0].ShouldBe(e);
        ergebnis.ZeitlimitUeberschritten.ShouldBeTrue();
        ergebnis.Ueberziehung.ShouldBe(TimeSpan.FromSeconds(5));
        ergebnis.Warnung.ShouldNotBeNull();
        ergebnis.Warnung!.ShouldContain("TestMod");
        s.VerbleibendeRestzeit.ShouldBe(TimeSpan.FromSeconds(-5));
    }

    [Fact]
    public void FuegeEilmeldungHinzu_SameEilmeldungTwice_Throws_AndChangesNothing()
    {
        var s = NewShow();
        var e = new Eilmeldung("Breaking", 10, 3, "Wien");
        s.FuegeEilmeldungHinzu(e);
        Should.Throw<BeitragMembershipException>(() => s.FuegeEilmeldungHinzu(e));
        s.AnzahlBeitraege.ShouldBe(1);
    }

    [Fact]
    public void FuegeEilmeldungHinzu_ShiftsOthersBack()
    {
        var s = NewShow();
        var m1 = new Musikbeitrag("S1", 200, "A", "Al", "G");
        var m2 = new Musikbeitrag("S2", 300, "A", "Al", "G");
        s.BelegeNeuePositionEin(m1);
        s.BelegeNeuePositionEin(m2);
        var e = new Eilmeldung("Breaking", 60, 3, "Wien");

        s.FuegeEilmeldungHinzu(e);

        s.StartzeitVon(e).ShouldBe(new TimeSpan(8, 0, 0));
        s.StartzeitVon(m1).ShouldBe(new TimeSpan(8, 1, 0));
        s.StartzeitVon(m2).ShouldBe(new TimeSpan(8, 4, 20));
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_SplitsFirstContribution()
    {
        var s = NewShow();
        var songA = new Musikbeitrag("A", 480, "X", "Y", "Z");
        var songB = new Musikbeitrag("B", 300, "X", "Y", "Z");
        s.BelegeNeuePositionEin(songA);
        s.BelegeNeuePositionEin(songB);

        s.FuegeEilmeldungMittenImProgrammHinzu(
            new Eilmeldung("Breaking", 60, 3, "Wien"),
            bereitsAusgestrahlteSekunden: 180);

        s.AnzahlBeitraege.ShouldBe(4);
        s.AktuellerBeitrag!.Kategorie.ShouldBe("Eilmeldung");
        s.Beitraege[0].ShouldBe(songA);
        songA.DauerSekunden.ShouldBe(180);
        s.Beitraege[1].Kategorie.ShouldBe("Eilmeldung");
        s.Beitraege[2].Titel.ShouldContain("(Fortsetzung)");
        s.Beitraege[2].DauerSekunden.ShouldBe(300);
        s.Beitraege[3].ShouldBe(songB);
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_PreservesAllOtherContributions()
    {
        var s = NewShow();
        var a = new Musikbeitrag("A", 480, "X", "Y", "Z");
        var b = new Musikbeitrag("B", 300, "X", "Y", "Z");
        var c = new Musikbeitrag("C", 200, "X", "Y", "Z");
        s.BelegeNeuePositionEin(a);
        s.BelegeNeuePositionEin(b);
        s.BelegeNeuePositionEin(c);

        s.FuegeEilmeldungMittenImProgrammHinzu(
            new Eilmeldung("Breaking", 60, 3, "Wien"),
            bereitsAusgestrahlteSekunden: 100);

        s.AnzahlBeitraege.ShouldBe(5);
        s.Beitraege[4].ShouldBe(c);
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_FullyPlayed_InsertsAfterCurrent_DoesNotSplit()
    {
        var s = NewShow();
        var a = new Musikbeitrag("A", 200, "X", "Y", "Z");
        var b = new Musikbeitrag("B", 200, "X", "Y", "Z");
        s.BelegeNeuePositionEin(a);
        s.BelegeNeuePositionEin(b);

        s.FuegeEilmeldungMittenImProgrammHinzu(
            new Eilmeldung("Breaking", 60, 3, "Wien"),
            bereitsAusgestrahlteSekunden: 250);

        s.AnzahlBeitraege.ShouldBe(3);
        s.Beitraege[0].ShouldBe(a);
        a.DauerSekunden.ShouldBe(200);
        s.Beitraege[1].Kategorie.ShouldBe("Eilmeldung");
        s.Beitraege[2].ShouldBe(b);
        s.AktuellerBeitrag.ShouldBe(s.Beitraege[1]);
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_ExceedsLimit_InsertsAnyway_AndWarns()
    {
        var s = NewShow(1);
        var a = new Musikbeitrag("A", 55, "X", "Y", "Z");
        s.BelegeNeuePositionEin(a);

        var ergebnis = s.FuegeEilmeldungMittenImProgrammHinzu(
            new Eilmeldung("Breaking", 30, 3, "Wien"),
            bereitsAusgestrahlteSekunden: 20);

        // Split happened and the show is consistent: 20 s + 30 s + 35 s = 85 s.
        s.AnzahlBeitraege.ShouldBe(3);
        s.GeplanteGesamtzeit.ShouldBe(TimeSpan.FromSeconds(85));
        ergebnis.ZeitlimitUeberschritten.ShouldBeTrue();
        ergebnis.Ueberziehung.ShouldBe(TimeSpan.FromSeconds(25));
        ergebnis.Warnung.ShouldNotBeNull();
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_NegativeElapsed_Throws_AndChangesNothing()
    {
        var s = NewShow();
        var a = new Musikbeitrag("A", 200, "X", "Y", "Z");
        s.BelegeNeuePositionEin(a);

        Should.Throw<InvalidDurationException>(() =>
            s.FuegeEilmeldungMittenImProgrammHinzu(new Eilmeldung("Breaking", 30, 3, "Wien"), -1));

        s.AnzahlBeitraege.ShouldBe(1);
        a.DauerSekunden.ShouldBe(200);
    }

    [Fact]
    public void FuegeEilmeldungMittenImProgrammHinzu_SplitsModeration_KeepsType()
    {
        var s = NewShow();
        var mod = new Moderation("Intro", 120, "Hallo!");
        s.BelegeNeuePositionEin(mod);

        s.FuegeEilmeldungMittenImProgrammHinzu(new Eilmeldung("Breaking", 30, 3, "Wien"), 60);

        s.Beitraege[2].ShouldBeOfType<Moderation>();
        s.Beitraege[2].DauerSekunden.ShouldBe(60);
    }
}

public class RadioProgramTests
{
    [Fact]
    public void Ctor_EmptyProgram()
    {
        var p = new RadioProgram();
        p.SendungCount.ShouldBe(0);
    }

    [Fact]
    public void FuegeSendungHinzu_Appends()
    {
        var p = new RadioProgram();
        var s = new Sendung("Show1", "Mod", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60));
        p.FuegeSendungHinzu(s);
        p.SendungCount.ShouldBe(1);
    }

    [Fact]
    public void FuegeSendungHinzu_KeepsShowsOrderedByStartTime()
    {
        var p = new RadioProgram();
        var evening = new Sendung("Evening", "Bob", new TimeSpan(20, 0, 0), TimeSpan.FromMinutes(60));
        var morning = new Sendung("Morning", "Anna", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60));
        var noon = new Sendung("Noon", "Cem", new TimeSpan(12, 0, 0), TimeSpan.FromMinutes(60));
        p.FuegeSendungHinzu(evening);
        p.FuegeSendungHinzu(morning);
        p.FuegeSendungHinzu(noon);

        p.Sendungen.ShouldBe(new[] { morning, noon, evening });
        p.Sendungen.ShouldNotBeAssignableTo<System.Collections.Generic.List<Sendung>>();
    }

    [Fact]
    public void FuegeSendungHinzu_Overlap_Throws()
    {
        var p = new RadioProgram();
        p.FuegeSendungHinzu(new Sendung("A", "M", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60)));

        Should.Throw<ShowOverlapException>(() =>
            p.FuegeSendungHinzu(new Sendung("B", "M", new TimeSpan(8, 30, 0), TimeSpan.FromMinutes(60))));
        Should.Throw<ShowOverlapException>(() =>
            p.FuegeSendungHinzu(new Sendung("C", "M", new TimeSpan(7, 30, 0), TimeSpan.FromMinutes(31))));
        p.SendungCount.ShouldBe(1);
    }

    [Fact]
    public void FuegeSendungHinzu_BackToBackShows_AreAllowed()
    {
        var p = new RadioProgram();
        p.FuegeSendungHinzu(new Sendung("A", "M", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60)));
        p.FuegeSendungHinzu(new Sendung("B", "M", new TimeSpan(9, 0, 0), TimeSpan.FromMinutes(60)));
        p.SendungCount.ShouldBe(2);
    }

    [Fact]
    public void FindeSendungZuUhrzeit_ReturnsShowAtThatTime()
    {
        var p = new RadioProgram();
        p.FuegeSendungHinzu(new Sendung("Morning", "Anna", new TimeSpan(8, 0, 0), TimeSpan.FromMinutes(60)));
        p.FuegeSendungHinzu(new Sendung("Evening", "Bob", new TimeSpan(20, 0, 0), TimeSpan.FromMinutes(60)));

        p.FindeSendungZuUhrzeit(new TimeSpan(8, 30, 0))!.Name.ShouldBe("Morning");
        p.FindeSendungZuUhrzeit(new TimeSpan(20, 15, 0))!.Name.ShouldBe("Evening");
        p.FindeSendungZuUhrzeit(new TimeSpan(15, 0, 0)).ShouldBeNull();
    }
}

public class RadioSenderTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesStation()
    {
        var rs = new RadioSender("Cloud-FM", "100.4 MHz");
        rs.Name.ShouldBe("Cloud-FM");
        rs.UebertragungsFrequenz.ShouldBe("100.4 MHz");
        rs.SenderId.ShouldBeGreaterThan(0);
        rs.Program.ShouldNotBeNull();
    }

    [Fact]
    public void Ctor_EmptyName_Throws()
    {
        Should.Throw<InvalidNameException>(() => new RadioSender("", "100.0"));
    }

    [Fact]
    public void Ctor_EmptyFrequency_Throws()
    {
        Should.Throw<InvalidNameException>(() => new RadioSender("FM", ""));
    }

    [Fact]
    public void DistinctStations_HaveDistinctIds()
    {
        var a = new RadioSender("A", "100");
        var b = new RadioSender("B", "101");
        a.SenderId.ShouldNotBe(b.SenderId);
    }
}