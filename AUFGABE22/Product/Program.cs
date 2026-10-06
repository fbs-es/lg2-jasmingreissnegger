using Fbs.Lg2;

Console.WriteLine("=== Review Portal Demo ===\n");

Manufacturer manu = new("Acme GmbH", "support@acme.example");
Product product = new("Wireless Headphones", 99.90m, manu);
Customer alice = new("Alice");
Customer bob = new("Bob");

Console.WriteLine($"Manufacturer: {manu.Name} <{manu.SupportEmail}>");
Console.WriteLine($"Product: {product.Name} ({product.Price:C}) made by {product.Manufacturer.Name}");
Console.WriteLine($"Customers: {alice.Name}, {bob.Name}\n");

DateTime now = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
Review r1 = new(alice, product, 5, "Top!", now, verifiedPurchase: true);
Review r2 = new(bob, product, 3, "Ok, but battery life is short", now.AddDays(1), verifiedPurchase: false);

Console.WriteLine($"Reviews so far: {product.Reviews.Count}");
Console.WriteLine($"  Alice: {r1.Stars}* - {r1.Comment} (verified={r1.VerifiedPurchase})");
Console.WriteLine($"  Bob:   {r2.Stars}* - {r2.Comment} (verified={r2.VerifiedPurchase})");
Console.WriteLine($"Average stars: {product.AverageStars():0.00}\n");

Reply reply = product.Reviews[0].AddReply("Thanks for the kind words!", now.AddDays(2));
Console.WriteLine($"Vendor reply to review #{r1.ReviewId}: \"{reply.Text}\"");

try
{
    r1.AddReply("second reply", now.AddDays(3));
}
catch (ReviewAlreadyRepliedException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

Console.WriteLine("\n=== Demo done ===");