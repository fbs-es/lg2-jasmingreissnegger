using Fbs.Lg2;

Console.WriteLine("=== Online-Shop Demo ===\n");

Supplier supplier = new("Acme GmbH");
Product phone = new("Smartphone", 599.00m, initialStock: 10, minStock: 4, supplier);
Product caseProd = new("Phone Case", 19.90m, initialStock: 50, minStock: 10, supplier);

Customer alice = new("Alice", "alice@example.com");

Console.WriteLine($"Supplier: {supplier.Name}");
Console.WriteLine($"Products: {phone.Name} ({phone.Price:C}, stock={phone.Stock}, min={phone.MinStock}), {caseProd.Name} ({caseProd.Price:C}, stock={caseProd.Stock})");
Console.WriteLine($"Customer: {alice.Name} <{alice.Email}>\n");

DateTime now = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

Order order1 = new(alice, now);
Console.WriteLine($"Created order #{order1.OrderId} for {order1.Customer.Name} on {order1.OrderDate:yyyy-MM-dd}, status={order1.Status}");

order1.AddPosition(phone, 2);
order1.AddPosition(caseProd, 5);
Console.WriteLine($"Order #{order1.OrderId} now has {order1.Positions.Count} positions, total {order1.TotalAmount:C}");
Console.WriteLine($"  - #{order1.Positions[0].PositionNumber}: {order1.Positions[0].Product.Name} x {order1.Positions[0].Quantity} @ {order1.Positions[0].HistoricalUnitPrice:C} = {order1.Positions[0].LineTotal:C}");
Console.WriteLine($"  - #{order1.Positions[1].PositionNumber}: {order1.Positions[1].Product.Name} x {order1.Positions[1].Quantity} @ {order1.Positions[1].HistoricalUnitPrice:C} = {order1.Positions[1].LineTotal:C}");
Console.WriteLine($"  phone stock now: {phone.Stock}, needs reorder: {phone.NeedsReorder()}");

phone.UpdatePrice(649.00m);
Console.WriteLine($"\nPhone price changed to {phone.Price:C} (historical price in order preserved: {order1.Positions[0].HistoricalUnitPrice:C})");

Order order2 = new(alice, now.AddDays(1));
Console.WriteLine($"\nCreated order #{order2.OrderId}, status={order2.Status}");
order2.AddPosition(phone, 1);
Console.WriteLine($"Order #{order2.OrderId} total: {order2.TotalAmount:C} (uses NEW price {phone.Price:C})");

try
{
    order2.AddPosition(phone, 100);
}
catch (InvalidStockException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

order1.MarkShipped();
Console.WriteLine($"\nOrder #{order1.OrderId} marked shipped, status={order1.Status}");

try
{
    order1.Cancel();
}
catch (OrderNotCancellableException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

Order order3 = new(alice, now.AddDays(2));
order3.AddPosition(phone, 3);
Console.WriteLine($"\nCreated order #{order3.OrderId}, phone stock now: {phone.Stock}");
Console.WriteLine($"  needs reorder: {phone.NeedsReorder()}, supplier orders for phone: {phone.SupplierOrders.Count}");
Console.WriteLine($"  last supplier order: #{phone.SupplierOrders[^1].SupplierOrderId} qty={phone.SupplierOrders[^1].Quantity} on {phone.SupplierOrders[^1].OrderDate:yyyy-MM-dd}");

order3.Cancel();
Console.WriteLine($"\nOrder #{order3.OrderId} cancelled, phone stock after refund: {phone.Stock}, status={order3.Status}");

Console.WriteLine("\n=== Demo done ===");