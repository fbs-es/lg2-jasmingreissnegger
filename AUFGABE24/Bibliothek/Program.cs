using Fbs.Lg2;

Console.WriteLine("=== Bibliothek Demo ===\n");

Library library = new("Stadtbibliothek", "Hauptstrasse 1, 1010 Wien");
Console.WriteLine($"Library: {library.Name}, {library.Address}");

Book book1 = new("978-3-16-148410-0", "Clean Code", "Robert C. Martin");
Book book2 = new("978-0-13-235088-4", "Clean Architecture", "Robert C. Martin");
library.AddBook(book1);
library.AddBook(book2);
Console.WriteLine($"Added {library.BookCount} books to inventory.");

User anna = new("Anna Mueller", "anna@example.com", new DateTime(2026, 1, 15));
User bob = new("Bob Schmidt", "bob@example.com", new DateTime(2026, 1, 16));
library.RegisterUser(anna);
library.RegisterUser(bob);
Console.WriteLine($"Registered users: {anna.Name} (#{anna.UserId}), {bob.Name} (#{bob.UserId})\n");

DateTime loanDate = new DateTime(2026, 1, 20);
DateTime plannedReturn = new DateTime(2026, 2, 3);

Loan loan = library.StartLoan(anna, book1, loanDate, plannedReturn);
Console.WriteLine($"Anna borrows '{book1.Title}', planned return: {loan.PlannedReturnDate:yyyy-MM-dd}, status: {loan.Status}");
Console.WriteLine($"  Book availability: {book1.Availability}");

DateTime extendedReturn = new DateTime(2026, 2, 17);
loan.ExtendLoan(extendedReturn);
Console.WriteLine($"\nExtended loan. New planned return: {loan.PlannedReturnDate:yyyy-MM-dd}, extended: {loan.Extended}");

try
{
    loan.ExtendLoan(new DateTime(2026, 3, 1));
}
catch (LoanExtensionException ex)
{
    Console.WriteLine($"[REJECTED] {ex.Message}");
}

DateTime actualReturn = new DateTime(2026, 2, 20);
decimal fee = loan.ProcessReturn(actualReturn);
Console.WriteLine($"\nReturn on {actualReturn:yyyy-MM-dd}: status={loan.Status}, late fee={fee:C}");
Console.WriteLine($"  Book availability: {book1.Availability}");

Loan loan2 = library.StartLoan(bob, book2, loanDate, plannedReturn);
DateTime onTimeReturn = new DateTime(2026, 2, 1);
decimal fee2 = loan2.ProcessReturn(onTimeReturn);
Console.WriteLine($"\nBob returns '{book2.Title}' on time: status={loan2.Status}, fee={fee2:C}");

Console.WriteLine($"\nFinal library state: {library.BookCount} books, {library.UserCount} users, {library.LoanCount} loans processed ({library.ActiveLoanCount} active).");

Console.WriteLine("\n=== Demo done ===");