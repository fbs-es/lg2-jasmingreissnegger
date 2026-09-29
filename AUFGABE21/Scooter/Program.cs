using Fbs.Lg2;

Console.WriteLine("=== CityGlide Demo ===\n");

Scooter scooter = new();
Customer customer = new("Anna Mueller", "anna@example.com", 10.00m);
Console.WriteLine($"Created scooter #{scooter.ScooterId} (battery {scooter.BatteryLevel}%, locked={scooter.IsLocked})");
Console.WriteLine($"Created customer #{customer.CustomerId} ({customer.Name}, balance {customer.Balance:C}, blocked={customer.IsBlocked})");

customer.StartRental(scooter);
Console.WriteLine($"[OK] rental started | scooter unlocked={!scooter.IsLocked}, customer has scooter #{customer.CurrentRental!.ScooterId}");

customer.EndRental(minutes: 30);
Console.WriteLine($"[OK] rental ended | customer balance {customer.Balance:C}, scooter battery {scooter.BatteryLevel}%, scooter locked={scooter.IsLocked}");

customer.EndRental(minutes: 60);
Console.WriteLine($"[OK] rental ended | customer balance {customer.Balance:C}, blocked={customer.IsBlocked}, dunning={customer.HasOpenDunning}, emails sent={customer.SentDunningEmails.Count}");
Console.WriteLine($"    last email: {customer.SentDunningEmails[^1]}");

try
{
    customer.StartRental(scooter);
}
catch (CustomerBlockedException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

customer.TopUp(10.00m);
Console.WriteLine($"[OK] top-up | balance {customer.Balance:C}, blocked={customer.IsBlocked}, dunning={customer.HasOpenDunning}");

Console.WriteLine("\n=== Demo done ===");
