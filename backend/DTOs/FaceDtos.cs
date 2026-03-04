using System.ComponentModel.DataAnnotations;

namespace UpiFraudApi.DTOs;

public record FaceRegisterRequest([Required] string ImageBase64);
public record FaceVerifyRequest([Required] string ImageBase64);
