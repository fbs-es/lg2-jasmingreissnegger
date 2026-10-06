namespace Fbs.Lg2;

public sealed class Sparkonto
{
    private const string BankCode = "12345678";
    private const string CountryCode = "DE";

    private static readonly object _lock = new();
    private static int _nextAccountNumber = 1_000_000_000;

    private readonly string _accountNumber;
    private readonly string _iban;
    private string _accountHolder;
    private decimal _balance;

    public string AccountNumber => _accountNumber;
    public string IBAN => _iban;
    public string AccountHolder => _accountHolder;
    public decimal Balance => _balance;

    public Sparkonto(string accountHolder, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new InvalidNameException("Account holder name must not be null, empty, or whitespace.");
        }

        int number;
        lock (_lock)
        {
            number = _nextAccountNumber++;
        }
        _accountNumber = number.ToString("D10");
        _iban = GenerateIBAN(number);

        _accountHolder = accountHolder.Trim();
        _balance = initialBalance < 0m ? 0m : initialBalance;

        CheckInvariants();
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new InvalidAmountException($"Deposit amount must be greater than 0 (received: {amount}).");
        }

        _balance += amount;
        CheckInvariants();
    }

    public bool Deposit(decimal amount, out string message)
    {
        try
        {
            Deposit(amount);
            message = $"Deposit of {amount:C} succeeded.";
            return true;
        }
        catch (InvalidAmountException ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new InvalidAmountException($"Withdrawal amount must be greater than 0 (received: {amount}).");
        }

        if (amount > _balance)
        {
            throw new InsufficientFundsException(amount, _balance);
        }

        _balance -= amount;
        CheckInvariants();
    }

    public bool Withdraw(decimal amount, out string message)
    {
        try
        {
            Withdraw(amount);
            message = $"Withdrawal of {amount:C} succeeded.";
            return true;
        }
        catch (SparkontoException ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidNameException("New name must not be null, empty, or whitespace.");
        }

        _accountHolder = newName.Trim();
        CheckInvariants();
    }

    public bool UpdateName(string newName, out string message)
    {
        try
        {
            UpdateName(newName);
            message = $"Name updated to '{_accountHolder}'.";
            return true;
        }
        catch (InvalidNameException ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public void TransferTo(Sparkonto target, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (ReferenceEquals(target, this))
        {
            throw new SelfTransferException($"Account {_accountNumber} cannot transfer to itself.");
        }

        if (amount <= 0m)
        {
            throw new InvalidAmountException($"Transfer amount must be greater than 0 (received: {amount}).");
        }

        if (_balance < amount)
        {
            throw new InsufficientFundsException(amount, _balance);
        }

        _balance -= amount;
        target._balance += amount;

        CheckInvariants();
        target.CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_balance < 0m)
        {
            throw new InvariantViolationException($"Invariant violated: balance ({_balance}) must not be negative.");
        }
        if (string.IsNullOrWhiteSpace(_accountHolder))
        {
            throw new InvariantViolationException("Invariant violated: account holder must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_accountNumber))
        {
            throw new InvariantViolationException("Invariant violated: account number must not be empty.");
        }
        if (string.IsNullOrWhiteSpace(_iban))
        {
            throw new InvariantViolationException("Invariant violated: IBAN must not be empty.");
        }
    }

    private static string GenerateIBAN(int accountNumber)
    {
        return $"{CountryCode}00{BankCode}{accountNumber:D10}";
    }
}