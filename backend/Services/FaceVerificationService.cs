using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace UpiFraudApi.Services;

public class FaceVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public FaceVerificationService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<FaceRegisterResponse> RegisterEmbeddingAsync(string imageBase64)
    {
        var baseUrl = _config["FaceService:BaseUrl"] ?? "http://localhost:8010";
        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/face/register", new { image_base64 = imageBase64 });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FaceRegisterResponse>()
               ?? throw new InvalidOperationException("Invalid face register response");
    }

    public async Task<FaceVerifyResponse> VerifyFaceAsync(string imageBase64, byte[] storedEmbedding)
    {
        var baseUrl = _config["FaceService:BaseUrl"] ?? "http://localhost:8010";
        var threshold = double.TryParse(_config["FaceService:MatchThreshold"], out var t) ? t : 0.45;
        var storedEmbeddingBase64 = Convert.ToBase64String(storedEmbedding);

        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/face/verify", new
        {
            image_base64 = imageBase64,
            stored_embedding_base64 = storedEmbeddingBase64,
            threshold
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FaceVerifyResponse>()
               ?? throw new InvalidOperationException("Invalid face verify response");
    }

    public static byte[] Base64ToBytes(string base64) => Convert.FromBase64String(base64);

    public static double CosineSimilarity(byte[] embA, byte[] embB)
    {
        var a = BytesToFloatArray(embA);
        var b = BytesToFloatArray(embB);
        if (a.Length != b.Length || a.Length == 0) return 0;

        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static float[] BytesToFloatArray(byte[] bytes)
    {
        var count = bytes.Length / sizeof(float);
        var result = new float[count];
        Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
        return result;
    }
}

public record FaceRegisterResponse(
    [property: JsonPropertyName("embedding_base64")] string EmbeddingBase64,
    [property: JsonPropertyName("embedding_size")] int EmbeddingSize,
    [property: JsonPropertyName("model_version")] string ModelVersion
);

public record FaceVerifyResponse(
    [property: JsonPropertyName("similarity_score")] double SimilarityScore,
    [property: JsonPropertyName("match")] bool Match,
    [property: JsonPropertyName("threshold")] double Threshold,
    [property: JsonPropertyName("model_version")] string ModelVersion
);
