using System;
using System.Linq;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class CustomerTests
{
    private static Customer NewCustomer(string name = "Anna") => new(name);

    [Fact]
    public void Ctor_ValidName_CreatesCustomer()
    {
        var c = new Customer("Anna Mueller");

        c.Name.ShouldBe("Anna Mueller");
        c.CustomerId.ShouldBeGreaterThan(0);
        c.Reviews.ShouldBeEmpty();
    }

    [Fact]
    public void Ctor_TrimsWhitespace()
    {
        var c = new Customer("  Bob  ");
        c.Name.ShouldBe("Bob");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Customer(name!));
    }

    [Fact]
    public void UpdateName_ValidName_ChangesName()
    {
        var c = new Customer("Bob");
        c.UpdateName("Robert");
        c.Name.ShouldBe("Robert");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_InvalidName_Throws(string? name)
    {
        var c = new Customer("Bob");
        Should.Throw<InvalidNameException>(() => c.UpdateName(name!));
        c.Name.ShouldBe("Bob");
    }

    [Fact]
    public void Reviews_AreAttachedViaNewReview()
    {
        var c = new Customer("Alice");
        var m = new Manufacturer("Acme", "a@x.com");
        var p = new Product("Phone", 100m, m);

        _ = new Review(c, p, 5, "great", DateTime.UtcNow, true);

        c.Reviews.Count.ShouldBe(1);
    }

    [Fact]
    public void Ctor_DistinctCustomers_HaveDistinctIds()
    {
        var a = new Customer("A");
        var b = new Customer("B");
        a.CustomerId.ShouldNotBe(b.CustomerId);
    }
}

public class ManufacturerTests
{
    private static Manufacturer NewManu(string name = "Acme", string email = "support@acme.example")
        => new(name, email);

    [Fact]
    public void Ctor_ValidInputs_CreatesManufacturer()
    {
        var m = new Manufacturer("Acme", "support@acme.example");
        m.Name.ShouldBe("Acme");
        m.SupportEmail.ShouldBe("support@acme.example");
        m.Products.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        Should.Throw<InvalidNameException>(() => new Manufacturer(name!, "x@y.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plain-string")]
    [InlineData("@nope.com")]
    public void Ctor_InvalidEmail_Throws(string? email)
    {
        Should.Throw<InvalidEmailException>(() => new Manufacturer("Acme", email!));
    }

    [Fact]
    public void UpdateEmail_Invalid_Throws()
    {
        var m = NewManu();
        Should.Throw<InvalidEmailException>(() => m.UpdateSupportEmail("nope"));
    }

    [Fact]
    public void Products_AreAttachedViaNewProduct()
    {
        var m = NewManu();
        _ = new Product("Phone", 100m, m);
        _ = new Product("Tablet", 200m, m);
        m.Products.Count.ShouldBe(2);
    }
}

public class ProductTests
{
    private static Manufacturer NewManu() => new("Acme", "a@x.com");

    [Fact]
    public void Ctor_ValidInputs_CreatesProduct()
    {
        var m = NewManu();
        var p = new Product("Phone", 599m, m);

        p.Name.ShouldBe("Phone");
        p.Price.ShouldBe(599m);
        p.Manufacturer.ShouldBeSameAs(m);
        p.Reviews.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Ctor_NegativePrice_Throws(decimal price)
    {
        var m = NewManu();
        Should.Throw<InvalidPriceException>(() => new Product("Phone", price, m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_InvalidName_Throws(string? name)
    {
        var m = NewManu();
        Should.Throw<InvalidNameException>(() => new Product(name!, 100m, m));
    }

    [Fact]
    public void Ctor_NullManufacturer_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new Product("Phone", 100m, null!));
    }

    [Fact]
    public void UpdatePrice_Negative_Throws()
    {
        var p = new Product("Phone", 100m, NewManu());
        Should.Throw<InvalidPriceException>(() => p.UpdatePrice(-1m));
    }

    [Fact]
    public void AverageStars_NoReviews_ReturnsZero()
    {
        var p = new Product("Phone", 100m, NewManu());
        p.AverageStars().ShouldBe(0m);
    }

    [Fact]
    public void AverageStars_SingleReview_ReturnsThatValue()
    {
        var p = new Product("Phone", 100m, NewManu());
        var c = new Customer("Alice");
        _ = new Review(c, p, 4, "good", DateTime.UtcNow, true);
        p.AverageStars().ShouldBe(4m);
    }

    [Fact]
    public void AverageStars_MultipleReviews_ReturnsArithmeticMean()
    {
        var p = new Product("Phone", 100m, NewManu());
        var alice = new Customer("Alice");
        var bob = new Customer("Bob");
        var carl = new Customer("Carl");
        _ = new Review(alice, p, 5, "great", DateTime.UtcNow, true);
        _ = new Review(bob, p, 3, "ok",    DateTime.UtcNow, true);
        _ = new Review(carl, p, 4, "fine",  DateTime.UtcNow, false);

        p.AverageStars().ShouldBe(4m);
    }

    [Fact]
    public void Reviews_AreAttachedViaNewReview()
    {
        var p = new Product("Phone", 100m, NewManu());
        var c = new Customer("Alice");
        _ = new Review(c, p, 5, "great", DateTime.UtcNow, true);
        p.Reviews.Count.ShouldBe(1);
    }

    [Fact]
    public void Ctor_DistinctProducts_HaveDistinctIds()
    {
        var m = NewManu();
        var a = new Product("A", 1m, m);
        var b = new Product("B", 1m, m);
        a.ProductId.ShouldNotBe(b.ProductId);
    }
}

public class ReviewTests
{
    private static Manufacturer M() => new("Acme", "a@x.com");
    private static Product P() => new("Phone", 100m, M());
    private static Customer C(string name = "Alice") => new(name);
    private static DateTime Now => new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ctor_ValidInputs_CreatesReview()
    {
        var c = C(); var p = P();
        var r = new Review(c, p, 4, "good", Now, true);

        r.Stars.ShouldBe(4);
        r.Comment.ShouldBe("good");
        r.CreatedAt.ShouldBe(Now);
        r.VerifiedPurchase.ShouldBeTrue();
        r.Customer.ShouldBeSameAs(c);
        r.Product.ShouldBeSameAs(p);
        r.Reply.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Ctor_InvalidStars_Throws(int stars)
    {
        Should.Throw<InvalidStarsException>(() => new Review(C(), P(), stars, "c", Now, false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidComment_Throws(string? c)
    {
        Should.Throw<InvalidCommentException>(() => new Review(C(), P(), 5, c!, Now, false));
    }

    [Fact]
    public void Ctor_NullCustomer_Throws() =>
        Should.Throw<ArgumentNullException>(() => new Review(null!, P(), 5, "c", Now, false));

    [Fact]
    public void Ctor_NullProduct_Throws() =>
        Should.Throw<ArgumentNullException>(() => new Review(C(), null!, 5, "c", Now, false));

    [Fact]
    public void UpdateStars_Invalid_Throws()
    {
        var r = new Review(C(), P(), 3, "ok", Now, false);
        Should.Throw<InvalidStarsException>(() => r.UpdateStars(0));
        r.Stars.ShouldBe(3);
    }

    [Fact]
    public void UpdateStars_Valid_ChangesStars()
    {
        var r = new Review(C(), P(), 3, "ok", Now, false);
        r.UpdateStars(5);
        r.Stars.ShouldBe(5);
    }

    [Fact]
    public void AddReply_FirstTime_CreatesReply()
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        var reply = r.AddReply("thanks!", Now.AddDays(1));

        reply.ShouldNotBeNull();
        reply.Text.ShouldBe("thanks!");
        reply.ReplyDate.ShouldBe(Now.AddDays(1));
        reply.Review.ShouldBeSameAs(r);
        r.Reply.ShouldBeSameAs(reply);
    }

    [Fact]
    public void AddReply_SecondTime_Throws()
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        r.AddReply("first", Now);
        Should.Throw<ReviewAlreadyRepliedException>(() => r.AddReply("second", Now.AddDays(1)));
        r.Reply!.Text.ShouldBe("first");
    }

    [Fact]
    public void RemoveReply_WithReply_ReturnsTrueAndClears()
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        r.AddReply("first", Now);

        r.RemoveReply().ShouldBeTrue();
        r.Reply.ShouldBeNull();
    }

    [Fact]
    public void RemoveReply_WithoutReply_ReturnsFalse()
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        r.RemoveReply().ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddReply_InvalidText_Throws(string? text)
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        Should.Throw<InvalidCommentException>(() => r.AddReply(text!, Now));
    }

    [Fact]
    public void ReadingProperties_DoesNotMutate()
    {
        var r = new Review(C(), P(), 5, "great", Now, true);
        for (var i = 0; i < 5; i++)
        {
            _ = r.Stars; _ = r.Comment; _ = r.CreatedAt;
            _ = r.VerifiedPurchase; _ = r.Customer; _ = r.Product;
            _ = r.Reply; _ = r.ReviewId;
        }
        r.Stars.ShouldBe(5);
        r.Comment.ShouldBe("great");
        r.VerifiedPurchase.ShouldBeTrue();
    }
}

public class ReplyTests
{
    private static Manufacturer M() => new("Acme", "a@x.com");
    private static Product P() => new("Phone", 100m, M());
    private static Customer C() => new("Alice");
    private static DateTime Now => new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static Review R() => new(C(), P(), 5, "great", Now, true);

    [Fact]
    public void AddReply_CreatesReply_WithCorrectReview()
    {
        var r = R();
        var reply = r.AddReply("thanks!", Now.AddDays(1));

        reply.Text.ShouldBe("thanks!");
        reply.ReplyDate.ShouldBe(Now.AddDays(1));
        reply.Review.ShouldBeSameAs(r);
    }

    [Fact]
    public void UpdateText_Invalid_Throws()
    {
        var r = R();
        var reply = r.AddReply("ok", Now);
        Should.Throw<InvalidCommentException>(() => reply.UpdateText(""));
        reply.Text.ShouldBe("ok");
    }

    [Fact]
    public void UpdateText_Valid_ChangesText()
    {
        var r = R();
        var reply = r.AddReply("ok", Now);
        reply.UpdateText("  thanks a lot!  ");
        reply.Text.ShouldBe("thanks a lot!");
    }

    [Fact]
    public void Review_HasAtMostOneReply_Enforced()
    {
        var r = R();
        r.AddReply("first", Now);

        Should.Throw<ReviewAlreadyRepliedException>(() => r.AddReply("second", Now.AddDays(1)));
    }
}
