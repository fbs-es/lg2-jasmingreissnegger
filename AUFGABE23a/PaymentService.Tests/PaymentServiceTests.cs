using Fbs.Lg2.Payment;
using Shouldly;

namespace Fbs.Lg2.Payment;

public class InMemoryPaymentServiceTests
{
    [Fact]
    public void Charge_PositiveAmount_ReturnsApprovedWithTxId()
    {
        var ps = new InMemoryPaymentService();

        var result = ps.Charge(99.95m, "Test order");

        result.Success.ShouldBeTrue();
        result.TransactionId.ShouldNotBeNull();
        result.FailureReason.ShouldBeNull();
    }

    [Fact]
    public void Charge_ZeroAmount_ReturnsDeclined()
    {
        var ps = new InMemoryPaymentService();
        var result = ps.Charge(0m, "Test");
        result.Success.ShouldBeFalse();
        result.FailureReason.ShouldNotBeNull();
    }

    [Fact]
    public void Charge_NegativeAmount_ReturnsDeclined()
    {
        var ps = new InMemoryPaymentService();
        var result = ps.Charge(-10m, "Test");
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Charge_EmptyDescription_ReturnsDeclined()
    {
        var ps = new InMemoryPaymentService();
        var result = ps.Charge(10m, "");
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Charge_TwoCharges_ProduceDistinctIds()
    {
        var ps = new InMemoryPaymentService();
        var r1 = ps.Charge(10m, "First");
        var r2 = ps.Charge(20m, "Second");
        r1.TransactionId.ShouldNotBe(r2.TransactionId);
    }

    [Fact]
    public void Refund_KnownTransaction_Approves()
    {
        var ps = new InMemoryPaymentService();
        var charge = ps.Charge(50m, "Order");
        var refund = ps.Refund(charge.TransactionId!, 50m);
        refund.Success.ShouldBeTrue();
    }

    [Fact]
    public void Refund_UnknownTransaction_Declines()
    {
        var ps = new InMemoryPaymentService();
        var result = ps.Refund("TX-NONEXISTENT", 10m);
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Refund_AmountExceedingCharge_Declines()
    {
        var ps = new InMemoryPaymentService();
        var charge = ps.Charge(50m, "Order");
        var refund = ps.Refund(charge.TransactionId!, 100m);
        refund.Success.ShouldBeFalse();
    }

    [Fact]
    public void Refund_EmptyTransactionId_Declines()
    {
        var ps = new InMemoryPaymentService();
        var result = ps.Refund("", 10m);
        result.Success.ShouldBeFalse();
    }
}

public class NullPaymentServiceTests
{
    [Fact]
    public void Charge_AlwaysApproves()
    {
        var ps = new NullPaymentService();
        var result = ps.Charge(100m, "Anything");
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void Refund_AlwaysApproves()
    {
        var ps = new NullPaymentService();
        var result = ps.Refund("WHATEVER", 100m);
        result.Success.ShouldBeTrue();
    }
}

public class PaymentResultTests
{
    [Fact]
    public void Approved_CarriesTransactionId_NoFailureReason()
    {
        var r = PaymentResult.Approved("TX-001");
        r.Success.ShouldBeTrue();
        r.TransactionId.ShouldBe("TX-001");
        r.FailureReason.ShouldBeNull();
    }

    [Fact]
    public void Declined_CarriesReason_NoTransactionId()
    {
        var r = PaymentResult.Declined("Out of stock");
        r.Success.ShouldBeFalse();
        r.TransactionId.ShouldBeNull();
        r.FailureReason.ShouldBe("Out of stock");
    }
}
