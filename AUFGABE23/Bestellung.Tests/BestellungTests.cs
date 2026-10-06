using System;
using System.Linq;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class SupplierTests
{
    [Fact]
    public void Ctor_ValidName_CreatesSupplier()
    {
        var s = new Supplier("Acme GmbH");

        s.Name.ShouldBe("Acme GmbH");
        s.SupplierId.ShouldBeGreaterThan(0);
        s.Products.ShouldBeEmpty();
        s.SupplierOrders.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Supplier(name!));
    }

    [Fact]
    public void UpdateName_ValidName_ChangesName()
    {
        var s = new Supplier("Acme");
        s.UpdateName("NewName");
        s.Name.ShouldBe("NewName");
    }

    [Fact]
    public void Products_AreAttachedViaNewProduct()
    {
        var s = new Supplier("Acme");
        _ = new Product("Phone", 100m, 10, 3, s);

        s.Products.Count.ShouldBe(1);
    }
}

public class CustomerTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesCustomer()
    {
        var c = new Customer("Anna", "anna@example.com");

        c.Name.ShouldBe("Anna");
        c.Email.ShouldBe("anna@example.com");
        c.CustomerId.ShouldBeGreaterThan(0);
        c.Orders.ShouldBeEmpty();
    }

    [Fact]
    public void Ctor_TrimsWhitespace()
    {
        var c = new Customer("  Bob  ", "  bob@x.com  ");
        c.Name.ShouldBe("Bob");
        c.Email.ShouldBe("bob@x.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Customer(name!, "a@b.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Ctor_InvalidEmail_Throws(string? email)
    {
        Should.Throw<InvalidEmailException>(() => new Customer("Bob", email!));
    }

    [Fact]
    public void Orders_AreAttachedViaNewOrder()
    {
        var c = new Customer("Alice", "a@b.com");
        _ = new Order(c, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        c.Orders.Count.ShouldBe(1);
    }
}

public class ProductTests
{
    private static Supplier NewSupplier() => new("Acme");
    private static Product NewProduct(int stock = 50, int min = 5)
        => new("Phone", 100m, stock, min, NewSupplier());

    [Fact]
    public void Ctor_ValidInputs_CreatesProduct()
    {
        var p = NewProduct();

        p.Name.ShouldBe("Phone");
        p.Price.ShouldBe(100m);
        p.Stock.ShouldBe(50);
        p.MinStock.ShouldBe(5);
        p.ProductId.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Ctor_NegativePrice_Throws(decimal price)
    {
        Should.Throw<InvalidPriceException>(() => new Product("P", price, 10, 1, NewSupplier()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Product(name!, 100m, 10, 1, NewSupplier()));
    }

    [Fact]
    public void Ctor_NullSupplier_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new Product("Phone", 100m, 10, 1, null!));
    }

    [Fact]
    public void Ctor_NegativeStock_Throws()
    {
        Should.Throw<InvalidStockException>(() => new Product("Phone", 100m, -1, 0, NewSupplier()));
    }

    [Fact]
    public void Ctor_NegativeMinStock_Throws()
    {
        Should.Throw<InvalidStockException>(() => new Product("Phone", 100m, 10, -1, NewSupplier()));
    }

    [Fact]
    public void DecreaseStock_PositionsStock()
    {
        var p = NewProduct(50, 5);
        p.DecreaseStock(10);
        p.Stock.ShouldBe(40);
    }

    [Fact]
    public void DecreaseStock_GoesBelowMin_TriggersZeroAutoReorder()
    {
        var p = NewProduct(10, 5);
        p.DecreaseStock(6);
        p.Stock.ShouldBe(4);
        p.NeedsReorder().ShouldBeTrue();
        p.SupplierOrders.Count.ShouldBe(1);
    }

    [Fact]
    public void DecreaseStock_HitsMinExactly_TriggersZeroAutoReorder()
    {
        var p = NewProduct(10, 5);
        p.DecreaseStock(5);
        p.Stock.ShouldBe(5);
        p.NeedsReorder().ShouldBeTrue();
        p.SupplierOrders.Count.ShouldBe(1);
    }

    [Fact]
    public void DecreaseStock_StaysAboveMin_DoesZeroReorderTrigger()
    {
        var p = NewProduct(50, 5);
        p.DecreaseStock(10);
        p.Stock.ShouldBe(40);
        p.NeedsReorder().ShouldBeFalse();
        p.SupplierOrders.Count.ShouldBe(0);
    }

    [Fact]
    public void DecreaseStock_NotEnoughStock_Throws()
    {
        var p = NewProduct(5, 1);
        Should.Throw<InvalidStockException>(() => p.DecreaseStock(10));
        p.Stock.ShouldBe(5);
        p.SupplierOrders.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseStock_NonPositive_Throws(int amount)
    {
        var p = NewProduct();
        Should.Throw<InvalidQuantityException>(() => p.DecreaseStock(amount));
    }

    [Fact]
    public void IncreaseStock_RestoresStock()
    {
        var p = NewProduct(50, 5);
        p.DecreaseStock(40);
        p.IncreaseStock(30);
        p.Stock.ShouldBe(40);
    }

    [Fact]
    public void UpdatePrice_ValidPrice_ChangesPrice()
    {
        var p = NewProduct();
        p.UpdatePrice(200m);
        p.Price.ShouldBe(200m);
    }

    [Fact]
    public void UpdatePrice_NegativePrice_Throws()
    {
        var p = NewProduct();
        Should.Throw<InvalidPriceException>(() => p.UpdatePrice(-1m));
    }

    [Fact]
    public void NeedsReorder_ReturnsTrueWhenStockAtMin()
    {
        var p = NewProduct(5, 5);
        p.NeedsReorder().ShouldBeTrue();
    }

    [Fact]
    public void NeedsReorder_ReturnsTrueWhenStockBelowMin()
    {
        var p = NewProduct(3, 5);
        p.NeedsReorder().ShouldBeTrue();
    }

    [Fact]
    public void NeedsReorder_ReturnsFalseWhenStockAboveMin()
    {
        var p = NewProduct(10, 5);
        p.NeedsReorder().ShouldBeFalse();
    }
}

public class OrderTests
{
    private static Supplier NewSupplier() => new("Acme");
    private static Customer NewCustomer(string name = "Alice") => new(name, "a@b.com");
    private static Product NewProduct(int stock = 50, int min = 5)
        => new("Phone", 100m, stock, min, NewSupplier());
    private static DateTime Now => new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ctor_ValidInputs_CreatesOrder()
    {
        var c = NewCustomer();
        var o = new Order(c, Now);

        o.Customer.ShouldBeSameAs(c);
        o.OrderDate.ShouldBe(Now);
        o.Status.ShouldBe(OrderStatus.Offen);
        o.Positions.ShouldBeEmpty();
        o.TotalAmount.ShouldBe(0m);
    }

    [Fact]
    public void AddPosition_ValidProduct_AppendsPosition()
    {
        var c = NewCustomer();
        var p = NewProduct(50, 5);
        var o = new Order(c, Now);

        var pos = o.AddPosition(p, 3);

        pos.PositionNumber.ShouldBe(1);
        pos.Quantity.ShouldBe(3);
        pos.Product.ShouldBeSameAs(p);
        pos.HistoricalUnitPrice.ShouldBe(100m);
        pos.Order.ShouldBeSameAs(o);
        o.Positions.Count.ShouldBe(1);
        p.Stock.ShouldBe(47);
    }

    [Fact]
    public void AddPosition_MultiplePositions_IncrementPositionNumber()
    {
        var c = NewCustomer();
        var s = NewSupplier();
        var p1 = NewProduct(50, 5);
        var p2 = new Product("Case", 99m, 50, 5, s);
        _ = p2;

        var o = new Order(c, Now);
        var pos1 = o.AddPosition(p1, 2);
        var pos2 = o.AddPosition(p1, 1);

        pos1.PositionNumber.ShouldBe(1);
        pos2.PositionNumber.ShouldBe(2);
    }

    [Fact]
    public void AddPosition_CapturesHistoricalPrice()
    {
        var c = NewCustomer();
        var p = NewProduct(50, 5);
        var o = new Order(c, Now);
        var pos = o.AddPosition(p, 1);

        p.UpdatePrice(150m);

        pos.HistoricalUnitPrice.ShouldBe(100m);
        o.TotalAmount.ShouldBe(100m);
    }

    [Fact]
    public void AddPosition_InsufficientStock_Throws()
    {
        var c = NewCustomer();
        var p = NewProduct(2, 1);
        var o = new Order(c, Now);

        Should.Throw<InvalidStockException>(() => o.AddPosition(p, 10));
        o.Positions.ShouldBeEmpty();
        p.Stock.ShouldBe(2);
    }

    [Fact]
    public void AddPosition_OnShippedOrder_Throws()
    {
        var c = NewCustomer();
        var p = NewProduct();
        var o = new Order(c, Now);
        o.AddPosition(p, 1);
        o.MarkShipped();

        Should.Throw<OrderNotModifiableException>(() => o.AddPosition(p, 1));
    }

    [Fact]
    public void AddPosition_OnCancelledOrder_Throws()
    {
        var c = NewCustomer();
        var p = NewProduct();
        var o = new Order(c, Now);
        o.AddPosition(p, 1);
        o.Cancel();

        Should.Throw<OrderNotModifiableException>(() => o.AddPosition(p, 1));
    }

    [Fact]
    public void TotalAmount_SumsAcrossAllPositions()
    {
        var c = NewCustomer();
        var p = NewProduct(100, 5);
        var o = new Order(c, Now);
        o.AddPosition(p, 2);
        o.AddPosition(p, 3);

        o.TotalAmount.ShouldBe(500m);
    }

    [Fact]
    public void MarkShipped_FromOffen_Succeeds()
    {
        var c = NewCustomer();
        var o = new Order(c, Now);
        o.MarkShipped();
        o.Status.ShouldBe(OrderStatus.Versendet);
    }

    [Fact]
    public void MarkShipped_FromShipped_Throws()
    {
        var c = NewCustomer();
        var o = new Order(c, Now);
        o.MarkShipped();

        Should.Throw<OrderNotModifiableException>(() => o.MarkShipped());
    }

    [Fact]
    public void Cancel_FromOffen_RestoresStockAndChangesStatus()
    {
        var c = NewCustomer();
        var p = NewProduct(50, 5);
        var o = new Order(c, Now);
        o.AddPosition(p, 5);
        p.Stock.ShouldBe(45);

        o.Cancel();

        o.Status.ShouldBe(OrderStatus.Storniert);
        o.CancelledAt.ShouldNotBeNull();
        p.Stock.ShouldBe(50);
    }

    [Fact]
    public void Cancel_FromShipped_Throws()
    {
        var c = NewCustomer();
        var p = NewProduct();
        var o = new Order(c, Now);
        o.AddPosition(p, 1);
        o.MarkShipped();

        Should.Throw<OrderNotCancellableException>(() => o.Cancel());
        p.Stock.ShouldBe(49);
    }

    [Fact]
    public void Cancel_FromAlreadyCancelled_Throws()
    {
        var c = NewCustomer();
        var p = NewProduct();
        var o = new Order(c, Now);
        o.AddPosition(p, 1);
        o.Cancel();

        Should.Throw<OrderNotCancellableException>(() => o.Cancel());
    }

    [Fact]
    public void Cancel_EmptyOrder_IsAllowed()
    {
        var c = NewCustomer();
        var o = new Order(c, Now);
        o.Cancel();
        o.Status.ShouldBe(OrderStatus.Storniert);
        o.Positions.ShouldBeEmpty();
    }

    [Fact]
    public void Cancel_TriggersAutoReorderIfStockNowAtMin()
    {
        var c = NewCustomer();
        var p = NewProduct(10, 5);
        var o = new Order(c, Now);
        o.AddPosition(p, 6);
        p.Stock.ShouldBe(4);

        o.Cancel();

        p.Stock.ShouldBe(10);
        p.SupplierOrders.Count.ShouldBe(1);
    }

    [Fact]
    public void Order_RecordsDistinctIds()
    {
        var c = NewCustomer();
        var o1 = new Order(c, Now);
        var o2 = new Order(c, Now);
        o1.OrderId.ShouldNotBe(o2.OrderId);
    }
}

public class SupplierOrderTests
{
    [Fact]
    public void CreatedWith_Product_DateAndQuantity()
    {
        var s = new Supplier("Acme");
        var p = new Product("Phone", 100m, 10, 5, s);

        p.DecreaseStock(7);

        p.SupplierOrders.Count.ShouldBe(1);
        var so = p.SupplierOrders[0];
        so.Product.ShouldBeSameAs(p);
        so.Quantity.ShouldBeGreaterThan(0);
        so.OrderDate.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public void MultipleDecreasesBelowMin_CanCreateMultipleSupplierOrders()
    {
        var s = new Supplier("Acme");
        var p = new Product("Phone", 100m, 10, 5, s);

        p.DecreaseStock(6);
        p.IncreaseStock(1);
        p.DecreaseStock(2);

        p.SupplierOrders.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void SupplierOrder_AppearsInSupplierList()
    {
        var s = new Supplier("Acme");
        var p = new Product("Phone", 100m, 10, 5, s);

        p.DecreaseStock(8);

        s.SupplierOrders.Count.ShouldBe(1);
        s.SupplierOrders[0].Product.ShouldBeSameAs(p);
    }
}