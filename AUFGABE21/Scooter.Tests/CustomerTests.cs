using System.Linq;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class CustomerTests
{
    private static Customer NewCustomer(decimal balance = 20m)
        => new("Anna Mueller", "anna@example.com", balance);

    private static Scooter NewScooter() => new();

    // ---- Constructor --------------------------------------------------------

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
        c.SentDunningEmails.ShouldBeEmpty();
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

    // ---- StartRental --------------------------------------------------------

    [Fact]
    public void StartRental_HappyPath_AssignsAndUnlocksScooter()
    {
        var c = NewCustomer();
        var s = NewScooter();

        c.StartRental(s);

        c.CurrentRental.ShouldBeSameAs(s);
        s.IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void StartRental_CustomerAlreadyRenting_Throws()
    {
        var c = NewCustomer();
        var s1 = NewScooter();
        var s2 = NewScooter();
        c.StartRental(s1);

        Should.Throw<CustomerAlreadyRentingException>(() => c.StartRental(s2));
    }

    [Fact]
    public void StartRental_ScooterAlreadyUnlocked_Throws()
    {
        var c = NewCustomer();
        var s = NewScooter();

        // Reflection-free setup: simulate "already unlocked" via a private mechanism.
        // We construct a second customer, start a rental on the same scooter (impossible because scooter starts locked).
        // Instead, exercise the path indirectly: Scooter state changes only via Customer methods,
        // so the only way to reach IsLocked=false is through StartRental on someone else.
        // Use a second customer + first customer already on the same scooter -> cannot re-unlock.
        // For a direct test, we need a second customer to first lock-unlock it.
        var c2 = NewCustomer();
        c2.StartRental(s);
        s.IsLocked.ShouldBeFalse();

        // Now customer c tries to start rental on the already-unlocked scooter.
        Should.Throw<ScooterNotAvailableException>(() => c.StartRental(s));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(10)]
    [InlineData(0)]
    public void StartRental_BatteryAtOrBelowThreshold_Throws(int battery)
    {
        var c = NewCustomer();
        var s = NewScooter();

        // Drive battery down via a rental on a second customer who can afford it.
        var c2 = NewCustomer(100m);
        c2.StartRental(s);
        c2.EndRental(100 - battery);

        Should.Throw<InsufficientBatteryException>(() => c.StartRental(s));
    }

    [Fact]
    public void StartRental_BalanceBelowMinimum_Throws()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();

        // Burn balance to exactly 0.99 via rentals.
        c.StartRental(s);
        // 10.00 - minutes*0.20 = 0.99  =>  minutes = (10-0.99)/0.20 = 45.05 -> use 45 -> balance = 10 - 9 = 1.00  (allowed)
        // 46 minutes -> balance = 10 - 9.20 = 0.80 (still > 0 but below 1.00 after)
        // After EndRental with 46 minutes, balance = 0.80 -> try StartRental again on a fresh scooter
        c.EndRental(46);

        c.Balance.ShouldBe(0.80m);
        var s2 = NewScooter();
        Should.Throw<InsufficientBalanceException>(() => c.StartRental(s2));
    }

    [Fact]
    public void StartRental_BlockedCustomer_Throws()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s);
        c.EndRental(60); // 10 - 12 = -2.00 -> blocked

        c.IsBlocked.ShouldBeTrue();
        var s2 = NewScooter();
        Should.Throw<CustomerBlockedException>(() => c.StartRental(s2));
    }

    [Fact]
    public void StartRental_FailedStart_LeavesStateUnchanged()
    {
        var c = NewCustomer(10m);
        var s = NewScooter();

        // Drive customer balance below MinStartBalance via the rental flow.
        c.StartRental(s);
        c.EndRental(46); // 10 - 9.20 = 0.80

        var s2 = NewScooter();
        Should.Throw<InsufficientBalanceException>(() => c.StartRental(s2));

        c.CurrentRental.ShouldBeNull();
        s2.IsLocked.ShouldBeTrue();
        s2.BatteryLevel.ShouldBe(100);
    }

    // ---- EndRental ----------------------------------------------------------

    [Fact]
    public void EndRental_HappyPath_DeductsCostDrainsBatteryAndLocksScooter()
    {
        var c = NewCustomer(20m);
        var s = NewScooter();
        c.StartRental(s);

        c.EndRental(30);

        c.Balance.ShouldBe(20m - 6m);   // 0.20 * 30 = 6.00
        s.BatteryLevel.ShouldBe(70);     // 100 - 30
        s.IsLocked.ShouldBeTrue();
        c.CurrentRental.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void EndRental_NonPositiveDuration_Throws(int minutes)
    {
        var c = NewCustomer();
        Should.Throw<InvalidRentalDurationException>(() => c.EndRental(minutes));
    }

    [Fact]
    public void EndRental_NoActiveRental_Throws()
    {
        var c = NewCustomer();
        Should.Throw<NoActiveRentalException>(() => c.EndRental(10));
    }

    [Fact]
    public void EndRental_FailedEnd_LeavesStateUnchanged()
    {
        var c = NewCustomer();
        var s = NewScooter();
        c.StartRental(s);
        decimal balanceBefore = c.Balance;
        int batteryBefore = s.BatteryLevel;

        Should.Throw<InvalidRentalDurationException>(() => c.EndRental(0));

        c.Balance.ShouldBe(balanceBefore);
        s.BatteryLevel.ShouldBe(batteryBefore);
        c.CurrentRental.ShouldBe(s);
        s.IsLocked.ShouldBeFalse();
    }

    // ---- Dunning on negative balance ---------------------------------------

    [Fact]
    public void EndRental_BalanceGoesNegative_BlocksAndSendsDunningEmail()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s);

        c.EndRental(60); // cost = 12.00, balance = -2.00

        c.Balance.ShouldBe(-2.00m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
        c.SentDunningEmails.Count.ShouldBe(1);
        c.SentDunningEmails[0].ShouldContain("bob@x.com");
    }

    [Fact]
    public void StartRental_AfterBecomingBlocked_IsRejected()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s);
        c.EndRental(60); // balance -2

        var s2 = NewScooter();
        Should.Throw<CustomerBlockedException>(() => c.StartRental(s2));
    }

    // ---- TopUp --------------------------------------------------------------

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
        c.StartRental(s);
        c.EndRental(60); // balance -2, blocked + dunning
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();

        c.TopUp(5m); // balance = 3.00

        c.Balance.ShouldBe(3m);
        c.IsBlocked.ShouldBeFalse();
        c.HasOpenDunning.ShouldBeFalse();
    }

    [Fact]
    public void TopUp_BalanceStillNegativeOrZero_DoesNotUnblock()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s);
        c.EndRental(60); // balance -2

        c.TopUp(1m); // balance = -1.00

        c.Balance.ShouldBe(-1m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
    }

    [Fact]
    public void StartRental_AfterUnblockViaTopUp_StartsSuccessfully()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s1 = NewScooter();
        c.StartRental(s1);
        c.EndRental(60); // -2

        c.TopUp(5m); // 3.00, unblocked

        var s2 = NewScooter();
        c.StartRental(s2);

        c.CurrentRental.ShouldBeSameAs(s2);
        s2.IsLocked.ShouldBeFalse();
    }

    // ---- Reading does not mutate -------------------------------------------

    [Fact]
    public void ReadingProperties_DoesNotMutate()
    {
        var c = new Customer("Bob", "bob@x.com", 10.00m);
        var s = NewScooter();
        c.StartRental(s);
        c.EndRental(60); // -2, blocked

        for (var i = 0; i < 10; i++)
        {
            _ = c.CustomerId;
            _ = c.Name;
            _ = c.Email;
            _ = c.Balance;
            _ = c.IsBlocked;
            _ = c.HasOpenDunning;
            _ = c.CurrentRental;
            _ = c.SentDunningEmails;
        }

        c.Balance.ShouldBe(-2m);
        c.IsBlocked.ShouldBeTrue();
        c.HasOpenDunning.ShouldBeTrue();
        c.SentDunningEmails.Count.ShouldBe(1);
    }

    // ---- Invariants: battery stays in [0, 100] -----------------------------

    [Fact]
    public void EndRental_VeryLongRide_BatteryClampedAtZero()
    {
        var c = NewCustomer(1000m);
        var s = NewScooter();
        c.StartRental(s);

        c.EndRental(500); // battery would go to -400, must clamp at 0

        s.BatteryLevel.ShouldBe(0);
        s.IsLocked.ShouldBeTrue();
    }
}
