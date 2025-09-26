using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public class UserParameterMappingService : IUserParameterMappingService
{
    private readonly GymRoutineContext _context;

    public UserParameterMappingService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<UserRoutineParameters> BuildUserParametersAsync(int userProfileId, CancellationToken cancellationToken = default)
    {
        // Load all user data
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.Id == userProfileId, cancellationToken);

        if (userProfile == null)
        {
            throw new ArgumentException($"User profile with ID {userProfileId} not found");
        }

        var equipmentPreferences = await _context.UserEquipmentPreferences
            .Include(ep => ep.EquipmentType)
            .Where(ep => ep.UserProfileId == userProfileId)
            .ToListAsync(cancellationToken);

        var muscleGroupPreferences = await _context.UserMuscleGroupPreferences
            .Include(mgp => mgp.MuscleGroup)
            .Where(mgp => mgp.UserProfileId == userProfileId)
            .ToListAsync(cancellationToken);

        var physicalLimitations = await _context.UserPhysicalLimitations
            .Where(pl => pl.UserProfileId == userProfileId)
            .ToListAsync(cancellationToken);

        return await BuildUserParametersFromEntitiesAsync(
            userProfile,
            equipmentPreferences,
            muscleGroupPreferences,
            physicalLimitations,
            cancellationToken);
    }

    public async Task<UserRoutineParameters> BuildUserParametersFromEntitiesAsync(
        Entities.UserProfile userProfile,
        IEnumerable<UserEquipmentPreference> equipmentPreferences,
        IEnumerable<UserMuscleGroupPreference> muscleGroupPreferences,
        IEnumerable<UserPhysicalLimitation> physicalLimitations,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For future async operations

        var parameters = new UserRoutineParameters
        {
            // Demographics
            Name = userProfile.Name,
            Age = userProfile.Age,
            Gender = MapGender(userProfile.Gender),
            TrainingDaysPerWeek = userProfile.TrainingDaysPerWeek,
            ExperienceLevel = DetermineExperienceLevel(userProfile.Age, userProfile.TrainingDaysPerWeek),

            // Equipment
            AvailableEquipment = MapEquipmentPreferences(equipmentPreferences),
            GymType = DetermineGymType(equipmentPreferences),

            // Goals and Preferences
            PrimaryGoal = DeterminePrimaryGoal(muscleGroupPreferences, userProfile.Age),
            PreferredSessionDuration = CalculateSessionDuration(userProfile.TrainingDaysPerWeek),
            IncludeCardio = ShouldIncludeCardio(userProfile.Age, muscleGroupPreferences),
            IncludeFlexibility = ShouldIncludeFlexibility(userProfile.Age, physicalLimitations),

            // Muscle Group Focus
            MuscleGroupPreferences = MapMuscleGroupPreferences(muscleGroupPreferences),

            // Physical Limitations
            PhysicalLimitations = MapPhysicalLimitations(physicalLimitations),
            RecommendedIntensity = CalculateRecommendedIntensity(userProfile.Age, physicalLimitations),
            AvoidExercises = GetExercisesToAvoid(physicalLimitations),

            // Additional preferences
            PreferredExerciseTypes = DeterminePreferredExerciseTypes(equipmentPreferences, userProfile.Age)
        };

        return parameters;
    }

    private string MapGender(Gender gender)
    {
        return gender switch
        {
            Gender.Hombre => "Hombre",
            Gender.Mujer => "Mujer",
            Gender.Otro => "No binario",
            _ => "No especificado"
        };
    }

    private string DetermineExperienceLevel(int age, int trainingDays)
    {
        // Simple heuristic based on training frequency and age
        if (trainingDays <= 2)
            return "Principiante";
        else if (trainingDays <= 4 && age < 50)
            return "Intermedio";
        else if (trainingDays >= 5 && age < 40)
            return "Avanzado";
        else
            return "Intermedio"; // Conservative for older adults
    }

    private List<string> MapEquipmentPreferences(IEnumerable<UserEquipmentPreference> equipmentPreferences)
    {
        return equipmentPreferences
            .Where(ep => ep.EquipmentType != null)
            .Select(ep => ep.EquipmentType!.Name)
            .ToList();
    }

    private string DetermineGymType(IEnumerable<UserEquipmentPreference> equipmentPreferences)
    {
        var equipmentNames = equipmentPreferences
            .Where(ep => ep.EquipmentType != null)
            .Select(ep => ep.EquipmentType!.Name.ToLower())
            .ToList();

        // Check for full gym equipment
        var gymEquipment = new[] { "máquinas", "pesas", "barras", "mancuernas" };
        var homeEquipment = new[] { "peso corporal", "bandas", "esterilla" };

        int gymScore = gymEquipment.Count(ge => equipmentNames.Any(en => en.Contains(ge)));
        int homeScore = homeEquipment.Count(he => equipmentNames.Any(en => en.Contains(he)));

        if (gymScore >= 2)
            return "Gimnasio";
        else if (homeScore >= 1 || equipmentNames.Any(en => en.Contains("peso corporal")))
            return "Casa";
        else
            return "Parque";
    }

    private string DeterminePrimaryGoal(IEnumerable<UserMuscleGroupPreference> muscleGroupPreferences, int age)
    {
        var preferences = muscleGroupPreferences.ToList();

        if (!preferences.Any())
        {
            return age >= 50 ? "Fitness General" : "Masa";
        }

        // Count high emphasis muscle groups
        var highEmphasisCount = preferences.Count(mgp => mgp.EmphasisLevel == EmphasisLevel.Alto);

        if (age >= 60)
            return "Fitness General";
        else if (highEmphasisCount >= 3)
            return "Masa";
        else if (preferences.Any(mgp => mgp.MuscleGroup?.Name.Contains("Cardio") == true))
            return "Resistencia";
        else
            return "Fitness General";
    }

    private int CalculateSessionDuration(int trainingDays)
    {
        return trainingDays switch
        {
            <= 2 => 60, // Fewer days = longer sessions
            3 => 50,
            4 => 45,
            >= 5 => 40  // More days = shorter sessions
        };
    }

    private bool ShouldIncludeCardio(int age, IEnumerable<UserMuscleGroupPreference> muscleGroupPreferences)
    {
        // Always include some cardio for older adults
        if (age >= 50) return true;

        // Check if user has cardio preference
        return muscleGroupPreferences.Any(mgp =>
            mgp.MuscleGroup?.Name.Contains("Cardio") == true ||
            mgp.MuscleGroup?.Name.Contains("Resistencia") == true);
    }

    private bool ShouldIncludeFlexibility(int age, IEnumerable<UserPhysicalLimitation> physicalLimitations)
    {
        // Always include flexibility for older adults or those with limitations
        return age >= 40 || physicalLimitations.Any();
    }

    private List<MuscleGroupFocus> MapMuscleGroupPreferences(IEnumerable<UserMuscleGroupPreference> muscleGroupPreferences)
    {
        return muscleGroupPreferences
            .Where(mgp => mgp.MuscleGroup != null)
            .Select((mgp, index) => new MuscleGroupFocus
            {
                MuscleGroup = mgp.MuscleGroup!.Name,
                EmphasisLevel = MapEmphasisLevel(mgp.EmphasisLevel),
                Priority = index + 1 // Simple priority based on order
            })
            .ToList();
    }

    private string MapEmphasisLevel(EmphasisLevel emphasisLevel)
    {
        return emphasisLevel switch
        {
            EmphasisLevel.Alto => "Alto",
            EmphasisLevel.Medio => "Medio",
            EmphasisLevel.Bajo => "Bajo",
            _ => "Medio"
        };
    }

    private List<string> MapPhysicalLimitations(IEnumerable<UserPhysicalLimitation> physicalLimitations)
    {
        return physicalLimitations
            .Select(pl => MapLimitationType(pl.LimitationType, pl.Description, pl.CustomRestrictions))
            .ToList();
    }

    private string MapLimitationType(LimitationType limitationType, string? description, string? customRestrictions)
    {
        var limitation = limitationType switch
        {
            LimitationType.ProblemasEspalda => "Problemas de espalda",
            LimitationType.ProblemasRodilla => "Problemas de rodilla",
            LimitationType.ProblemasHombro => "Problemas de hombro",
            LimitationType.ProblemasCardivasculares => "Problemas cardiovasculares",
            LimitationType.Artritis => "Artritis",
            LimitationType.Personalizada => customRestrictions ?? "Limitación personalizada",
            _ => "Limitación no especificada"
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            limitation += $" ({description})";
        }

        return limitation;
    }

    private int CalculateRecommendedIntensity(int age, IEnumerable<UserPhysicalLimitation> physicalLimitations)
    {
        int baseIntensity = 3; // Default moderate intensity

        // Age adjustments
        if (age >= 70) baseIntensity = Math.Min(baseIntensity, 2);
        else if (age >= 60) baseIntensity = Math.Min(baseIntensity, 3);
        else if (age <= 25) baseIntensity = Math.Min(baseIntensity + 1, 5);

        // Limitation adjustments
        var limitationCount = physicalLimitations.Count();
        var hasCardiovascularIssues = physicalLimitations.Any(pl =>
            pl.LimitationType == LimitationType.ProblemasCardivasculares);

        if (hasCardiovascularIssues) baseIntensity = Math.Min(baseIntensity, 2);
        else if (limitationCount >= 2) baseIntensity = Math.Min(baseIntensity, 2);
        else if (limitationCount == 1) baseIntensity = Math.Min(baseIntensity, 3);

        return Math.Max(1, Math.Min(5, baseIntensity));
    }

    private List<string> GetExercisesToAvoid(IEnumerable<UserPhysicalLimitation> physicalLimitations)
    {
        var avoidList = new List<string>();

        foreach (var limitation in physicalLimitations)
        {
            switch (limitation.LimitationType)
            {
                case LimitationType.ProblemasEspalda:
                    avoidList.AddRange(new[] { "Peso muerto", "Sentadillas profundas", "Abdominales tradicionales" });
                    break;
                case LimitationType.ProblemasRodilla:
                    avoidList.AddRange(new[] { "Sentadillas profundas", "Lunges", "Saltos" });
                    break;
                case LimitationType.ProblemasHombro:
                    avoidList.AddRange(new[] { "Press militar", "Elevaciones laterales pesadas", "Dominadas" });
                    break;
                case LimitationType.ProblemasCardivasculares:
                    avoidList.AddRange(new[] { "Ejercicios de alta intensidad", "Sprints", "Levantamientos máximos" });
                    break;
            }
        }

        return avoidList.Distinct().ToList();
    }

    private List<string> DeterminePreferredExerciseTypes(IEnumerable<UserEquipmentPreference> equipmentPreferences, int age)
    {
        var preferences = new List<string>();

        var hasWeights = equipmentPreferences.Any(ep =>
            ep.EquipmentType?.Name.Contains("pesas") == true ||
            ep.EquipmentType?.Name.Contains("mancuernas") == true);

        var hasBodyweight = equipmentPreferences.Any(ep =>
            ep.EquipmentType?.Name.Contains("peso corporal") == true);

        if (hasWeights)
        {
            preferences.Add("Entrenamiento con pesas");
        }

        if (hasBodyweight || age >= 50)
        {
            preferences.Add("Ejercicios funcionales");
            preferences.Add("Ejercicios de peso corporal");
        }

        if (age >= 60)
        {
            preferences.Add("Ejercicios de equilibrio");
            preferences.Add("Ejercicios de bajo impacto");
        }

        return preferences;
    }
}