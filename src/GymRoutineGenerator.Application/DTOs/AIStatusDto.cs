namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// Data Transfer Object for AI service status information
/// </summary>
public record AIStatusDto
{
    /// <summary>
    /// Indicates whether the AI service is available
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Detailed status message from the AI service
    /// </summary>
    public string StatusMessage { get; init; } = string.Empty;

    /// <summary>
    /// Name of the AI model being used (e.g., "Mistral 7B")
    /// </summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates if the AI service is online and responding
    /// </summary>
    public bool IsOnline { get; init; }

    /// <summary>
    /// Additional details about the service status
    /// </summary>
    public string? Details { get; init; }
}
