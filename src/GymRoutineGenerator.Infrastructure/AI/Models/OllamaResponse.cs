namespace GymRoutineGenerator.Infrastructure.AI.Models;

public class OllamaResponse
{
    public string Model { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public bool Done { get; set; }
    public DateTime Created_at { get; set; }
    public OllamaContext Context { get; set; } = new();
}

public class OllamaContext
{
    public int[] Context { get; set; } = Array.Empty<int>();
}