using System.Threading;
using System.Threading.Tasks;
using GymRoutineGenerator.Application.Commands.WorkoutPlans;
using GymRoutineGenerator.Application.DTOs;
using GymRoutineGenerator.Application.Queries.Exercises;
using MediatR;

namespace GymRoutineGenerator.Services;

/// <summary>
/// Servicio que encapsula el uso de MediatR para comandos y queries de rutinas.
/// Los formularios WinForms/WinUI pueden inyectar este servicio para usar CQRS.
/// </summary>
public class RoutineCommandService
{
    private readonly IMediator _mediator;

    public RoutineCommandService(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un nuevo plan de entrenamiento usando CQRS
    /// </summary>
    public async Task<WorkoutPlanDto?> CreateWorkoutPlanAsync(
        string name,
        string userName,
        int userAge,
        string gender,
        string userLevel,
        int trainingDaysPerWeek,
        string? description = null,
        List<string>? userLimitations = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateWorkoutPlanCommand(
            name,
            userName,
            userAge,
            gender,
            userLevel,
            trainingDaysPerWeek,
            description,
            userLimitations
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return result.Value;
        }

        // En caso de error, podrías lanzar excepción o retornar null
        // Por ahora retornamos null
        Console.WriteLine($"Error creating workout plan: {result.Error}");
        return null;
    }

    /// <summary>
    /// Obtiene todos los ejercicios activos usando CQRS
    /// </summary>
    public async Task<List<ExerciseDto>> GetActiveExercisesAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetActiveExercisesQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.ToList();
        }

        return new List<ExerciseDto>();
    }

    /// <summary>
    /// Obtiene ejercicios por grupo muscular usando CQRS
    /// </summary>
    public async Task<List<ExerciseDto>> GetExercisesByMuscleGroupAsync(
        string muscleGroupName,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExercisesByMuscleGroupQuery(muscleGroupName);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.ToList();
        }

        return new List<ExerciseDto>();
    }
}
