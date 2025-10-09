using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Infrastructure.Documents;
using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Infrastructure.Services.Adapters;

/// <summary>
/// Adapter that implements Domain document export service using Infrastructure service
/// </summary>
public class DocumentExportDomainServiceAdapter : IDocumentExportDomainService
{
    private readonly IDocumentExportService _documentExportService;

    public DocumentExportDomainServiceAdapter(IDocumentExportService documentExportService)
    {
        _documentExportService = documentExportService ?? throw new ArgumentNullException(nameof(documentExportService));
    }

    public async Task<bool> ExportToWordAsync(
        string userName,
        WorkoutPlan workoutPlan,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        // Convert Domain WorkoutPlan to legacy WorkoutDay list
        var workoutDays = ConvertToWorkoutDays(workoutPlan);

        // Create UserRoutineParameters from WorkoutPlan
        var routineParameters = new UserRoutineParameters
        {
            Name = userName,
            Age = workoutPlan.UserAge,
            Gender = workoutPlan.Gender,
            TrainingDaysPerWeek = workoutPlan.TrainingDaysPerWeek,
            ExperienceLevel = workoutPlan.UserLevel.Name,
            FitnessLevel = workoutPlan.UserLevel.Name,
            TrainingDays = workoutPlan.TrainingDaysPerWeek,
            PrimaryGoal = "Fitness general", // WorkoutPlan doesn't store goals directly
            Goals = new List<string>() // WorkoutPlan doesn't store goals directly
        };

        // ✅ USE EXPORT WITH IMAGES - includes exercise images from database
        var success = await _documentExportService.ExportToWordWithImagesAsync(
            outputPath,
            workoutDays,
            routineParameters
        );

        return success;
    }

    public async Task<bool> ExportToPDFAsync(
        string userName,
        WorkoutPlan workoutPlan,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        // For PDF, use text-based export (legacy method)
        // PDF export with images requires different implementation
        var routineText = GenerateRoutineText(workoutPlan);

        var success = await _documentExportService.ExportToPDFAsync(
            outputPath,
            routineText,
            userName
        );

        return success;
    }

    private List<WorkoutDay> ConvertToWorkoutDays(WorkoutPlan workoutPlan)
    {
        var workoutDays = new List<WorkoutDay>();

        foreach (var routine in workoutPlan.Routines)
        {
            var workoutDay = new WorkoutDay
            {
                Name = routine.Name,
                FocusAreas = routine.Description ?? "Entrenamiento general",
                DayNumber = routine.DayNumber,
                Exercises = new List<GymRoutineGenerator.Core.Models.Exercise>()
            };

            foreach (var routineExercise in routine.Exercises)
            {
                var domainExercise = routineExercise.Exercise;
                var legacyExercise = new GymRoutineGenerator.Core.Models.Exercise
                {
                    Name = domainExercise.Name,
                    SpanishName = domainExercise.Name, // Domain Exercise doesn't have SpanishName
                    Description = domainExercise.Description ?? string.Empty,
                    // Map sets/reps information from RoutineExercise
                    RecommendedSets = routineExercise.Sets.Count,
                    RecommendedReps = routineExercise.Sets.FirstOrDefault()?.Repetitions.ToString() ?? "10"
                };

                workoutDay.Exercises.Add(legacyExercise);
            }

            workoutDays.Add(workoutDay);
        }

        return workoutDays;
    }

    private string GenerateRoutineText(WorkoutPlan workoutPlan)
    {
        var text = new System.Text.StringBuilder();

        text.AppendLine($"Plan de Entrenamiento: {workoutPlan.Name}");
        text.AppendLine($"Usuario: {workoutPlan.UserName}");
        text.AppendLine($"Edad: {workoutPlan.UserAge} años");
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

                // Get primary muscle group from TargetMuscles collection
                var primaryMuscle = exercise.TargetMuscles.FirstOrDefault();
                if (primaryMuscle != null)
                {
                    text.AppendLine($"    Grupo muscular: {primaryMuscle.SpanishName}");
                }

                text.AppendLine($"    Equipamiento: {exercise.Equipment.SpanishName}");
                text.AppendLine($"    Nivel: {exercise.Difficulty.Name}");
                text.AppendLine($"    Series: {routineExercise.Sets.Count}");

                // Add reps information from sets
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
