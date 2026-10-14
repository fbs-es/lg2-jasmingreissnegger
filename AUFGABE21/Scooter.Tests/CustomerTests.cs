using System;
using System.Linq;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class CustomerTests
{
    private static Customer NewCustomer(decimal balance = 20m)
        => new("Anna Mueller", "anna@example.com", balance);

    private static Scooter NewScooter() => new();

    private static DateTime Now => new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ctor_ValidInputs_CreatesCustomer()
    {
        var c = new Customer("Anna Mueller", "anna@example.com", 10m);

        c.Name.ShouldBe("Anna Mueller");
        c.Email.ShouldBe("anna@example.com");
        c.Balance.ShouldBe(10m);
        c.IsBlocked.ShouldBeFalse();
        c.HasOpenDunning.ShouldBeFalse();
        c.CurrentRental.ShouldBeNull();
        c.SentDunningEmailCount.ShouldBe(0);
        c.RentalCount.ShouldBe(0);
        c.CustomerId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Ctor_TrimsWhitespaceInNameAndEmail()
    {
        var c = new Customer("  Bob  ", "  bob@x.com  ", 10m);

        c.Name.ShouldBe("Bob");
        c.Email.ShouldBe("bob@x.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Customer(name!, "a@b.com", 10m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Ctor_InvalidEmail_Throws(string? email)
    {
        Should.Throw<InvalidEmailException>(() => new Customer("Bob", email!, 10m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(9.99)]
    public void Ctor_BalanceBelowMinimum_Throws(decimal balance)
    {
        Should.Throw<InvalidBalanceException>(() => new Customer("Bob", "bob@x.com", balance));
    }

    [Fact]
    public void Ctor_BalanceExactlyMinimum_IsAccepted()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);

        c.Balance.ShouldBe(10.00m);
    }

    [Fact]
    public void Ctor_DistinctCustomers_HaveDistinctIds()
    {
        var a = new Customer("A", "a@x.com", 10m);
        var b = new Customer("B", "b@x.com", 10m);

        a.CustomerId.ShouldNotBe(b.CustomerId);
    }

    [Fact]
    public void StartRental_HappyPath_AssignsAndUnlocksScooter()
    {
        var c = NewCustomer();
        var s = NewScooter();

        c.StartRental(s, Now);

        c.CurrentRental.ShouldBeSameAs(s);
        s.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void StartRental_CustomerAlreadyRenting_Throws()
    {
        var c = NewCustomer();
        var s1 = NewScooter();
        var s2 = NewScooter();
        c.StartRental(s1, Now);

        Should.Throw<CustomerAlreadyRentingException>(() => c.StartRental(s2, Now));
    }

    [Fact]
    public void StartRental_ScooterAlreadyUnlocked_Throws()
    {
        var c = NewCustomer();
        var s = NewScooter();
        var c2 = NewCustomer();
        c2.StartRental(s, Now);
        s.IsLocked.ShouldBeFalse();

        Should.Throw<ScooterNotAvailableException>(() => c.StartRental(s, Now));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(10)]
    [InlineData(0)]
    public void StartRental_BatteryAtOrBelowThreshold_Throws(int battery)
    {
        var c = NewCustomer();
        var s = NewScooter();
        var c2 = NewCustomer(100m);
        c2.StartRental(s, Now);
        c2.EndRental(Now.AddMinutes(100 - battery));

        Should.Throw<InsufficientBatteryException>(() => c.StartRental(s, Now));
    }

    [Fact]
    public void StartRental_BalanceBelowMinimum_Throws()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(46));

        c.Balance.ShouldBe(0.80m);
        var s2 = NewScooter();
        Should.Throw<InsufficientBalanceException>(() => c.StartRental(s2, Now));
    }

    [Fact]
    public void StartRental_BlockedCustomer_Throws()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(60));

        c.IsBlocked.ShouldBeTrue();
        var s2 = NewScooter();
        Should.Throw<CustomerBlockedException>(() => c.StartRental(s2, Now));
    }

    [Fact]
    public void StartRental_FailedStart_LeavesStateUnchanged()
    {
        var c = NewCustomer(10m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(46));

        var s2 = NewScooter();
        Should.Throw<InsufficientBalanceException>(() => c.StartRental(s2, Now));

        c.CurrentRental.ShouldBeNull();
        s2.IsLocked.ShouldBeTrue();
        s2.BatteryLevel.ShouldBe(100);
    }

    [Fact]
    public void EndRental_HappyPath_DeductsCostDrainsBatteryAndLocksScooter()
    {
        var c = NewCustomer(20m);
        var s = NewScooter();
        c.StartRental(s, Now);

        c.EndRental(Now.AddMinutes(30));

        c.Balance.ShouldBe(20m - 6m);
        s.BatteryLevel.ShouldBe(70);
        s.IsLocked.ShouldBeTrue();
        c.CurrentRental.ShouldBeNull();
    }

    [Fact]
    public void EndRental_NoActiveRental_Throws()
    {
        var c = NewCustomer();
        Should.Throw<NoActiveRentalException>(() => c.EndRental(Now.AddMinutes(10)));
    }

    [Fact]
    public void EndRental_FailedEnd_LeavesStateUnchanged()
    {
        var c = NewCustomer();
        var s = NewScooter();
        c.StartRental(s, Now);
        decimal balanceBefore = c.Balance;
        int batteryBefore = s.BatteryLevel;

        Should.Throw<InvalidRentalDurationException>(() => c.EndRental(Now.AddSeconds(-1)));

        c.Balance.ShouldBe(balanceBefore);
        s.BatteryLevel.ShouldBe(batteryBefore);
        c.CurrentRental.ShouldBe(s);
        s.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void EndRental_BalanceGoesNegative_BlocksAndSendsDunningEmail()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);

        c.EndRental(Now.AddMinutes(60));

        c.Balance.ShouldBe(-2.00m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
        c.SentDunningEmailCount.ShouldBe(1);
        c.GetLastDunningEmail()!.ShouldContain("bob@x.com");
    }

    [Fact]
    public void StartRental_AfterBecomingBlocked_IsRejected()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(60));

        var s2 = NewScooter();
        Should.Throw<CustomerBlockedException>(() => c.StartRental(s2, Now));
    }

    [Fact]
    public void TopUp_PositiveAmount_IncreasesBalance()
    {
        var c = NewCustomer();
        c.TopUp(50m);
        c.Balance.ShouldBe(70m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TopUp_NonPositiveAmount_Throws(decimal amount)
    {
        var c = NewCustomer();
        Should.Throw<InvalidBalanceException>(() => c.TopUp(amount));
    }

    [Fact]
    public void TopUp_BringsBalanceAboveZero_UnblocksAndResetsDunning()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(60));
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();

        c.TopUp(5m);

        c.Balance.ShouldBe(3m);
        c.IsBlocked.ShouldBeFalse();
        c.HasOpenDunning.ShouldBeFalse();
    }

    [Fact]
    public void TopUp_BalanceStillNegativeOrZero_DoesNotUnblock()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(60));

        c.TopUp(1m);

        c.Balance.ShouldBe(-1m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
    }

    [Fact]
    public void StartRental_AfterUnblockViaTopUp_StartsSuccessfully()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s1 = NewScooter();
        c.StartRental(s1, Now);
        c.EndRental(Now.AddMinutes(60));

        c.TopUp(5m);

        var s2 = NewScooter();
        c.StartRental(s2, Now);

        c.CurrentRental.ShouldBeSameAs(s2);
        s2.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void ReadingProperties_DoesNotMutate()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(60));

        for (var i = 0; i < 10; i++)
        {
            _ = c.CustomerId;
            _ = c.Name;
            _ = c.Email;
            _ = c.Balance;
            _ = c.IsBlocked;
            _ = c.HasOpenDunning;
            _ = c.CurrentRental;
            _ = c.SentDunningEmailCount;
            _ = c.RentalCount;
            _ = c.WasRented(s);
        }

        c.Balance.ShouldBe(-2m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
        c.SentDunningEmailCount.ShouldBe(1);
        c.RentalCount.ShouldBe(1);
    }

    [Fact]
    public void EndRental_VeryLongRide_BatteryClampedAtZero()
    {
        var c = NewCustomer(1000m);
        var s = NewScooter();
        c.StartRental(s, Now);

        c.EndRental(Now.AddMinutes(500));

        s.BatteryLevel.ShouldBe(0);
        s.IsLocked.ShouldBeTrue();
    }

    [Fact]
    public void EndRental_RecordsHistoryEntry_WithRealTimestamps()
    {
        var c = NewCustomer(100m);
        var s = NewScooter();
        DateTime start = Now;
        DateTime end = start.AddMinutes(45);

        c.StartRental(s, start);
        c.EndRental(end);

        c.RentalCount.ShouldBe(1);
        s.RentalCount.ShouldBe(1);
        s.WasRentedBy(c).ShouldBeTrue();
        c.WasRented(s).ShouldBeTrue();

        var customerEntry = c.GetLastEntry();
        customerEntry.ShouldNotBeNull();
        customerEntry!.StartTime.ShouldBe(start);
        customerEntry.EndTime.ShouldBe(end);
        customerEntry.Duration.ShouldBe(TimeSpan.FromMinutes(45));
        customerEntry.Cost.ShouldBe(9m);
        customerEntry.Customer.ShouldBeSameAs(c);
        customerEntry.Scooter.ShouldBeSameAs(s);

        var scooterEntry = s.GetLastEntry();
        scooterEntry.ShouldNotBeNull();
        scooterEntry!.ShouldBeSameAs(customerEntry);
    }

    [Fact]
    public void EndRental_MultipleRentals_BuildCompleteHistory()
    {
        var c = NewCustomer(1000m);
        var s1 = NewScooter();
        var s2 = NewScooter();
        var s3 = NewScooter();

        c.StartRental(s1, Now);
        c.EndRental(Now.AddMinutes(10));

        c.StartRental(s2, Now.AddHours(1));
        c.EndRental(Now.AddHours(1).AddMinutes(20));

        c.StartRental(s3, Now.AddHours(2));
        c.EndRental(Now.AddHours(2).AddMinutes(5));

        c.RentalCount.ShouldBe(3);
        s1.RentalCount.ShouldBe(1);
        s2.RentalCount.ShouldBe(1);
        s3.RentalCount.ShouldBe(1);
        c.WasRented(s1).ShouldBeTrue();
        c.WasRented(s2).ShouldBeTrue();
        c.WasRented(s3).ShouldBeTrue();
        s1.WasRentedBy(c).ShouldBeTrue();
        s2.WasRentedBy(c).ShouldBeTrue();
        s3.WasRentedBy(c).ShouldBeTrue();
    }

    [Fact]
    public void EndRental_EndBeforeStart_Throws()
    {
        var c = NewCustomer();
        var s = NewScooter();
        c.StartRental(s, Now);

        Should.Throw<InvalidRentalDurationException>(() => c.EndRental(Now.AddSeconds(-1)));
    }

    [Fact]
    public void EndRental_ZeroDuration_RoundsUpToOneMinute()
    {
        var c = NewCustomer(100m);
        var s = NewScooter();

        c.StartRental(s, Now);
        c.EndRental(Now);

        c.Balance.ShouldBe(100m - 0.20m);
        c.GetLastEntry()!.Cost.ShouldBe(0.20m);
    }

    [Fact]
    public void StartRental_NoTimestamp_UsesUtcNow()
    {
        var c = NewCustomer();
        var s = NewScooter();

        c.StartRental(s);

        c.CurrentRental.ShouldBeSameAs(s);
        s.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void Scooter_TotalRevenue_SumsAllRentalCosts()
    {
        var c = NewCustomer(100m);
        var s = NewScooter();

        c.StartRental(s, Now);
        c.EndRental(Now.AddMinutes(15));

        s.TotalRevenue.ShouldBe(3.00m);

        c.StartRental(s, Now.AddHours(1));
        c.EndRental(Now.AddHours(1).AddMinutes(10));

        s.TotalRevenue.ShouldBe(3.00m + 2.00m);
    }
}