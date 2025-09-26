using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public interface IMuscleGroupPreferenceService
{
    Task<List<UserMuscleGroupPreference>> SetUserMuscleGroupPreferencesAsync(int userProfileId, List<MuscleGroupPreferenceRequest> preferences);
    Task<List<UserMuscleGroupPreference>> GetUserMuscleGroupPreferencesAsync(int userProfileId);
    Task<List<MuscleGroup>> GetAllMuscleGroupsAsync();
    Task<bool> ClearUserMuscleGroupPreferencesAsync(int userProfileId);
    Task<TrainingObjectiveTemplate> ApplyTrainingObjectiveTemplateAsync(int userProfileId, TrainingObjectiveType objectiveType);
}

public class MuscleGroupPreferenceRequest
{
    public int MuscleGroupId { get; set; }
    public EmphasisLevel EmphasisLevel { get; set; }
}

public class TrainingObjectiveTemplate
{
    public TrainingObjectiveType ObjectiveType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MuscleGroupPreferenceRequest> MuscleGroupPreferences { get; set; } = new();
}

public enum TrainingObjectiveType
{
    WeightLoss = 1,
    MuscleGain = 2,
    GeneralFitness = 3,
    Strength = 4,
    Endurance = 5,
    Mobility = 6
}