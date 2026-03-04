using System.ComponentModel.DataAnnotations;

namespace UpiFraudApi.DTOs;

public record RequestOtpRequest(string PhoneNumber, string UpiPin);
public record VerifyOtpRequest(string PhoneNumber, string OtpCode, [Required] string FaceImageBase64);
public record BalanceRequest(string UpiPin);
