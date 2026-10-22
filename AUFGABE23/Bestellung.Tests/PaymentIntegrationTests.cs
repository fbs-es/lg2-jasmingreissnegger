using System;
using Fbs.Lg2;
using Fbs.Lg2.Payment;
using Shouldly;

namespace Fbs.Lg2;

public class PaymentIntegrationTests
{
    private static Customer NewCustomer() => new("Anna", "a@b.com");
    private static Product NewProduct(Supplier s, int stock = 100)
        => new("Phone", 100m, stock, 1, s);
    private static Supplier NewSupplier() => new("Acme");

    [Fact]
    public void Ctor_DefaultPaymentService_NullPaymentService_Used()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);
        var o = new Order(c, DateTime.UtcNow);

        o.PaymentTransactionIds.ShouldBeEmpty();

        o.AddPosition(p, 1);
        o.PaymentTransactionIds.Count.ShouldBe(1);
    }

    [Fact]
    public void AddPosition_CallsPaymentService_Charge()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);
        var stub = new RecordingPaymentService();
        var o = new Order(c, DateTime.UtcNow, stub);

        o.AddPosition(p, 2);

        stub.ChargeCalls.Count.ShouldBe(1);
        stub.ChargeCalls[0].Amount.ShouldBe(200m);
        stub.ChargeCalls[0].Description.ShouldContain("Phone");
    }

    [Fact]
    public void AddPosition_MultiplePositions_AccumulatesPayment()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p1 = NewProduct(s);
        var p2 = new Product("Case", 50m, 50, 1, s);
        var stub = new RecordingPaymentService();
        var o = new Order(c, DateTime.UtcNow, stub);

        o.AddPosition(p1, 2);
        o.AddPosition(p2, 1);

        stub.ChargeCalls.Count.ShouldBe(2);
        o.PaidAmount.ShouldBe(250m);
        o.PaymentTransactionIds.ShouldBe(new[] { "STUB-001", "STUB-002" });
        o.GetPosition(1)!.PaymentTransactionId.ShouldBe("STUB-001");
        o.GetPosition(2)!.PaymentTransactionId.ShouldBe("STUB-002");
    }

    [Fact]
    public void Cancel_AfterPayment_CallsRefund()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);
        var stub = new RecordingPaymentService();
        var o = new Order(c, DateTime.UtcNow, stub);

        o.AddPosition(p, 2);
        o.Cancel();

        stub.RefundCalls.Count.ShouldBe(1);
        stub.RefundCalls[0].Amount.ShouldBe(200m);
        stub.RefundCalls[0].TransactionId.ShouldBe(o.PaymentTransactionIds[0]);
    }

    [Fact]
    public void Cancel_BeforeAnyPayment_DoesNotCallRefund()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var stub = new RecordingPaymentService();
        var o = new Order(c, DateTime.UtcNow, stub);

        o.Cancel();

        stub.RefundCalls.Count.ShouldBe(0);
    }

    [Fact]
    public void Cancel_DeclinedRefund_Throws()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);
        var stub = new RecordingPaymentService { RefundDeclined = true };
        var o = new Order(c, DateTime.UtcNow, stub);

        o.AddPosition(p, 1);
        Should.Throw<InvalidOrderPaymentException>(() => o.Cancel());

        // Refund failed: stock and status must stay unchanged.
        p.Stock.ShouldBe(99);
        o.Status.ShouldBe(OrderStatus.Offen);
        o.CancelledAt.ShouldBeNull();
    }

    [Fact]
    public void Cancel_TwoPositions_RefundsEachTransactionWithItsOwnAmount()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p1 = NewProduct(s);
        var p2 = new Product("Case", 50m, 50, 1, s);
        var stub = new RecordingPaymentService();
        var o = new Order(c, DateTime.UtcNow, stub);
        o.AddPosition(p1, 2);
        o.AddPosition(p2, 1);

        o.Cancel();

        stub.RefundCalls.ShouldBe(new[] { ("STUB-001", 200m), ("STUB-002", 50m) });
        o.PaidAmount.ShouldBe(0m);
        o.Status.ShouldBe(OrderStatus.Storniert);
    }

    [Fact]
    public void Cancel_TwoPositions_WithInMemoryPaymentService_Succeeds()
    {
        var s = NewSupplier();
        var p1 = new Product("A", 10m, 50, 1, s);
        var p2 = new Product("B", 20m, 50, 1, s);
        var o = new Order(NewCustomer(), DateTime.UtcNow, new InMemoryPaymentService());
        o.AddPosition(p1, 1);
        o.AddPosition(p2, 1);

        o.Cancel();

        o.Status.ShouldBe(OrderStatus.Storniert);
        p1.Stock.ShouldBe(50);
        p2.Stock.ShouldBe(50);
    }

    [Fact]
    public void Cancel_RetryAfterDeclinedRefund_DoesNotRefundOrRestockTwice()
    {
        var s = NewSupplier();
        var p = NewProduct(s);
        var stub = new RecordingPaymentService { RefundDeclined = true };
        var o = new Order(NewCustomer(), DateTime.UtcNow, stub);
        o.AddPosition(p, 3);
        Should.Throw<InvalidOrderPaymentException>(() => o.Cancel());

        stub.RefundDeclined = false;
        o.Cancel();

        p.Stock.ShouldBe(100);
        o.Status.ShouldBe(OrderStatus.Storniert);
        stub.RefundCalls.Count.ShouldBe(2);
        o.PaidAmount.ShouldBe(0m);
    }

    [Fact]
    public void AddPosition_DeclinedCharge_Throws()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);
        var stub = new RecordingPaymentService { ChargeDeclined = true };
        var o = new Order(c, DateTime.UtcNow, stub);

        Should.Throw<InvalidOrderPaymentException>(() => o.AddPosition(p, 1));

        // Atomic: nothing changed.
        o.PositionCount.ShouldBe(0);
        o.TotalAmount.ShouldBe(0m);
        o.PaidAmount.ShouldBe(0m);
        p.Stock.ShouldBe(100);
    }

    [Fact]
    public void DifferentPaymentServices_AreSwappable()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p = NewProduct(s);

        var stubA = new RecordingPaymentService();
        var oA = new Order(c, DateTime.UtcNow, stubA);
        oA.AddPosition(p, 1);
        stubA.ChargeCalls.Count.ShouldBe(1);

        var stubB = new RecordingPaymentService();
        var oB = new Order(c, DateTime.UtcNow, stubB);
        oB.AddPosition(p, 1);
        stubB.ChargeCalls.Count.ShouldBe(1);

        stubA.ChargeCalls.Count.ShouldBe(1);
    }
}

public class RecordingPaymentService : IPaymentService
{
    public List<(decimal Amount, string Description)> ChargeCalls { get; } = new();
    public List<(string TransactionId, decimal Amount)> RefundCalls { get; } = new();

    public bool ChargeDeclined { get; set; }
    public bool RefundDeclined { get; set; }

    public PaymentResult Charge(decimal amount, string description)
    {
        ChargeCalls.Add((amount, description));
        if (ChargeDeclined)
        {
            return PaymentResult.Declined("Test decline");
        }
        return PaymentResult.Approved($"STUB-{ChargeCalls.Count:D3}");
    }

    public PaymentResult Refund(string transactionId, decimal amount)
    {
        RefundCalls.Add((transactionId, amount));
        if (RefundDeclined)
        {
            return PaymentResult.Declined("Test decline");
        }
        return PaymentResult.Approved($"{transactionId}-REFUND");
    }
}
