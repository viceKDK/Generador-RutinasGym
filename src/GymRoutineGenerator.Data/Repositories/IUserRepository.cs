using GymRoutineGenerator.Data.Entities;
using CoreModels = GymRoutineGenerator.Core.Models;
using CoreServices = GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Data.Repositories;

public interface IUserRepository
{
    // User Profile methods
    Task<UserProfile> GetByIdAsync(int userId);
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task<UserProfile> UpdateAsync(UserProfile profile);
    Task<List<UserProfile>> GetAllAsync();
    Task<bool> DeleteAsync(int userId);
    Task<UserProfile?> GetByNameAsync(string name);
    Task<List<UserProfile>> GetByAgeRangeAsync(int minAge, int maxAge);
    Task<List<UserProfile>> GetByFitnessLevelAsync(string fitnessLevel);

    // User Routine methods
    Task<UserRoutine> GetUserRoutineByIdAsync(int routineId);
    Task<UserRoutine> CreateUserRoutineAsync(UserRoutine routine);
    Task<UserRoutine> UpdateUserRoutineAsync(UserRoutine routine);
    Task<List<UserRoutine>> GetUserRoutineHistoryAsync(int userId, int maxCount);

    // Physical Limitations
    Task<List<CoreModels.UserPhysicalLimitation>> GetUserPhysicalLimitationsAsync(int userId);

    // Equipment Preferences
    Task<List<CoreModels.UserEquipmentPreference>> GetUserEquipmentPreferencesAsync(int userId);

    // Muscle Group Preferences
    Task<List<CoreModels.UserMuscleGroupPreference>> GetUserMuscleGroupPreferencesAsync(int userId);

    // Conversation methods
    Task SaveConversationSessionAsync(CoreServices.ConversationSession session);
    Task SaveConversationTurnAsync(CoreServices.ConversationTurn turn);
    Task<List<CoreServices.ConversationTurn>> GetConversationHistoryAsync(string sessionId, int maxTurns);
    Task UpdateConversationSessionAsync(CoreServices.ConversationSession session);
    Task<CoreServices.ConversationSession?> GetActiveConversationSessionAsync(int userId);
    Task<CoreServices.ConversationSession?> GetConversationSessionAsync(string sessionId);
    Task<List<CoreServices.ConversationSession>> GetUserConversationSessionsAsync(int userId, int maxSessions);

    // Modification history
    Task SaveRoutineModificationAsync(RoutineModification historyEntry);
}