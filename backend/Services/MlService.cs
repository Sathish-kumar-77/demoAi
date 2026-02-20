using System.Net.Http.Json;

namespace UpiFraudApi.Services;

public class MlService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MlService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<MlResponse> PredictAsync(Dictionary<string, double> features)
    {
        var baseUrl = _config["MlService:BaseUrl"] ?? "http://localhost:8000";
        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/predict", new MlRequest(features));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MlResponse>()
               ?? new MlResponse(0.0, false);
    }
}

public record MlRequest(Dictionary<string, double> Features);
public record MlResponse(double FraudProbability, bool IsFraud);
