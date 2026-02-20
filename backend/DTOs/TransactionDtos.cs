namespace UpiFraudApi.DTOs;

public record PayRequest(string UpiId, decimal Amount, string Note, string DeviceId, string City);

public record PayResponse(
    int TransactionId,
    bool IsFraud,
    double FraudProbability,
    List<string> Reasons
);
