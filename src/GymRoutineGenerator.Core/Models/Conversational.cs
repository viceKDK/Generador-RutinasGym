namespace GymRoutineGenerator.Core.Models.Routines;

// Simple UserProfile for conversational context
public class ConversationalUserProfile
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public string FitnessLevel { get; set; } = "";
}

// Support classes for the conversational service
public class ConversationContext
{
    public string ConversationId { get; set; } = "";
    public string UserRequest { get; set; } = "";
    public string CurrentRoutine { get; set; } = "";
    public ConversationalUserProfile UserProfile { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public List<ConversationContext> PreviousMessages { get; set; } = new();
    public string AIResponse { get; set; } = "";
    public string PendingModifications { get; set; } = "";
    public bool RequiresConfirmation { get; set; }
    public bool ModificationApplied { get; set; }
}

public class ConversationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string ModifiedRoutine { get; set; } = "";
    public List<string> Suggestions { get; set; } = new();
    public string ConversationId { get; set; } = "";
    public bool RequiresUserConfirmation { get; set; }
    public string ConfirmationMessage { get; set; } = "";
}

public class ProcessedConversationalResponse
{
    public string Response { get; set; } = "";
    public string ModifiedRoutine { get; set; } = "";
    public List<string> Suggestions { get; set; } = new();
    public bool RequiresConfirmation { get; set; }
    public string ConfirmationMessage { get; set; } = "";
}

public class ConversationHistory
{
    public string ConversationId { get; set; } = "";
    public List<ConversationMessage> Messages { get; set; } = new();
}

public class ConversationMessage
{
    public string UserMessage { get; set; } = "";
    public string AIResponse { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public bool WasModificationApplied { get; set; }
}