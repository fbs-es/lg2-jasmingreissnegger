using System;
using Fbs.Lg2;
using Shouldly;

namespace Fbs.Lg2;

public class BookTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesBook()
    {
        var b = new Book("978-3-16-148410-0", "Clean Code", "Robert C. Martin");

        b.Title.ShouldBe("Clean Code");
        b.Author.ShouldBe("Robert C. Martin");
        b.Isbn.ShouldBe("978-3-16-148410-0");
        b.Availability.ShouldBe(BookAvailability.Verfuegbar);
        b.BookId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Ctor_TrimsWhitespace()
    {
        var b = new Book("  ISBN  ", "  Title  ", "  Author  ");

        b.Isbn.ShouldBe("ISBN");
        b.Title.ShouldBe("Title");
        b.Author.ShouldBe("Author");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidIsbn_Throws(string? isbn)
    {
        Should.Throw<InvalidIsbnException>(() => new Book(isbn!, "Title", "Author"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_InvalidTitle_Throws(string? title)
    {
        Should.Throw<InvalidNameException>(() => new Book("ISBN", title!, "Author"));
    }

    [Fact]
    public void UpdateTitle_ValidTitle_ChangesTitle()
    {
        var b = new Book("ISBN", "Old", "Author");
        b.UpdateTitle("New");
        b.Title.ShouldBe("New");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTitle_Invalid_Throws(string? title)
    {
        var b = new Book("ISBN", "Valid", "Author");
        Should.Throw<InvalidNameException>(() => b.UpdateTitle(title!));
        b.Title.ShouldBe("Valid");
    }

    [Fact]
    public void MarkAsBorrowed_SetsAvailability()
    {
        var b = new Book("ISBN", "Title", "Author");
        b.MarkAsBorrowed();
        b.Availability.ShouldBe(BookAvailability.Ausgeliehen);
    }

    [Fact]
    public void MarkAsAvailable_RestoresAvailability()
    {
        var b = new Book("ISBN", "Title", "Author");
        b.MarkAsBorrowed();
        b.MarkAsAvailable();
        b.Availability.ShouldBe(BookAvailability.Verfuegbar);
    }
}

public class UserTests
{
    [Fact]
    public void Ctor_ValidInputs_CreatesUser()
    {
        var u = new User("Anna", "anna@example.com", new DateTime(2026, 1, 15));

        u.Name.ShouldBe("Anna");
        u.Email.ShouldBe("anna@example.com");
        u.RegistrationDate.ShouldBe(new DateTime(2026, 1, 15));
        u.UserId.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Ctor_InvalidEmail_Throws(string? email)
    {
        Should.Throw<InvalidEmailException>(() => new User("Bob", email!, new DateTime(2026, 1, 1)));
    }

    [Fact]
    public void UpdateName_ValidName_ChangesName()
    {
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        u.UpdateName("Annalena");
        u.Name.ShouldBe("Annalena");
    }

    [Fact]
    public void ReadingProperties_DoesNotMutate()
    {
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 15));
        for (var i = 0; i < 5; i++)
        {
            _ = u.UserId;
            _ = u.Name;
            _ = u.Email;
            _ = u.RegistrationDate;
        }
        u.Name.ShouldBe("Anna");
    }
}

public class LibraryTests
{
    private static DateTime Now => new DateTime(2026, 1, 20);
    private static DateTime Return => new DateTime(2026, 2, 3);

    [Fact]
    public void Ctor_ValidInputs_CreatesLibrary()
    {
        var lib = new Library("Stadtbibliothek", "Hauptstrasse 1");

        lib.Name.ShouldBe("Stadtbibliothek");
        lib.BookCount.ShouldBe(0);
        lib.UserCount.ShouldBe(0);
        lib.LoanCount.ShouldBe(0);
    }

    [Fact]
    public void AddBook_AppendsToInventory()
    {
        var lib = new Library("L", "A");
        var b = new Book("ISBN", "Title", "Author");

        lib.AddBook(b);

        lib.BookCount.ShouldBe(1);
        lib.HasBook(b).ShouldBeTrue();
    }

    [Fact]
    public void AddBook_Null_Throws()
    {
        var lib = new Library("L", "A");
        Should.Throw<ArgumentNullException>(() => lib.AddBook(null!));
    }

    [Fact]
    public void RegisterUser_AppendsToUsers()
    {
        var lib = new Library("L", "A");
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));

        lib.RegisterUser(u);

        lib.UserCount.ShouldBe(1);
        lib.HasUser(u).ShouldBeTrue();
    }

    [Fact]
    public void StartLoan_HappyPath_CreatesLoanAndMarksBook()
    {
        var lib = new Library("L", "A");
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        var b = new Book("ISBN", "Title", "Author");
        lib.RegisterUser(u);
        lib.AddBook(b);

        var loan = lib.StartLoan(u, b, Now, Return);

        lib.LoanCount.ShouldBe(1);
        lib.ActiveLoanCount.ShouldBe(1);
        b.Availability.ShouldBe(BookAvailability.Ausgeliehen);
        loan.Status.ShouldBe(LoanStatus.Aktiv);
        loan.User.ShouldBeSameAs(u);
        loan.Book.ShouldBeSameAs(b);
    }

    [Fact]
    public void StartLoan_BookAlreadyBorrowed_Throws()
    {
        var lib = new Library("L", "A");
        var u1 = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        var u2 = new User("Bob", "b@b.com", new DateTime(2026, 1, 2));
        var b = new Book("ISBN", "Title", "Author");
        lib.RegisterUser(u1);
        lib.RegisterUser(u2);
        lib.AddBook(b);

        lib.StartLoan(u1, b, Now, Return);
        Should.Throw<BookNotAvailableException>(() => lib.StartLoan(u2, b, Now, Return));
    }

    [Fact]
    public void StartLoan_UserNotRegistered_Throws()
    {
        var lib = new Library("L", "A");
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        var b = new Book("ISBN", "Title", "Author");
        lib.AddBook(b);

        Should.Throw<InvalidLoanDateException>(() => lib.StartLoan(u, b, Now, Return));
    }

    [Fact]
    public void StartLoan_PlannedReturnBeforeLoan_Throws()
    {
        var lib = new Library("L", "A");
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        var b = new Book("ISBN", "Title", "Author");
        lib.RegisterUser(u);
        lib.AddBook(b);

        Should.Throw<InvalidLoanDateException>(() => lib.StartLoan(u, b, Now, Now));
    }
}

public class LoanTests
{
    private static DateTime LoanDate => new DateTime(2026, 1, 20);
    private static DateTime PlannedReturn => new DateTime(2026, 2, 3);

    private static (Library lib, User user, Book book, Loan loan) NewActiveLoan()
    {
        var lib = new Library("L", "A");
        var u = new User("Anna", "a@b.com", new DateTime(2026, 1, 1));
        var b = new Book("ISBN", "Title", "Author");
        lib.RegisterUser(u);
        lib.AddBook(b);
        var loan = lib.StartLoan(u, b, LoanDate, PlannedReturn);
        return (lib, u, b, loan);
    }

    [Fact]
    public void ExtendLoan_FirstTime_PushesReturnDate()
    {
        var (_, _, _, loan) = NewActiveLoan();
        DateTime newReturn = new DateTime(2026, 2, 17);

        loan.ExtendLoan(newReturn);

        loan.PlannedReturnDate.ShouldBe(newReturn);
        loan.Extended.ShouldBeTrue();
    }

    [Fact]
    public void ExtendLoan_SecondTime_Throws()
    {
        var (_, _, _, loan) = NewActiveLoan();
        loan.ExtendLoan(new DateTime(2026, 2, 17));

        Should.Throw<LoanExtensionException>(() => loan.ExtendLoan(new DateTime(2026, 3, 1)));
    }

    [Fact]
    public void ExtendLoan_NewDateNotAfterCurrent_Throws()
    {
        var (_, _, _, loan) = NewActiveLoan();

        Should.Throw<LoanExtensionException>(() => loan.ExtendLoan(loan.PlannedReturnDate));
    }

    [Fact]
    public void ProcessReturn_OnTime_StatusZurueckgegeben()
    {
        var (_, _, book, loan) = NewActiveLoan();

        var fee = loan.ProcessReturn(new DateTime(2026, 2, 1));

        fee.ShouldBe(0m);
        loan.Status.ShouldBe(LoanStatus.Zurueckgegeben);
        loan.LateFee.ShouldBe(0m);
        book.Availability.ShouldBe(BookAvailability.Verfuegbar);
    }

    [Fact]
    public void ProcessReturn_Overdue_CalculatesLateFee()
    {
        var (_, _, book, loan) = NewActiveLoan();
        DateTime overdueReturn = new DateTime(2026, 2, 5);

        var fee = loan.ProcessReturn(overdueReturn);

        loan.Status.ShouldBe(LoanStatus.Ueberzogen);
        fee.ShouldBe(1.00m);
        loan.LateFee.ShouldBe(1.00m);
        book.Availability.ShouldBe(BookAvailability.Verfuegbar);
    }

    [Fact]
    public void ProcessReturn_DoubleCall_Throws()
    {
        var (_, _, _, loan) = NewActiveLoan();
        loan.ProcessReturn(new DateTime(2026, 2, 1));

        Should.Throw<LoanExtensionException>(() => loan.ProcessReturn(new DateTime(2026, 2, 5)));
    }

    [Fact]
    public void ExtendLoan_AfterReturn_Throws()
    {
        var (_, _, _, loan) = NewActiveLoan();
        loan.ProcessReturn(new DateTime(2026, 2, 1));

        Should.Throw<LoanExtensionException>(() => loan.ExtendLoan(new DateTime(2026, 2, 17)));
    }

    [Fact]
    public void ReadingProperties_DoesNotMutate()
    {
        var (_, _, _, loan) = NewActiveLoan();
        for (var i = 0; i < 5; i++)
        {
            _ = loan.User;
            _ = loan.Book;
            _ = loan.LoanDate;
            _ = loan.PlannedReturnDate;
            _ = loan.ActualReturnDate;
            _ = loan.Extended;
            _ = loan.Status;
            _ = loan.LateFee;
        }
        loan.Status.ShouldBe(LoanStatus.Aktiv);
    }
}