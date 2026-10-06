using Fbs.Lg2;

Console.WriteLine("=== CityGlide Demo ===\n");

Scooter scooter1 = new();
Scooter scooter2 = new();
Customer customer = new("Anna Mueller", "anna@example.com", 10.00m);
Console.WriteLine($"Created scooters #{scooter1.ScooterId} and #{scooter2.ScooterId}");
Console.WriteLine($"Created customer #{customer.CustomerId} ({customer.Name}, balance {customer.Balance:C}, blocked={customer.IsBlocked})");

DateTime t0 = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

customer.StartRental(scooter1, t0);
Console.WriteLine($"[OK] rental started on scooter #{scooter1.ScooterId} at {t0:O}");
customer.EndRental(t0.AddMinutes(30));
Console.WriteLine($"[OK] rental ended | balance {customer.Balance:C}, scooter battery {scooter1.BatteryLevel}%, locked={scooter1.IsLocked}");

customer.StartRental(scooter2, t0.AddHours(1));
Console.WriteLine($"[OK] rental started on scooter #{scooter2.ScooterId}");
customer.EndRental(t0.AddHours(1).AddMinutes(60));
Console.WriteLine($"[OK] rental ended | balance {customer.Balance:C}, blocked={customer.IsBlocked}, dunning={customer.HasOpenDunning}, emails sent={customer.SentDunningEmails.Count}");
Console.WriteLine($"    last email: {customer.SentDunningEmails[^1]}");

Console.WriteLine();
Console.WriteLine($"Customer #{customer.CustomerId} rental history ({customer.RentalHistory.Count} entries):");
foreach (var entry in customer.RentalHistory)
{
    Console.WriteLine($"  - rental #{entry.RentalId}: scooter #{entry.Scooter.ScooterId}, {entry.StartTime:HH:mm} -> {entry.EndTime:HH:mm}, duration {entry.Duration.TotalMinutes} min, cost {entry.Cost:C}");
}

Console.WriteLine();
Console.WriteLine($"Scooter #{scooter1.ScooterId} rental history ({scooter1.RentalHistory.Count} entries):");
foreach (var entry in scooter1.RentalHistory)
{
    Console.WriteLine($"  - rental #{entry.RentalId}: customer #{entry.Customer.CustomerId}, cost {entry.Cost:C}");
}

try
{
    customer.StartRental(scooter1);
}
catch (CustomerBlockedException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

customer.TopUp(10.00m);
Console.WriteLine($"[OK] top-up | balance {customer.Balance:C}, blocked={customer.IsBlocked}, dunning={customer.HasOpenDunning}");

Console.WriteLine("\n=== Demo done ===");