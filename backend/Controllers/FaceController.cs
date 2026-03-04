using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.DTOs;
using UpiFraudApi.Entities;
using UpiFraudApi.Services;

namespace UpiFraudApi.Controllers;

[ApiController]
[Route("api/face")]
[Authorize]
public class FaceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FaceVerificationService _faceService;

    public FaceController(AppDbContext db, FaceVerificationService faceService)
    {
        _db = db;
        _faceService = faceService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(FaceRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            return BadRequest("ImageBase64 is required");
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _faceService.RegisterEmbeddingAsync(request.ImageBase64);

        var embeddingBytes = FaceVerificationService.Base64ToBytes(result.EmbeddingBase64);
        var existing = await _db.UserFaceEmbeddings.FirstOrDefaultAsync(x => x.UserId == userId);

        if (existing == null)
        {
            _db.UserFaceEmbeddings.Add(new UserFaceEmbedding
            {
                UserId = userId,
                Embedding = embeddingBytes,
                EmbeddingSize = result.EmbeddingSize,
                ModelVersion = result.ModelVersion
            });
        }
        else
        {
            existing.Embedding = embeddingBytes;
            existing.EmbeddingSize = result.EmbeddingSize;
            existing.ModelVersion = result.ModelVersion;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Face embedding registered", result.EmbeddingSize, result.ModelVersion });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(FaceVerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            return BadRequest("ImageBase64 is required");
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var stored = await _db.UserFaceEmbeddings.FirstOrDefaultAsync(x => x.UserId == userId);
        if (stored == null)
        {
            return BadRequest("Face not registered for user");
        }

        var result = await _faceService.VerifyFaceAsync(request.ImageBase64, stored.Embedding);
        return Ok(result);
    }
}
