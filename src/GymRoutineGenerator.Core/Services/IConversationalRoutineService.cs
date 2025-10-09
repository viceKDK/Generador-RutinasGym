using GymRoutineGenerator.Core.Models.Routines;

namespace GymRoutineGenerator.Core.Services;

public interface IConversationalRoutineService
{
    Task<ConversationResponse> ProcessRoutineModificationAsync(
        string userRequest,
        string currentRoutine,
        ConversationalUserProfile userProfile,
        string conversationId = "");

    // Nuevo método para streaming
    Task ProcessRoutineModificationStreamingAsync(
        string userRequest,
        string currentRoutine,
        ConversationalUserProfile userProfile,
        Action<string> onTokenReceived,
        string conversationId = "");

    Task<ConversationResponse> ConfirmModificationAsync(
        string conversationId,
        bool confirmed);

    Task<List<string>> GetRoutineSuggestionsAsync(
        ConversationalUserProfile userProfile,
        string currentRoutine = "");

    ConversationHistory GetConversationHistory(string conversationId);

    void ClearConversationHistory(string conversationId = "");
}