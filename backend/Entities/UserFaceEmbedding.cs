namespace UpiFraudApi.Entities;

public class UserFaceEmbedding
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public byte[] Embedding { get; set; } = Array.Empty<byte>();
    public string ModelVersion { get; set; } = "arcface-onnx-v1";
    public int EmbeddingSize { get; set; } = 512;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
