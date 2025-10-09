using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Models.Routines;

namespace GymRoutineGenerator.Core.Services
{
    public interface IConversationMemoryService
    {
        /// <summary>
        /// Inicia una nueva sesión de conversación para un usuario
        /// </summary>
        Task<ConversationSession> StartConversationSessionAsync(int userId, string sessionType = "routine_modification");

        /// <summary>
        /// Agrega un turno de conversación (pregunta del usuario + respuesta del asistente)
        /// </summary>
        Task<ConversationTurn> AddConversationTurnAsync(int sessionId, string userMessage, string assistantResponse, ConversationContext? context = null);

        /// <summary>
        /// Obtiene el historial de conversación de una sesión específica
        /// </summary>
        Task<List<ConversationTurn>> GetConversationHistoryAsync(int sessionId, int maxTurns = 10);

        /// <summary>
        /// Construye el contexto completo de conversación para una sesión
        /// </summary>
        Task<ConversationContext> BuildConversationContextAsync(int sessionId);

        /// <summary>
        /// Actualiza el contexto de conversación con nueva información
        /// </summary>
        Task UpdateConversationContextAsync(int sessionId, ConversationContext context);

        /// <summary>
        /// Obtiene la sesión activa de un usuario (si existe)
        /// </summary>
        Task<ConversationSession?> GetActiveSessionAsync(int userId);

        /// <summary>
        /// Finaliza una sesión de conversación
        /// </summary>
        Task EndConversationSessionAsync(int sessionId);

        /// <summary>
        /// Genera un resumen de una sesión de conversación completada
        /// </summary>
        Task<ConversationSummary> GetConversationSummaryAsync(int sessionId);

        /// <summary>
        /// Obtiene el historial de sesiones de conversación de un usuario
        /// </summary>
        Task<List<ConversationSession>> GetUserConversationHistoryAsync(int userId, int maxSessions = 10);

        /// <summary>
        /// Limpia sesiones expiradas para liberar memoria
        /// </summary>
        Task CleanupExpiredSessionsAsync();
    }

    public class ConversationSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public int TotalInteractions { get; set; }
        public Dictionary<string, object> SessionData { get; set; } = new();
        public string? SessionSummary { get; set; }
    }

    public class ConversationTurn
    {
        public string Id { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public string AssistantResponse { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public ConversationContext Context { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ConversationContext
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime SessionStartTime { get; set; }
        public DateTime LastInteractionTime { get; set; }
        public int TotalInteractions { get; set; }
        public List<string> RecentTopics { get; set; } = new();
        public List<string> UserIntents { get; set; } = new();
        public string CurrentFocus { get; set; } = string.Empty;
        public ConversationFlow ConversationFlow { get; set; } = new();
        public ConversationUserPreferences UserPreferences { get; set; } = new();
        public List<string> PendingActions { get; set; } = new();
        public RoutineContext? RoutineContext { get; set; }

        // Additional compatibility properties
        public string ConversationId { get; set; } = string.Empty;
        public string UserRequest { get; set; } = string.Empty;
        public string CurrentRoutine { get; set; } = string.Empty;
        public ConversationalUserProfile UserProfile { get; set; } = new();
        public List<ConversationContext> PreviousMessages { get; set; } = new();
        public string PendingModifications { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string AIResponse { get; set; } = string.Empty;
        public bool RequiresConfirmation { get; set; } = false;
        public bool ModificationApplied { get; set; } = false;
    }

    public class ConversationFlow
    {
        public string Phase { get; set; } = string.Empty;
        public List<string> ProgressIndicators { get; set; } = new();
        public List<string> StuckIndicators { get; set; } = new();
        public List<string> NextSuggestedActions { get; set; } = new();
    }

    public class ConversationUserPreferences
    {
        public List<string> EquipmentPreferences { get; set; } = new();
        public List<string> MuscleGroupPreferences { get; set; } = new();
        public List<string> PhysicalLimitations { get; set; } = new();
    }

    public class ConversationSummary
    {
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TotalTurns { get; set; }
        public List<string> MainTopics { get; set; } = new();
        public List<string> ActionsPerformed { get; set; } = new();
        public double UserSatisfaction { get; set; }
        public List<string> KeyInsights { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
