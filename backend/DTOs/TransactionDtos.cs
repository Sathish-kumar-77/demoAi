namespace UpiFraudApi.DTOs;

public record PayRequest(
    string PayeeUpiId,
    string PayeePhone,
    decimal Amount,
    string Remark,
    string DeviceId,
    int HourOfDay,
    string Channel,
    string? FaceImageBase64
);

public record PayResponse(
    int TransactionId,
    bool IsFraud,
    string Prediction,
    double FraudProbability,
    List<string> Reasons,
    string Status
);
