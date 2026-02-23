namespace UpiFraudApi.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";

    public int? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}
