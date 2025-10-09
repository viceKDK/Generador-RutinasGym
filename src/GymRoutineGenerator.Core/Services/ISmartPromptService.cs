using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface ISmartPromptService
    {
        Task<string> BuildContextualPromptAsync(UserRoutine routine, string userMessage, UserProfile profile);
        Task<string> BuildExplanationPromptAsync(ExerciseModification modification);
        Task<string> BuildSafetyValidationPromptAsync(ExerciseModification modification, UserProfile profile);
        Task<string> BuildExerciseSearchPromptAsync(string userDescription, UserProfile profile);
        Task<string> BuildProgressionAnalysisPromptAsync(UserProfile profile, List<UserRoutine> routineHistory);
        Task<string> BuildRoutineOptimizationPromptAsync(UserRoutine routine, UserProfile profile);
        Task<string> BuildAlternativeExercisePromptAsync(Exercise currentExercise, UserProfile profile, string reason);
    }

    public class PromptContext
    {
        public UserProfile UserProfile { get; set; } = new();
        public UserRoutine? CurrentRoutine { get; set; }
        public List<UserRoutine> RoutineHistory { get; set; } = new();
        public List<UserPhysicalLimitation> PhysicalLimitations { get; set; } = new();
        public List<UserEquipmentPreference> EquipmentPreferences { get; set; } = new();
        public List<UserMuscleGroupPreference> MuscleGroupPreferences { get; set; } = new();
        public string ConversationContext { get; set; } = string.Empty;
        public DateTime SessionStartTime { get; set; } = DateTime.UtcNow;
        public List<string> PreviousInteractions { get; set; } = new();
    }

    public class PromptTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public List<string> RequiredVariables { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public PromptType Type { get; set; }
    }

    public enum PromptType
    {
        Conversational,
        Analytical,
        Safety,
        Educational,
        Search,
        Progression
    }
}