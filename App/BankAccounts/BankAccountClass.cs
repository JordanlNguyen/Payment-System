namespace BankAccount;
public class BankAccountClass
{
    public int AccountId { get; set; }
    public int PhoneNumber { get; set; }
    public int AccountNumber { get; set; }
    public string Currency { get; set; }
    public decimal Balance { get; set; }

    public BankAccountClass(int accountId, int phoneNumber, int accountNumber, string currency, decimal balance)
    {
        AccountId = accountId;
        this.PhoneNumber = phoneNumber;
        AccountNumber = accountNumber;
        Currency = currency;
        Balance = balance;
    }

    public int Deposit(decimal amount)
    {
        Balance += amount;
        return AccountId;
    }

    public void Withdraw(decimal amount)
    {
        if (IsEnough(amount))
        {
            Balance -= amount;
        }
    }

    public bool IsEnough(decimal amount)
    {
        if (Balance >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }        
    }
}
