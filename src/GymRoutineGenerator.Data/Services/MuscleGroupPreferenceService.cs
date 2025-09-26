using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public class MuscleGroupPreferenceService : IMuscleGroupPreferenceService
{
    private readonly GymRoutineContext _context;

    public MuscleGroupPreferenceService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<List<UserMuscleGroupPreference>> SetUserMuscleGroupPreferencesAsync(int userProfileId, List<MuscleGroupPreferenceRequest> preferences)
    {
        // Validate user profile exists
        var userProfile = await _context.UserProfiles.FindAsync(userProfileId);
        if (userProfile == null)
        {
            throw new ArgumentException($"Perfil de usuario con ID {userProfileId} no encontrado");
        }

        // Validate muscle groups exist
        var muscleGroupIds = preferences.Select(p => p.MuscleGroupId).ToList();
        var validMuscleGroups = await _context.MuscleGroups
            .Where(mg => muscleGroupIds.Contains(mg.Id))
            .ToListAsync();

        var invalidIds = muscleGroupIds.Except(validMuscleGroups.Select(mg => mg.Id)).ToList();
        if (invalidIds.Any())
        {
            throw new ArgumentException($"Grupos musculares no válidos: {string.Join(", ", invalidIds)}");
        }

        // Validate emphasis levels
        foreach (var pref in preferences)
        {
            if (!Enum.IsDefined(typeof(EmphasisLevel), pref.EmphasisLevel))
            {
                throw new ArgumentException($"Nivel de énfasis no válido: {pref.EmphasisLevel}");
            }
        }

        // Validate that not too many groups have high emphasis (max 3)
        var highEmphasisCount = preferences.Count(p => p.EmphasisLevel == EmphasisLevel.Alto);
        if (highEmphasisCount > 3)
        {
            throw new ArgumentException("No puede tener énfasis alto en más de 3 grupos musculares");
        }

        // Clear existing preferences
        await ClearUserMuscleGroupPreferencesAsync(userProfileId);

        // Create new preferences
        var newPreferences = preferences.Select(pref => new UserMuscleGroupPreference
        {
            UserProfileId = userProfileId,
            MuscleGroupId = pref.MuscleGroupId,
            EmphasisLevel = pref.EmphasisLevel
        }).ToList();

        _context.UserMuscleGroupPreferences.AddRange(newPreferences);
        await _context.SaveChangesAsync();

        // Return preferences with muscle group data
        return await GetUserMuscleGroupPreferencesAsync(userProfileId);
    }

    public async Task<List<UserMuscleGroupPreference>> GetUserMuscleGroupPreferencesAsync(int userProfileId)
    {
        return await _context.UserMuscleGroupPreferences
            .Include(umgp => umgp.MuscleGroup)
            .Where(umgp => umgp.UserProfileId == userProfileId)
            .OrderByDescending(umgp => umgp.EmphasisLevel)
            .ThenBy(umgp => umgp.MuscleGroup.SpanishName)
            .ToListAsync();
    }

    public async Task<List<MuscleGroup>> GetAllMuscleGroupsAsync()
    {
        return await _context.MuscleGroups
            .OrderBy(mg => mg.SpanishName)
            .ToListAsync();
    }

    public async Task<bool> ClearUserMuscleGroupPreferencesAsync(int userProfileId)
    {
        var existingPreferences = await _context.UserMuscleGroupPreferences
            .Where(umgp => umgp.UserProfileId == userProfileId)
            .ToListAsync();

        if (existingPreferences.Any())
        {
            _context.UserMuscleGroupPreferences.RemoveRange(existingPreferences);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<TrainingObjectiveTemplate> ApplyTrainingObjectiveTemplateAsync(int userProfileId, TrainingObjectiveType objectiveType)
    {
        var template = GetTrainingObjectiveTemplate(objectiveType);

        // Apply the template preferences
        await SetUserMuscleGroupPreferencesAsync(userProfileId, template.MuscleGroupPreferences);

        return template;
    }

    private static TrainingObjectiveTemplate GetTrainingObjectiveTemplate(TrainingObjectiveType objectiveType)
    {
        return objectiveType switch
        {
            TrainingObjectiveType.WeightLoss => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.WeightLoss,
                Name = "Pérdida de Peso",
                Description = "Enfoque en ejercicios de cuerpo completo y alta quema calórica",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 8, EmphasisLevel = EmphasisLevel.Alto },    // Cuerpo Completo
                    new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Alto },    // Core
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
                    new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio }    // Glúteos
                }
            },

            TrainingObjectiveType.MuscleGain => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.MuscleGain,
                Name = "Ganancia Muscular",
                Description = "Enfoque en grupos musculares principales para hipertrofia",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Alto },    // Pecho
                    new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },    // Espalda
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Alto },    // Piernas
                    new() { MuscleGroupId = 3, EmphasisLevel = EmphasisLevel.Medio },   // Hombros
                    new() { MuscleGroupId = 4, EmphasisLevel = EmphasisLevel.Medio },   // Brazos
                    new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio }    // Glúteos
                }
            },

            TrainingObjectiveType.GeneralFitness => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.GeneralFitness,
                Name = "Fitness General",
                Description = "Desarrollo equilibrado de todos los grupos musculares",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Medio },   // Pecho
                    new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Medio },   // Espalda
                    new() { MuscleGroupId = 3, EmphasisLevel = EmphasisLevel.Medio },   // Hombros
                    new() { MuscleGroupId = 4, EmphasisLevel = EmphasisLevel.Medio },   // Brazos
                    new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Medio },   // Core
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
                    new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio },   // Glúteos
                    new() { MuscleGroupId = 8, EmphasisLevel = EmphasisLevel.Medio }    // Cuerpo Completo
                }
            },

            TrainingObjectiveType.Strength => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.Strength,
                Name = "Fuerza",
                Description = "Enfoque en movimientos compuestos y fuerza funcional",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },    // Espalda
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Alto },    // Piernas
                    new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Alto },    // Core
                    new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Medio },   // Pecho
                    new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio }    // Glúteos
                }
            },

            TrainingObjectiveType.Endurance => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.Endurance,
                Name = "Resistencia",
                Description = "Enfoque en resistencia cardiovascular y muscular",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 8, EmphasisLevel = EmphasisLevel.Alto },    // Cuerpo Completo
                    new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Medio },   // Core
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
                    new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Bajo }     // Espalda
                }
            },

            TrainingObjectiveType.Mobility => new TrainingObjectiveTemplate
            {
                ObjectiveType = TrainingObjectiveType.Mobility,
                Name = "Movilidad",
                Description = "Enfoque en flexibilidad y rango de movimiento",
                MuscleGroupPreferences = new List<MuscleGroupPreferenceRequest>
                {
                    new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Alto },    // Core
                    new() { MuscleGroupId = 3, EmphasisLevel = EmphasisLevel.Medio },   // Hombros
                    new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
                    new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio },   // Glúteos
                    new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Bajo }     // Espalda
                }
            },

            _ => throw new ArgumentException($"Tipo de objetivo de entrenamiento no válido: {objectiveType}")
        };
    }
}