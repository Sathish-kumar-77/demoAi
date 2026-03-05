namespace UpiFraudApi.Entities;

public class BankAccount
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AccountNumberMasked { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string UpiPin { get; set; } = "1234";

    public List<UserLinkedAccount> LinkedUsers { get; set; } = new();
}
