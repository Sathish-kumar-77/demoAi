namespace UpiFraudApi.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string UpiId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double FraudProbability { get; set; }
    public bool IsFraud { get; set; }
    public string Prediction { get; set; } = "Safe";
    public string Status { get; set; } = "Completed";
    public string Reasons { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
