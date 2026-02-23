namespace UpiFraudApi.DTOs;

public record RequestOtpRequest(string PhoneNumber, string UpiPin);
public record VerifyOtpRequest(string PhoneNumber, string OtpCode);
public record BalanceRequest(string UpiPin);
