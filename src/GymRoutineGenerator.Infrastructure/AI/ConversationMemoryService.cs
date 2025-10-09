using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;
using System.Text.Json;
using UserEquipmentPreference = GymRoutineGenerator.Data.Entities.UserEquipmentPreference;
using UserMuscleGroupPreference = GymRoutineGenerator.Data.Entities.UserMuscleGroupPreference;
using UserPhysicalLimitation = GymRoutineGenerator.Data.Entities.UserPhysicalLimitation;

namespace GymRoutineGenerator.Infrastructure.AI
{
    public class ConversationMemoryService : IConversationMemoryService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ConversationMemoryService> _logger;

        // En memoria para sesiones activas
        private readonly Dictionary<int, ConversationSession> _activeSessions;
        private readonly Dictionary<int, List<ConversationTurn>> _activeConversations;

        // Configuración de memoria
        private const int MAX_CONVERSATION_HISTORY = 50;
        private const int MAX_ACTIVE_SESSIONS = 100;
        private const int SESSION_TIMEOUT_MINUTES = 30;

        public ConversationMemoryService(
            IUserRepository userRepository,
            ILogger<ConversationMemoryService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
            _activeSessions = new Dictionary<int, ConversationSession>();
            _activeConversations = new Dictionary<int, List<ConversationTurn>>();
        }

        public async Task<ConversationSession> StartConversationSessionAsync(int userId, string sessionType = "routine_modification")
        {
            try
            {
                _logger.LogInformation($"Starting conversation session for user {userId}");

                // Limpiar sesiones expiradas
                await CleanupExpiredSessionsAsync();

                var session = new ConversationSession
                {
                    Id = GenerateSessionId(),
                    UserId = userId,
                    SessionType = sessionType,
                    StartedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IsActive = true,
                    SessionData = new Dictionary<string, object>(),
                    TotalInteractions = 0
                };

                // Cargar contexto del usuario
                await LoadUserContextAsync(session);

                // Guardar en memoria activa
                _activeSessions[session.Id] = session;
                _activeConversations[session.Id] = new List<ConversationTurn>();

                // Persistir en base de datos
                await _userRepository.SaveConversationSessionAsync(session);

                _logger.LogInformation($"Started conversation session {session.Id} for user {userId}");
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting conversation session for user {userId}");
                throw;
            }
        }

        public async Task<ConversationTurn> AddConversationTurnAsync(int sessionId, string userMessage, string assistantResponse, ConversationContext? context = null)
        {
            try
            {
                if (!_activeSessions.ContainsKey(sessionId))
                {
                    throw new ArgumentException($"Session {sessionId} not found or expired");
                }

                var session = _activeSessions[sessionId];
                session.LastActivity = DateTime.UtcNow;
                session.TotalInteractions++;

                var turn = new ConversationTurn
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserMessage = userMessage,
                    AssistantResponse = assistantResponse,
                    Timestamp = DateTime.UtcNow,
                    MessageType = DetermineMessageType(userMessage),
                    Context = context ?? new ConversationContext(),
                    Metadata = ExtractMessageMetadata(userMessage, assistantResponse)
                };

                // Agregar a la conversación activa
                if (!_activeConversations.ContainsKey(sessionId))
                {
                    _activeConversations[sessionId] = new List<ConversationTurn>();
                }

                _activeConversations[sessionId].Add(turn);

                // Mantener solo las últimas N interacciones en memoria
                if (_activeConversations[sessionId].Count > MAX_CONVERSATION_HISTORY)
                {
                    _activeConversations[sessionId].RemoveAt(0);
                }

                // Actualizar datos de sesión con información relevante
                await UpdateSessionDataAsync(session, turn);

                // Persistir el turno en base de datos
                await _userRepository.SaveConversationTurnAsync(turn);

                _logger.LogInformation($"Added conversation turn to session {sessionId}");
                return turn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding conversation turn to session {sessionId}");
                throw;
            }
        }

        public async Task<List<ConversationTurn>> GetConversationHistoryAsync(int sessionId, int maxTurns = 10)
        {
            try
            {
                if (_activeConversations.ContainsKey(sessionId))
                {
                    // Devolver de memoria activa
                    return _activeConversations[sessionId]
                        .OrderByDescending(t => t.Timestamp)
                        .Take(maxTurns)
                        .OrderBy(t => t.Timestamp)
                        .ToList();
                }

                // Cargar de base de datos
                var history = await _userRepository.GetConversationHistoryAsync(sessionId.ToString(), maxTurns);
                _logger.LogInformation($"Retrieved {history?.Count ?? 0} conversation turns for session {sessionId}");
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving conversation history for session {sessionId}");
                return new List<ConversationTurn>();
            }
        }

        public async Task<ConversationContext> BuildConversationContextAsync(int sessionId)
        {
            try
            {
                if (!_activeSessions.ContainsKey(sessionId))
                {
                    throw new ArgumentException($"Session {sessionId} not found");
                }

                var session = _activeSessions[sessionId];
                var recentHistory = await GetConversationHistoryAsync(sessionId, 5);

                var context = new ConversationContext
                {
                    SessionId = sessionId,
                    UserId = session.UserId,
                    SessionType = session.SessionType,
                    SessionStartTime = session.StartedAt,
                    LastInteractionTime = session.LastActivity,
                    TotalInteractions = session.TotalInteractions,
                    RecentTopics = ExtractRecentTopics(recentHistory),
                    UserIntents = ExtractUserIntents(recentHistory),
                    CurrentFocus = DetermineCurrentFocus(recentHistory),
                    ConversationFlow = AnalyzeConversationFlow(recentHistory),
                    UserPreferences = await LoadUserPreferencesFromSessionAsync(session),
                    PendingActions = ExtractPendingActions(recentHistory)
                };

                // Agregar información específica del dominio
                if (session.SessionType == "routine_modification")
                {
                    context.RoutineContext = await BuildRoutineContextAsync(session, recentHistory);
                }

                _logger.LogInformation($"Built conversation context for session {sessionId}");
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building conversation context for session {sessionId}");
                return new ConversationContext { SessionId = sessionId };
            }
        }

        public async Task UpdateConversationContextAsync(int sessionId, ConversationContext context)
        {
            try
            {
                if (!_activeSessions.ContainsKey(sessionId))
                {
                    throw new ArgumentException($"Session {sessionId} not found");
                }

                var session = _activeSessions[sessionId];

                // Actualizar datos de sesión con información del contexto
                session.SessionData["lastFocus"] = context.CurrentFocus;
                session.SessionData["userIntents"] = JsonSerializer.Serialize(context.UserIntents);
                session.SessionData["recentTopics"] = JsonSerializer.Serialize(context.RecentTopics);

                if (context.RoutineContext != null)
                {
                    session.SessionData["currentRoutineId"] = context.RoutineContext.CurrentRoutineId;
                    session.SessionData["modificationCount"] = context.RoutineContext.ModificationCount;
                }

                // Persistir cambios
                await _userRepository.UpdateConversationSessionAsync(session);

                _logger.LogInformation($"Updated conversation context for session {sessionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating conversation context for session {sessionId}");
            }
        }

        public async Task<ConversationSession?> GetActiveSessionAsync(int userId)
        {
            try
            {
                var activeSession = _activeSessions.Values
                    .FirstOrDefault(s => s.UserId == userId && s.IsActive);

                if (activeSession != null && IsSessionActive(activeSession))
                {
                    return activeSession;
                }

                // Buscar en base de datos
                var dbSession = await _userRepository.GetActiveConversationSessionAsync(userId);
                if (dbSession != null && IsSessionActive(dbSession))
                {
                    // Cargar en memoria activa
                    _activeSessions[dbSession.Id] = dbSession;
                    return dbSession;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting active session for user {userId}");
                return null;
            }
        }

        public async Task EndConversationSessionAsync(int sessionId)
        {
            try
            {
                if (_activeSessions.ContainsKey(sessionId))
                {
                    var session = _activeSessions[sessionId];
                    session.IsActive = false;
                    session.EndedAt = DateTime.UtcNow;

                    // Generar resumen de la sesión
                    session.SessionSummary = await GenerateSessionSummaryAsync(sessionId);

                    // Persistir cambios finales
                    await _userRepository.UpdateConversationSessionAsync(session);

                    // Limpiar de memoria activa
                    _activeSessions.Remove(sessionId);
                    _activeConversations.Remove(sessionId);

                    _logger.LogInformation($"Ended conversation session {sessionId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ending conversation session {sessionId}");
            }
        }

        public async Task<ConversationSummary> GetConversationSummaryAsync(int sessionId)
        {
            try
            {
                var session = _activeSessions.ContainsKey(sessionId)
                    ? _activeSessions[sessionId]
                    : await _userRepository.GetConversationSessionAsync(sessionId.ToString());

                if (session == null)
                {
                    throw new ArgumentException($"Session {sessionId} not found");
                }

                var history = await GetConversationHistoryAsync(sessionId);

                var summary = new ConversationSummary
                {
                    SessionId = sessionId,
                    UserId = session.UserId,
                    SessionType = session.SessionType,
                    StartTime = session.StartedAt,
                    EndTime = session.EndedAt,
                    Duration = session.EndedAt.HasValue
                        ? session.EndedAt.Value - session.StartedAt
                        : DateTime.UtcNow - session.StartedAt,
                    TotalTurns = history.Count,
                    MainTopics = ExtractMainTopics(history),
                    ActionsPerformed = ExtractActionsPerformed(history),
                    UserSatisfaction = ExtractUserSatisfaction(history),
                    KeyInsights = ExtractKeyInsights(history),
                    Summary = session.SessionSummary ?? await GenerateSessionSummaryAsync(sessionId)
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation summary for session {sessionId}");
                throw;
            }
        }

        public async Task<List<ConversationSession>> GetUserConversationHistoryAsync(int userId, int maxSessions = 10)
        {
            try
            {
                var sessions = await _userRepository.GetUserConversationSessionsAsync(userId, maxSessions);
                _logger.LogInformation($"Retrieved {sessions.Count} conversation sessions for user {userId}");
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation history for user {userId}");
                return new List<ConversationSession>();
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            try
            {
                var expiredSessions = _activeSessions.Values
                    .Where(s => !IsSessionActive(s))
                    .ToList();

                foreach (var session in expiredSessions)
                {
                    await EndConversationSessionAsync(session.Id);
                }

                _logger.LogInformation($"Cleaned up {expiredSessions.Count} expired sessions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
            }
        }

        #region Private Helper Methods

        private int GenerateSessionId()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode());
        }

        private bool IsSessionActive(ConversationSession session)
        {
            if (!session.IsActive) return false;

            var timeSinceLastActivity = DateTime.UtcNow - session.LastActivity;
            return timeSinceLastActivity.TotalMinutes <= SESSION_TIMEOUT_MINUTES;
        }

        private async Task LoadUserContextAsync(ConversationSession session)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(session.UserId);
                if (user != null)
                {
                    session.SessionData["userName"] = user.Name;
                    session.SessionData["userFitnessLevel"] = user.FitnessLevel;
                    session.SessionData["userAge"] = user.Age;
                }

                // Cargar preferencias y limitaciones
                var limitations = await _userRepository.GetUserPhysicalLimitationsAsync(session.UserId);
                session.SessionData["physicalLimitations"] = JsonSerializer.Serialize(limitations);

                var preferences = await _userRepository.GetUserEquipmentPreferencesAsync(session.UserId);
                session.SessionData["equipmentPreferences"] = JsonSerializer.Serialize(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading user context for session {session.Id}");
            }
        }

        private string DetermineMessageType(string userMessage)
        {
            var message = userMessage.ToLowerInvariant();

            if (message.Contains("cambiar") || message.Contains("modificar") || message.Contains("reemplazar"))
                return "modification_request";
            if (message.Contains("dificil") || message.Contains("fácil") || message.Contains("intensidad"))
                return "difficulty_adjustment";
            if (message.Contains("duele") || message.Contains("dolor") || message.Contains("lesión"))
                return "safety_concern";
            if (message.Contains("alternativa") || message.Contains("otro ejercicio"))
                return "alternative_request";
            if (message.Contains("explicar") || message.Contains("por qué") || message.Contains("cómo"))
                return "explanation_request";
            if (message.Contains("tiempo") || message.Contains("duración") || message.Contains("rápido"))
                return "time_constraint";

            return "general_query";
        }

        private Dictionary<string, object> ExtractMessageMetadata(string userMessage, string assistantResponse)
        {
            var metadata = new Dictionary<string, object>();

            // Detectar entidades mencionadas
            metadata["mentionedExercises"] = ExtractExerciseNames(userMessage);
            metadata["mentionedBodyParts"] = ExtractBodyParts(userMessage);
            metadata["sentimentScore"] = AnalyzeSentiment(userMessage);
            metadata["responseLength"] = assistantResponse.Length;
            metadata["containsModification"] = assistantResponse.Contains("modificar") || assistantResponse.Contains("cambiar");

            return metadata;
        }

        private async Task UpdateSessionDataAsync(ConversationSession session, ConversationTurn turn)
        {
            // Actualizar estadísticas de la sesión
            if (turn.MessageType == "modification_request")
            {
                var modCount = session.SessionData.ContainsKey("modificationCount")
                    ? (int)session.SessionData["modificationCount"] + 1
                    : 1;
                session.SessionData["modificationCount"] = modCount;
            }

            // Trackear temas principales
            var topics = session.SessionData.ContainsKey("topics")
                ? JsonSerializer.Deserialize<List<string>>(session.SessionData["topics"].ToString()!) ?? new List<string>()
                : new List<string>();

            var newTopics = ExtractTopicsFromMessage(turn.UserMessage);
            topics.AddRange(newTopics);
            session.SessionData["topics"] = JsonSerializer.Serialize(topics.Distinct().ToList());
        }

        private List<string> ExtractRecentTopics(List<ConversationTurn> history)
        {
            var topics = new List<string>();
            foreach (var turn in history.TakeLast(5))
            {
                topics.AddRange(ExtractTopicsFromMessage(turn.UserMessage));
            }
            return topics.Distinct().ToList();
        }

        private List<string> ExtractUserIntents(List<ConversationTurn> history)
        {
            var intents = new List<string>();
            foreach (var turn in history.TakeLast(3))
            {
                intents.Add(DetermineMessageType(turn.UserMessage));
            }
            return intents.Distinct().ToList();
        }

        private string DetermineCurrentFocus(List<ConversationTurn> history)
        {
            if (!history.Any()) return "routine_setup";

            var recentTypes = history.TakeLast(3).Select(t => t.MessageType).ToList();

            if (recentTypes.Count(t => t == "modification_request") >= 2)
                return "routine_modification";
            if (recentTypes.Any(t => t == "safety_concern"))
                return "safety_discussion";
            if (recentTypes.Any(t => t == "alternative_request"))
                return "exercise_alternatives";

            return "general_guidance";
        }

        private ConversationFlow AnalyzeConversationFlow(List<ConversationTurn> history)
        {
            return new ConversationFlow
            {
                Phase = DetermineConversationPhase(history),
                ProgressIndicators = CalculateProgressIndicators(history),
                StuckIndicators = DetectStuckPatterns(history),
                NextSuggestedActions = SuggestNextActions(history)
            };
        }

        private async Task<ConversationUserPreferences> LoadUserPreferencesFromSessionAsync(ConversationSession session)
        {
            var preferences = new ConversationUserPreferences();

            if (session.SessionData.ContainsKey("equipmentPreferences"))
            {
                var equipmentJson = session.SessionData["equipmentPreferences"].ToString();
                preferences.EquipmentPreferences = JsonSerializer.Deserialize<List<string>>(equipmentJson!) ?? new List<string>();
            }

            return preferences;
        }

        private async Task<RoutineContext?> BuildRoutineContextAsync(ConversationSession session, List<ConversationTurn> history)
        {
            var context = new RoutineContext();

            if (session.SessionData.ContainsKey("currentRoutineId"))
            {
                context.CurrentRoutineId = (int)session.SessionData["currentRoutineId"];
            }

            if (session.SessionData.ContainsKey("modificationCount"))
            {
                context.ModificationCount = (int)session.SessionData["modificationCount"];
            }

            context.RecentModifications = ExtractRecentModifications(history);
            context.UserSatisfactionLevel = AnalyzeUserSatisfaction(history);

            return context;
        }

        private async Task<string> GenerateSessionSummaryAsync(int sessionId)
        {
            var history = await GetConversationHistoryAsync(sessionId);
            if (!history.Any()) return "Sesión sin interacciones";

            var summary = $"Sesión de {history.Count} interacciones. ";
            var topics = ExtractMainTopics(history);
            if (topics.Any())
            {
                summary += $"Temas principales: {string.Join(", ", topics)}. ";
            }

            var modifications = history.Count(h => h.MessageType == "modification_request");
            if (modifications > 0)
            {
                summary += $"Se realizaron {modifications} modificaciones. ";
            }

            return summary;
        }

        // Placeholder implementations for analysis methods
        private List<string> ExtractExerciseNames(string message) => new();
        private List<string> ExtractBodyParts(string message) => new();
        private double AnalyzeSentiment(string message) => 0.5;
        private List<string> ExtractTopicsFromMessage(string message) => new();
        private List<string> ExtractMainTopics(List<ConversationTurn> history) => new();
        private List<string> ExtractActionsPerformed(List<ConversationTurn> history) => new();
        private double ExtractUserSatisfaction(List<ConversationTurn> history) => 0.8;
        private List<string> ExtractKeyInsights(List<ConversationTurn> history) => new();
        private List<string> ExtractPendingActions(List<ConversationTurn> history) => new();

        private string DetermineConversationPhase(List<ConversationTurn> history) => "active";
        private List<string> CalculateProgressIndicators(List<ConversationTurn> history) => new();
        private List<string> DetectStuckPatterns(List<ConversationTurn> history) => new();
        private List<string> SuggestNextActions(List<ConversationTurn> history) => new();
        private List<string> ExtractRecentModifications(List<ConversationTurn> history) => new();
        private double AnalyzeUserSatisfaction(List<ConversationTurn> history) => 0.8;

        #endregion
    }
}