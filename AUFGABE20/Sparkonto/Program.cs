using Fbs.Lg2;

var account = new Sparkonto("Anna Müller", -50m);
Console.WriteLine($"Created: {account.AccountHolder}, # {account.AccountNumber}, IBAN {account.IBAN}, balance {account.Balance:C}");

if (account.Deposit(250m, out var ok1))
{
    Console.WriteLine($"[OK] {ok1}");
}
Console.WriteLine($"  -> balance: {account.Balance:C}");

if (!account.Deposit(-10m, out var err1))
{
    Console.WriteLine($"[REJECTED] {err1}");
}
Console.WriteLine($"  -> balance unchanged: {account.Balance:C}");

if (account.Withdraw(75m, out var ok2))
{
    Console.WriteLine($"[OK] {ok2}");
}
Console.WriteLine($"  -> balance: {account.Balance:C}");

if (!account.Withdraw(10_000m, out var err2))
{
    Console.WriteLine($"[REJECTED] {err2}");
}
Console.WriteLine($"  -> balance unchanged: {account.Balance:C}");

if (account.UpdateName("Anna Schmidt", out var ok3))
{
    Console.WriteLine($"[OK] {ok3}");
}
Console.WriteLine($"  -> current holder: {account.AccountHolder}");

if (!account.UpdateName("   ", out var err3))
{
    Console.WriteLine($"[REJECTED] {err3}");
}

Console.WriteLine($"\nFinal: holder='{account.AccountHolder}', balance={account.Balance:C}");