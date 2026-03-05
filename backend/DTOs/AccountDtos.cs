using System.ComponentModel.DataAnnotations;

namespace UpiFraudApi.DTOs;

public record RequestOtpRequest(string PhoneNumber);
public record VerifyOtpRequest(string PhoneNumber, string OtpCode, [Required] string FaceImageBase64, string? CreateUpiPin, string? ConfirmUpiPin);
public record BalanceRequest(string UpiPin);
