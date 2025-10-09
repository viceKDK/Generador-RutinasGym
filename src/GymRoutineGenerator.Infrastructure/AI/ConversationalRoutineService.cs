using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models.Routines;
using System.Text.Json;
using System.Text;
using ConversationContext = GymRoutineGenerator.Core.Services.ConversationContext;

namespace GymRoutineGenerator.Infrastructure.AI;

public class ConversationalRoutineService : IConversationalRoutineService
{
    private readonly IOllamaService _ollamaService;
    private readonly IPromptTemplateService _promptService;
    private readonly ISpanishResponseProcessor _responseProcessor;
    private readonly List<ConversationContext> _conversationHistory;

    public ConversationalRoutineService(
        IOllamaService ollamaService,
        IPromptTemplateService promptService,
        ISpanishResponseProcessor responseProcessor)
    {
        _ollamaService = ollamaService;
        _promptService = promptService;
        _responseProcessor = responseProcessor;
        _conversationHistory = new List<ConversationContext>();
    }

    public async Task<ConversationResponse> ProcessRoutineModificationAsync(
        string userRequest,
        string currentRoutine,
        ConversationalUserProfile userProfile,
        string conversationId = "")
    {
        try
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                conversationId = Guid.NewGuid().ToString();
            }

            // Build conversation context
            var context = BuildConversationContext(userRequest, currentRoutine, userProfile, conversationId);

            // Generate modification prompt
            var prompt = await _promptService.GetConversationalModificationPromptAsync(context);

            // Get AI response
            var aiResponse = await _ollamaService.GenerateResponseAsync(prompt);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                return new ConversationResponse
                {
                    Success = false,
                    Message = "No se pudo generar una respuesta de la IA.",
                    ConversationId = conversationId
                };
            }

            // Process the AI response
            var processedResponse = await _responseProcessor.ProcessConversationalResponseAsync(aiResponse, context);

            // Update conversation history
            UpdateConversationHistory(context, processedResponse);

            return new ConversationResponse
            {
                Success = true,
                Message = processedResponse.Response,
                ModifiedRoutine = processedResponse.ModifiedRoutine,
                Suggestions = processedResponse.Suggestions,
                ConversationId = conversationId,
                RequiresUserConfirmation = processedResponse.RequiresConfirmation,
                ConfirmationMessage = processedResponse.ConfirmationMessage
            };
        }
        catch (Exception ex)
        {
            return new ConversationResponse
            {
                Success = false,
                Message = $"Error al procesar la solicitud: {ex.Message}",
                ConversationId = conversationId
            };
        }
    }

    public async Task<ConversationResponse> ConfirmModificationAsync(
        string conversationId,
        bool confirmed)
    {
        try
        {
            var context = _conversationHistory.LastOrDefault(c => c.ConversationId == conversationId);
            if (context == null)
            {
                return new ConversationResponse
                {
                    Success = false,
                    Message = "No se encontró el contexto de la conversación."
                };
            }

            if (confirmed)
            {
                // Apply the pending modifications
                return new ConversationResponse
                {
                    Success = true,
                    Message = "Modificaciones aplicadas correctamente.",
                    ModifiedRoutine = context.PendingModifications,
                    ConversationId = conversationId
                };
            }
            else
            {
                // Cancel modifications
                return new ConversationResponse
                {
                    Success = true,
                    Message = "Modificaciones canceladas. ¿Hay algo más que te gustaría cambiar?",
                    ConversationId = conversationId
                };
            }
        }
        catch (Exception ex)
        {
            return new ConversationResponse
            {
                Success = false,
                Message = $"Error al confirmar modificación: {ex.Message}",
                ConversationId = conversationId
            };
        }
    }

    public async Task<List<string>> GetRoutineSuggestionsAsync(
        ConversationalUserProfile userProfile,
        string currentRoutine = "")
    {
        try
        {
            var prompt = await _promptService.GetSuggestionPromptAsync(userProfile, currentRoutine);
            var response = await _ollamaService.GenerateResponseAsync(prompt);

            if (string.IsNullOrWhiteSpace(response))
            {
                return GetDefaultSuggestions();
            }

            var suggestions = ExtractSuggestionsFromResponse(response);
            return suggestions.Any() ? suggestions : GetDefaultSuggestions();
        }
        catch (Exception)
        {
            return GetDefaultSuggestions();
        }
    }

    public ConversationHistory GetConversationHistory(string conversationId)
    {
        var contexts = _conversationHistory
            .Where(c => c.ConversationId == conversationId)
            .OrderBy(c => c.Timestamp)
            .ToList();

        return new ConversationHistory
        {
            ConversationId = conversationId,
            Messages = contexts.Select(c => new ConversationMessage
            {
                UserMessage = c.UserRequest,
                AIResponse = c.AIResponse,
                Timestamp = c.Timestamp,
                WasModificationApplied = c.ModificationApplied
            }).ToList()
        };
    }

    public void ClearConversationHistory(string conversationId = "")
    {
        if (string.IsNullOrEmpty(conversationId))
        {
            _conversationHistory.Clear();
        }
        else
        {
            _conversationHistory.RemoveAll(c => c.ConversationId == conversationId);
        }
    }

    private ConversationContext BuildConversationContext(
        string userRequest,
        string currentRoutine,
        ConversationalUserProfile userProfile,
        string conversationId)
    {
        return new ConversationContext
        {
            ConversationId = conversationId,
            UserRequest = userRequest,
            CurrentRoutine = currentRoutine,
            UserProfile = userProfile,
            Timestamp = DateTime.UtcNow,
            PreviousMessages = _conversationHistory
                .Where(c => c.ConversationId == conversationId)
                .OrderBy(c => c.Timestamp)
                .ToList()
        };
    }

    private void UpdateConversationHistory(ConversationContext context, ProcessedConversationalResponse response)
    {
        context.AIResponse = response.Response;
        context.PendingModifications = response.ModifiedRoutine;
        context.RequiresConfirmation = response.RequiresConfirmation;

        _conversationHistory.Add(context);

        // Keep only last 50 messages per conversation to avoid memory issues
        var conversationMessages = _conversationHistory
            .Where(c => c.ConversationId == context.ConversationId)
            .OrderByDescending(c => c.Timestamp)
            .Take(50)
            .ToList();

        _conversationHistory.RemoveAll(c => c.ConversationId == context.ConversationId);
        _conversationHistory.AddRange(conversationMessages);
    }

    // Nuevo método para streaming
    public async Task ProcessRoutineModificationStreamingAsync(
        string userRequest,
        string currentRoutine,
        ConversationalUserProfile userProfile,
        Action<string> onTokenReceived,
        string conversationId = "")
    {
        try
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                conversationId = Guid.NewGuid().ToString();
            }

            // Build conversation context
            var context = BuildConversationContext(userRequest, currentRoutine, userProfile, conversationId);

            // Generate modification prompt
            var prompt = await _promptService.GetConversationalModificationPromptAsync(context);

            // Call streaming API
            await _ollamaService.GenerateStreamingConversationalResponseAsync(
                prompt,
                onTokenReceived,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            onTokenReceived($"\n\n[Error: {ex.Message}]");
        }
    }

    private List<string> ExtractSuggestionsFromResponse(string response)
    {
        var suggestions = new List<string>();

        try
        {
            // Try to parse as JSON first
            if (response.TrimStart().StartsWith("[") || response.TrimStart().StartsWith("{"))
            {
                var jsonSuggestions = JsonSerializer.Deserialize<string[]>(response);
                if (jsonSuggestions != null)
                {
                    suggestions.AddRange(jsonSuggestions);
                }
            }
            else
            {
                // Parse as text with bullet points or numbered lists
                var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var cleanLine = line.Trim();
                    if (cleanLine.StartsWith("•") || cleanLine.StartsWith("-") ||
                        char.IsDigit(cleanLine[0]))
                    {
                        var suggestion = cleanLine.TrimStart('•', '-', ' ');
                        // Remove leading digits and dots
                        while (suggestion.Length > 0 && (char.IsDigit(suggestion[0]) || suggestion[0] == '.' || suggestion[0] == ' '))
                        {
                            suggestion = suggestion.Substring(1);
                        }
                        if (!string.IsNullOrWhiteSpace(suggestion))
                        {
                            suggestions.Add(suggestion.Trim());
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Fallback to simple line splitting
            suggestions = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(5)
                .ToList();
        }

        return suggestions;
    }

    private List<string> GetDefaultSuggestions()
    {
        return new List<string>
        {
            "Añadir más ejercicios de cardio",
            "Incrementar la intensidad de los ejercicios",
            "Cambiar ejercicios por variaciones más fáciles",
            "Agregar ejercicios de flexibilidad",
            "Modificar el número de series y repeticiones"
        };
    }
}

