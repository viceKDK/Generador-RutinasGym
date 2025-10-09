using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Domain.Services;

/// <summary>
/// Domain service for document export operations
/// </summary>
public interface IDocumentExportDomainService
{
    /// <summary>
    /// Exports a workout plan to Word document
    /// </summary>
    Task<bool> ExportToWordAsync(
        string userName,
        WorkoutPlan workoutPlan,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a workout plan to PDF document
    /// </summary>
    Task<bool> ExportToPDFAsync(
        string userName,
        WorkoutPlan workoutPlan,
        string outputPath,
        CancellationToken cancellationToken = default);
}
