namespace Bank;
public class BankClass
{
    public int BankId { get; set; }
    public string BankName { get; set; }
    public string Address { get; set; }
    public int PhoneNumber { get; set; }
    public int RoutingNumber { get; set; }

    public BankClass(int bankId, string bankName, string address, int phoneNumber, int routingNumber)
    {
        BankId = bankId;
        BankName = bankName;
        Address = address;
        PhoneNumber = phoneNumber;
        RoutingNumber = routingNumber;
    }
}