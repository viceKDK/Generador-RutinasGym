using GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Infrastructure.AI;

public interface IOllamaService
{
    Task<string> GenerateRoutineAsync(string prompt, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    Task<OllamaHealth> GetHealthStatusAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}