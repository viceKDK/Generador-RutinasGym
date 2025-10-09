using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Infrastructure.Services
{
    public interface IAIConversationService
    {
        // Conversación para modificación de rutinas
        Task<AIResponse> ProcessRoutineModificationRequestAsync(
            string userMessage,
            UserRoutine currentRoutine,
            ConversationContext context);

        // Búsqueda de ejercicios con IA
        Task<AIExerciseSearchResponse> FindExerciseByDescriptionAsync(
            string description,
            ConversationContext context);

        // Reconocimiento de imágenes
        Task<AIExerciseIdentificationResponse> IdentifyExerciseFromImageAsync(
            byte[] imageData,
            ConversationContext context);

        // Sugerencias inteligentes
        Task<List<ExerciseModificationSuggestion>> GetModificationSuggestionsAsync(
            UserRoutine routine,
            string userGoal);

        // Conversación general sobre fitness
        Task<AIResponse> ProcessGeneralFitnessQuestionAsync(
            string question,
            ConversationContext context);

        // Explicaciones educativas
        Task<string> ExplainExerciseBenefitsAsync(int exerciseId);
        Task<string> ExplainWorkoutPrincipleAsync(string principle);
    }

    public class ConversationContext
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public UserRoutine? CurrentRoutine { get; set; }
        public List<string> ConversationHistory { get; set; } = new();
        public Dictionary<string, object> UserPreferences { get; set; } = new();
        public DateTime SessionStarted { get; set; } = DateTime.Now;
    }

    public class AIResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<ExerciseModificationSuggestion> SuggestedChanges { get; set; } = new();
        public List<Exercise> RelevantExercises { get; set; } = new();
        public bool RequiresUserConfirmation { get; set; } = false;
        public string ActionType { get; set; } = string.Empty; // "Modify", "Suggest", "Explain", "Search"
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class AIExerciseSearchResponse
    {
        public bool Found { get; set; }
        public List<Exercise> MatchedExercises { get; set; } = new();
        public List<ExerciseFileInfo> MatchedFiles { get; set; } = new();
        public string Explanation { get; set; } = string.Empty;
        public float ConfidenceScore { get; set; }
        public List<string> AlternativeQueries { get; set; } = new();
    }

    public class AIExerciseIdentificationResponse
    {
        public bool Identified { get; set; }
        public Exercise? IdentifiedExercise { get; set; }
        public ExerciseFileInfo? IdentifiedFile { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // "Database", "docs/ejercicios"
        public float ConfidenceScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public List<Exercise> SimilarExercises { get; set; } = new();
    }

    public class ExerciseModificationSuggestion
    {
        public string Type { get; set; } = string.Empty; // "Replace", "Add", "Remove", "Modify"
        public int? TargetExerciseId { get; set; }
        public string TargetExerciseName { get; set; } = string.Empty;
        public Exercise? SuggestedExercise { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int Priority { get; set; } = 1; // 1 = High, 2 = Medium, 3 = Low
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}