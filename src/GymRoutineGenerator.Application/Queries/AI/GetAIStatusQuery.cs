using MediatR;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Domain.Services;

namespace GymRoutineGenerator.Application.Queries.AI;

/// <summary>
/// Query to get the current status of the AI service
/// </summary>
public record GetAIStatusQuery : IRequest<Result<AIStatusDto>>
{
    // No parameters needed - just checks current status
}

/// <summary>
/// Handler for querying AI service status
/// </summary>
public class GetAIStatusQueryHandler
    : IRequestHandler<GetAIStatusQuery, Result<AIStatusDto>>
{
    private readonly IWorkoutPlanGenerationService _workoutPlanService;

    public GetAIStatusQueryHandler(IWorkoutPlanGenerationService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService ?? throw new ArgumentNullException(nameof(workoutPlanService));
    }

    public async Task<Result<AIStatusDto>> Handle(
        GetAIStatusQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if AI is available
            var isAvailable = await _workoutPlanService.IsAIAvailableAsync(cancellationToken);

            // Get detailed status information
            var statusInfo = await _workoutPlanService.GetAIStatusAsync(cancellationToken);

            // Build the DTO
            var dto = new AIStatusDto
            {
                IsAvailable = isAvailable,
                StatusMessage = statusInfo ?? "Estado desconocido",
                ModelName = "Mistral 7B",
                IsOnline = isAvailable,
                Details = isAvailable
                    ? "El servicio de IA está funcionando correctamente y listo para generar rutinas personalizadas."
                    : "El servicio de IA no está disponible. Usando generador de rutinas básico como alternativa."
            };

            return Result<AIStatusDto>.Success(dto);
        }
        catch (Exception ex)
        {
            // Even if there's an error, return a DTO with offline status
            var offlineDto = new AIStatusDto
            {
                IsAvailable = false,
                StatusMessage = "Error al verificar el estado del servicio",
                ModelName = "Mistral 7B",
                IsOnline = false,
                Details = $"Error: {ex.Message}"
            };

            return Result<AIStatusDto>.Success(offlineDto);
        }
    }
}
