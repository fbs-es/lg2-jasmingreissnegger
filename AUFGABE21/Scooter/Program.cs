using Fbs.Lg2;

Console.WriteLine("=== CityGlide Demo ===\n");

Scooter scooter1 = new();
Scooter scooter2 = new();
Customer customer = new("Anna Mueller", "anna@example.com", 10.00m);
Console.WriteLine($"Created scooters #{scooter1.ScooterId} and #{scooter2.ScooterId}");
Console.WriteLine($"Created customer #{customer.CustomerId} ({customer.Name}, balance {customer.Balance:C}, blocked={customer.IsBlocked})");

customer.StartRental(scooter1);
Console.WriteLine($"[OK] rental started on scooter #{scooter1.ScooterId}");
customer.EndRental(minutes: 30);
Console.WriteLine($"[OK] rental ended | balance {customer.Balance:C}, scooter battery {scooter1.BatteryLevel}%, locked={scooter1.IsLocked}");

customer.StartRental(scooter2);
Console.WriteLine($"[OK] rental started on scooter #{scooter2.ScooterId}");
customer.EndRental(minutes: 60);
Console.WriteLine($"[OK] rental ended | balance {customer.Balance:C}, blocked={customer.IsBlocked}, dunning={customer.HasOpenDunning}, emails sent={customer.SentDunningEmails.Count}");
Console.WriteLine($"    last email: {customer.SentDunningEmails[^1]}");

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
