using System.Reflection;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class SparkontoTests
{
    [Fact]
    public void Ctor_ValidNameAndPositiveBalance_SetsValues()
    {
        var account = new Sparkonto("Anna Müller", 100m);

        account.AccountHolder.ShouldBe("Anna Müller");
        account.Balance.ShouldBe(100m);
        account.AccountNumber.ShouldNotBeNullOrWhiteSpace();
        account.IBAN.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Ctor_ZeroBalance_IsKept()
    {
        var account = new Sparkonto("Bob", 0m);

        account.Balance.ShouldBe(0m);
    }

    [Fact]
    public void Ctor_NegativeBalance_IsClampedToZero()
    {
        var account = new Sparkonto("Clara", -123.45m);

        account.Balance.ShouldBe(0m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Sparkonto(name!, 50m));
    }

    [Fact]
    public void Ctor_TrimsLeadingAndTrailingWhitespace()
    {
        var account = new Sparkonto("  Daniel  ", 10m);

        account.AccountHolder.ShouldBe("Daniel");
    }

    [Fact]
    public void Ctor_DistinctAccountsHaveDistinctNumbers()
    {
        var a = new Sparkonto("A", 0m);
        var b = new Sparkonto("B", 0m);
        var c = new Sparkonto("C", 0m);

        new[] { a.AccountNumber, b.AccountNumber, c.AccountNumber }.Distinct().Count().ShouldBe(3);
    }

    [Fact]
    public void Ctor_DistinctAccountsHaveDistinctIBANs()
    {
        var a = new Sparkonto("A", 0m);
        var b = new Sparkonto("B", 0m);

        a.IBAN.ShouldNotBe(b.IBAN);
    }

    [Fact]
    public void Ctor_IBANHasExpectedFormat()
    {
        var account = new Sparkonto("Erika", 0m);

        account.IBAN.ShouldStartWith("DE");
        account.IBAN.Length.ShouldBe(22);
    }

    [Fact]
    public void AccountNumber_IsStable_AfterMutations()
    {
        var account = new Sparkonto("Fritz", 0m);

        var before = account.AccountNumber;
        account.Deposit(50m);
        account.Withdraw(10m);
        account.UpdateName("Frieda");

        account.AccountNumber.ShouldBe(before);
    }

    [Fact]
    public void IBAN_IsStable_AfterNameUpdate()
    {
        var account = new Sparkonto("Greta", 0m);

        var before = account.IBAN;
        account.UpdateName("Hanna");

        account.IBAN.ShouldBe(before);
    }

    [Fact]
    public void Balance_HasNoPublicSetter()
    {
        typeof(Sparkonto).GetProperty(nameof(Sparkonto.Balance))!
            .GetSetMethod(nonPublic: false).ShouldBeNull();
    }

    [Fact]
    public void AccountHolder_HasNoPublicSetter()
    {
        typeof(Sparkonto).GetProperty(nameof(Sparkonto.AccountHolder))!
            .GetSetMethod(nonPublic: false).ShouldBeNull();
    }

    [Fact]
    public void AccountNumber_HasNoSetter()
    {
        typeof(Sparkonto).GetProperty(nameof(Sparkonto.AccountNumber))!
            .GetSetMethod(nonPublic: true).ShouldBeNull();
    }

    [Fact]
    public void IBAN_HasNoSetter()
    {
        typeof(Sparkonto).GetProperty(nameof(Sparkonto.IBAN))!
            .GetSetMethod(nonPublic: true).ShouldBeNull();
    }

    [Fact]
    public void Deposit_PositiveAmount_IncreasesBalance()
    {
        var account = new Sparkonto("Ida", 50m);

        account.Deposit(25m);

        account.Balance.ShouldBe(75m);
    }

    [Fact]
    public void Deposit_MultipleDeposits_Accumulate()
    {
        var account = new Sparkonto("Jonas", 0m);

        account.Deposit(10m);
        account.Deposit(20m);
        account.Deposit(30m);

        account.Balance.ShouldBe(60m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void Deposit_NonPositiveAmount_ThrowsAndLeavesBalance(decimal amount)
    {
        var account = new Sparkonto("Karin", 100m);

        Should.Throw<InvalidAmountException>(() => account.Deposit(amount));
        account.Balance.ShouldBe(100m);
    }

    [Fact]
    public void DepositTry_Success_ReturnsTrue()
    {
        var account = new Sparkonto("Lena", 0m);

        var ok = account.Deposit(15m, out var message);

        ok.ShouldBeTrue();
        message.ShouldContain("15");
        account.Balance.ShouldBe(15m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DepositTry_Failure_ReturnsFalse(decimal amount)
    {
        var account = new Sparkonto("Max", 50m);

        var ok = account.Deposit(amount, out var message);

        ok.ShouldBeFalse();
        message.ShouldNotBeNullOrWhiteSpace();
        account.Balance.ShouldBe(50m);
    }

    [Fact]
    public void Withdraw_PositiveAmountBelowBalance_DecreasesBalance()
    {
        var account = new Sparkonto("Nina", 200m);

        account.Withdraw(50m);

        account.Balance.ShouldBe(150m);
    }

    [Fact]
    public void Withdraw_ExactBalance_ReachesZero()
    {
        var account = new Sparkonto("Otto", 100m);

        account.Withdraw(100m);

        account.Balance.ShouldBe(0m);
    }

    [Fact]
    public void Withdraw_AmountAboveBalance_Throws()
    {
        var account = new Sparkonto("Paula", 50m);

        var ex = Should.Throw<InsufficientFundsException>(() => account.Withdraw(100m));
        ex.Requested.ShouldBe(100m);
        ex.Available.ShouldBe(50m);
        account.Balance.ShouldBe(50m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Withdraw_NonPositiveAmount_Throws(decimal amount)
    {
        var account = new Sparkonto("Quirin", 100m);

        Should.Throw<InvalidAmountException>(() => account.Withdraw(amount));
        account.Balance.ShouldBe(100m);
    }

    [Fact]
    public void WithdrawTry_Success_ReturnsTrue()
    {
        var account = new Sparkonto("Rolf", 100m);

        var ok = account.Withdraw(40m, out var message);

        ok.ShouldBeTrue();
        message.ShouldContain("40");
        account.Balance.ShouldBe(60m);
    }

    [Fact]
    public void WithdrawTry_InsufficientFunds_ReturnsFalse()
    {
        var account = new Sparkonto("Sara", 10m);

        var ok = account.Withdraw(50m, out var message);

        ok.ShouldBeFalse();
        message.ToLowerInvariant().ShouldContain("rejected");
        account.Balance.ShouldBe(10m);
    }

    [Fact]
    public void WithdrawTry_NegativeAmount_ReturnsFalse()
    {
        var account = new Sparkonto("Toni", 10m);

        var ok = account.Withdraw(-5m, out var message);

        ok.ShouldBeFalse();
        message.ShouldNotBeNullOrWhiteSpace();
        account.Balance.ShouldBe(10m);
    }

    [Fact]
    public void UpdateName_ValidName_ChangesHolder()
    {
        var account = new Sparkonto("Uli", 0m);

        account.UpdateName("Ursula");

        account.AccountHolder.ShouldBe("Ursula");
    }

    [Fact]
    public void UpdateName_TrimsWhitespace()
    {
        var account = new Sparkonto("Vera", 0m);

        account.UpdateName("  Vera Musterfrau  ");

        account.AccountHolder.ShouldBe("Vera Musterfrau");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void UpdateName_InvalidName_Throws(string? name)
    {
        var account = new Sparkonto("Walter", 0m);

        Should.Throw<InvalidNameException>(() => account.UpdateName(name!));
        account.AccountHolder.ShouldBe("Walter");
    }

    [Fact]
    public void UpdateNameTry_Success_ReturnsTrue()
    {
        var account = new Sparkonto("Xenia", 0m);

        var ok = account.UpdateName("Yvonne", out var message);

        ok.ShouldBeTrue();
        message.ShouldContain("Yvonne");
        account.AccountHolder.ShouldBe("Yvonne");
    }

    [Fact]
    public void UpdateNameTry_Failure_ReturnsFalse()
    {
        var account = new Sparkonto("Zoe", 0m);

        var ok = account.UpdateName("   ", out var message);

        ok.ShouldBeFalse();
        message.ShouldNotBeNullOrWhiteSpace();
        account.AccountHolder.ShouldBe("Zoe");
    }

    [Fact]
    public void ReadingHolderAndBalance_DoesNotMutate()
    {
        var account = new Sparkonto("Alfred", 1234.56m);

        for (var i = 0; i < 10; i++)
        {
            _ = account.AccountHolder;
            _ = account.Balance;
        }

        account.AccountHolder.ShouldBe("Alfred");
        account.Balance.ShouldBe(1234.56m);
    }

    [Fact]
    public void Invariant_BalanceNeverNegative_AfterMixedSequence()
    {
        var account = new Sparkonto("Beate", 100m);

        account.Deposit(50m);
        account.Withdraw(70m);
        Should.Throw<InsufficientFundsException>(() => account.Withdraw(1_000_000m));
        Should.Throw<InvalidAmountException>(() => account.Deposit(-1m));
        account.Withdraw(80m);
        Should.Throw<InsufficientFundsException>(() => account.Withdraw(0.01m));
        account.Deposit(0.01m);
        Should.Throw<InvalidAmountException>(() => account.Withdraw(0m));

        (account.Balance >= 0m).ShouldBeTrue();
    }

    [Fact]
    public void AllDomainExceptions_InheritFromSparkontoException()
    {
        typeof(SparkontoException).IsAssignableFrom(typeof(InvalidAmountException)).ShouldBeTrue();
        typeof(SparkontoException).IsAssignableFrom(typeof(InvalidNameException)).ShouldBeTrue();
        typeof(SparkontoException).IsAssignableFrom(typeof(InsufficientFundsException)).ShouldBeTrue();
        typeof(SparkontoException).IsAssignableFrom(typeof(InvariantViolationException)).ShouldBeTrue();
    }

    [Fact]
    public void TransferTo_HappyPath_DebitsSourceAndCreditsTarget()
    {
        var source = new Sparkonto("Anna", 200m);
        var target = new Sparkonto("Bob", 50m);

        source.TransferTo(target, 75m);

        source.Balance.ShouldBe(125m);
        target.Balance.ShouldBe(125m);
    }

    [Fact]
    public void TransferTo_HappyPath_PreservesTotalBalance()
    {
        var source = new Sparkonto("Anna", 200m);
        var target = new Sparkonto("Bob", 50m);
        decimal totalBefore = source.Balance + target.Balance;

        source.TransferTo(target, 75m);

        decimal totalAfter = source.Balance + target.Balance;
        totalAfter.ShouldBe(totalBefore);
    }

    [Fact]
    public void TransferTo_ExactBalance_Succeeds()
    {
        var source = new Sparkonto("Anna", 100m);
        var target = new Sparkonto("Bob", 0m);

        source.TransferTo(target, 100m);

        source.Balance.ShouldBe(0m);
        target.Balance.ShouldBe(100m);
    }

    [Fact]
    public void TransferTo_NullTarget_Throws()
    {
        var source = new Sparkonto("Anna", 100m);

        Should.Throw<ArgumentNullException>(() => source.TransferTo(null!, 10m));
        source.Balance.ShouldBe(100m);
    }

    [Fact]
    public void TransferTo_SelfTransfer_Throws()
    {
        var source = new Sparkonto("Anna", 100m);

        Should.Throw<SelfTransferException>(() => source.TransferTo(source, 10m));
        source.Balance.ShouldBe(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TransferTo_NonPositiveAmount_Throws(decimal amount)
    {
        var source = new Sparkonto("Anna", 100m);
        var target = new Sparkonto("Bob", 50m);

        Should.Throw<InvalidAmountException>(() => source.TransferTo(target, amount));
        source.Balance.ShouldBe(100m);
        target.Balance.ShouldBe(50m);
    }

    [Fact]
    public void TransferTo_InsufficientFunds_Throws()
    {
        var source = new Sparkonto("Anna", 50m);
        var target = new Sparkonto("Bob", 100m);

        Should.Throw<InsufficientFundsException>(() => source.TransferTo(target, 100m));
        source.Balance.ShouldBe(50m);
        target.Balance.ShouldBe(100m);
    }

    [Fact]
    public void TransferTo_FailedStart_LeavesBothBalancesUnchanged()
    {
        var source = new Sparkonto("Anna", 50m);
        var target = new Sparkonto("Bob", 100m);

        Should.Throw<InsufficientFundsException>(() => source.TransferTo(target, 200m));
        source.Balance.ShouldBe(50m);
        target.Balance.ShouldBe(100m);
    }

    [Fact]
    public void TransferTo_DoesNotViolateInvariants_AfterMultipleTransfers()
    {
        var a = new Sparkonto("Anna", 100m);
        var b = new Sparkonto("Bob", 100m);
        var c = new Sparkonto("Clara", 100m);

        a.TransferTo(b, 30m);
        b.TransferTo(c, 50m);
        c.TransferTo(a, 20m);

        (a.Balance >= 0m).ShouldBeTrue();
        (b.Balance >= 0m).ShouldBeTrue();
        (c.Balance >= 0m).ShouldBeTrue();
        (a.Balance + b.Balance + c.Balance).ShouldBe(300m);
    }
}
