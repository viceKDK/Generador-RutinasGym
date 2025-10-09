using System;
using System.Linq;
using System.Text;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Services;
using MediatR;

namespace GymRoutineGenerator.Application.Commands.WorkoutPlans;

/// <summary>
/// Handler for generating complete workout plans with exercises
/// </summary>
public class GenerateWorkoutPlanCommandHandler
    : IRequestHandler<GenerateWorkoutPlanCommand, Result<GenerateWorkoutPlanResult>>
{
    private readonly IWorkoutPlanGenerationService _workoutPlanService;

    public GenerateWorkoutPlanCommandHandler(IWorkoutPlanGenerationService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService ?? throw new ArgumentNullException(nameof(workoutPlanService));
    }

    public async Task<Result<GenerateWorkoutPlanResult>> Handle(
        GenerateWorkoutPlanCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var generationResult = await _workoutPlanService.GenerateWorkoutPlanAsync(
                request.UserName,
                request.Age,
                request.Gender,
                request.FitnessLevel,
                request.TrainingDays,
                request.Goals,
                cancellationToken);

            if (generationResult == null || generationResult.WorkoutPlan == null)
            {
                return Result.Failure<GenerateWorkoutPlanResult>("No se pudo generar la rutina. Por favor, intente nuevamente.");
            }

            var routineText = !string.IsNullOrWhiteSpace(generationResult.RoutineText)
                ? generationResult.RoutineText
                : BuildRoutineNarrative(generationResult.WorkoutPlan);

            var response = new GenerateWorkoutPlanResult
            {
                RoutineText = routineText,
                WorkoutPlan = generationResult.WorkoutPlan
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<GenerateWorkoutPlanResult>($"Error al generar plan de entrenamiento: {ex.Message}");
        }
    }

    private static string BuildRoutineNarrative(WorkoutPlan workoutPlan)
    {
        var text = new StringBuilder();

        text.AppendLine($"Plan de Entrenamiento: {workoutPlan.Name}");
        text.AppendLine($"Usuario: {workoutPlan.UserName}");
        text.AppendLine($"Edad: {workoutPlan.UserAge} años");
        text.AppendLine($"Género: {workoutPlan.Gender}");
        text.AppendLine($"Nivel: {workoutPlan.UserLevel.Name}");
        text.AppendLine($"Días por semana: {workoutPlan.TrainingDaysPerWeek}");
        text.AppendLine($"Fecha de creación: {workoutPlan.CreatedAt:dd/MM/yyyy}");
        text.AppendLine();

        foreach (var routine in workoutPlan.Routines)
        {
            text.AppendLine($"=== {routine.Name} - Día {routine.DayNumber} ===");
            if (!string.IsNullOrWhiteSpace(routine.Description))
            {
                text.AppendLine($"Descripción: {routine.Description}");
            }
            text.AppendLine();

            foreach (var routineExercise in routine.Exercises)
            {
                var exercise = routineExercise.Exercise;
                text.AppendLine($"  • {exercise.Name}");

                var primaryMuscle = exercise.TargetMuscles.FirstOrDefault();
                if (primaryMuscle != null)
                {
                    text.AppendLine($"    Grupo muscular: {primaryMuscle.SpanishName}");
                }

                text.AppendLine($"    Equipamiento: {exercise.Equipment.SpanishName}");
                text.AppendLine($"    Nivel: {exercise.Difficulty.Name}");
                text.AppendLine($"    Series: {routineExercise.Sets.Count}");

                if (routineExercise.Sets.Any())
                {
                    var repsInfo = string.Join(", ", routineExercise.Sets.Select(s => $"{s.Repetitions} reps"));
                    text.AppendLine($"    Repeticiones: {repsInfo}");
                }

                if (!string.IsNullOrWhiteSpace(routineExercise.Notes))
                {
                    text.AppendLine($"    Notas: {routineExercise.Notes}");
                }

                text.AppendLine();
            }

            text.AppendLine();
        }

        return text.ToString();
    }
}
