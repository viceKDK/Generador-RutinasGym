using MediatR;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Domain.Services;

namespace GymRoutineGenerator.Application.Commands.WorkoutPlans;

/// <summary>
/// Command to generate an alternative workout routine for a user
/// </summary>
public record GenerateAlternativeRoutineCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string FitnessLevel { get; init; } = string.Empty;
    public int TrainingDays { get; init; }
    public List<string> Goals { get; init; } = new();
}

/// <summary>
/// Handler for generating alternative workout routines
/// </summary>
public class GenerateAlternativeRoutineCommandHandler
    : IRequestHandler<GenerateAlternativeRoutineCommand, Result<string>>
{
    private readonly IWorkoutPlanGenerationService _workoutPlanService;

    public GenerateAlternativeRoutineCommandHandler(IWorkoutPlanGenerationService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService ?? throw new ArgumentNullException(nameof(workoutPlanService));
    }

    public async Task<Result<string>> Handle(
        GenerateAlternativeRoutineCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate alternative routine using the domain service
            var routine = await _workoutPlanService.GenerateAlternativeRoutineTextAsync(
                request.UserName,
                request.Age,
                request.Gender,
                request.FitnessLevel,
                request.TrainingDays,
                request.Goals,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(routine))
            {
                return Result.Failure<string>("No se pudo generar una rutina alternativa. Por favor, intente nuevamente.");
            }

            return Result<string>.Success(routine);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Error al generar rutina alternativa: {ex.Message}");
        }
    }
}
