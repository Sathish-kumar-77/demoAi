namespace UpiFraudApi.Entities;

public class Payee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string UpiId { get; set; } = string.Empty;
    public bool Verified { get; set; }
}
