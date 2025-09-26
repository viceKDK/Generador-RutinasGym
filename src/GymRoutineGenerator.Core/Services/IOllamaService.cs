namespace GymRoutineGenerator.Core.Services;

public interface IOllamaService
{
    Task<string> GenerateRoutineAsync(string prompt, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task<OllamaHealth> GetHealthStatusAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}

public class OllamaHealth
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> AvailableModels { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
}