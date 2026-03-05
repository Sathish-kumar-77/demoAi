namespace UpiFraudApi.Entities;

public class UserLinkedAccount
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public string UpiId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
