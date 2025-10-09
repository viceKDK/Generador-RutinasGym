using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Business.Services;

public interface IUserProfileService
{
    Task<UserProfile> CreateUserProfileAsync(string name, int age, string gender, string fitnessLevel);
    Task<UserProfile?> GetUserProfileAsync(int userId);
    Task<List<UserProfile>> GetAllUserProfilesAsync();
    Task<UserProfile> UpdateUserProfileAsync(UserProfile profile);
    Task<bool> DeleteUserProfileAsync(int userId);
    Task<UserProfile?> GetUserByNameAsync(string name);
    Task AddEquipmentPreferenceAsync(int userId, string equipmentType, bool isAvailable, string notes = "");
    Task AddPhysicalLimitationAsync(int userId, string limitationType, string description, int severity, string exercisesToAvoid = "[]");
    Task<UserRoutine> SaveUserRoutineAsync(int userId, string routineName, string routineData, string notes = "");
    Task<List<UserRoutine>> GetUserRoutineHistoryAsync(int userId);
    Task<UserProfile> GetRecommendedProfileForRoutineAsync(string fitnessLevel, int age);
}