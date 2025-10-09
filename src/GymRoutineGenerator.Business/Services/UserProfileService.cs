using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;

namespace GymRoutineGenerator.Business.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfile> CreateUserProfileAsync(string name, int age, string gender, string fitnessLevel)
    {
        var profile = new UserProfile
        {
            Name = name,
            Age = age,
            Gender = gender,
            FitnessLevel = fitnessLevel
        };

        return await _userRepository.CreateAsync(profile);
    }

    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<List<UserProfile>> GetAllUserProfilesAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<UserProfile> UpdateUserProfileAsync(UserProfile profile)
    {
        return await _userRepository.UpdateAsync(profile);
    }

    public async Task<bool> DeleteUserProfileAsync(int userId)
    {
        return await _userRepository.DeleteAsync(userId);
    }

    public async Task<UserProfile?> GetUserByNameAsync(string name)
    {
        return await _userRepository.GetByNameAsync(name);
    }

    public async Task AddEquipmentPreferenceAsync(int userId, string equipmentType, bool isAvailable, string notes = "")
    {
        var profile = await _userRepository.GetByIdAsync(userId);
        if (profile != null)
        {
            profile.EquipmentPreferences.Add(new UserEquipmentPreference
            {
                UserId = userId,
                EquipmentType = equipmentType,
                IsAvailable = isAvailable,
                Notes = notes
            });
            await _userRepository.UpdateAsync(profile);
        }
    }

    public async Task AddPhysicalLimitationAsync(int userId, string limitationType, string description, int severity, string exercisesToAvoid = "[]")
    {
        var profile = await _userRepository.GetByIdAsync(userId);
        if (profile != null)
        {
            profile.PhysicalLimitations.Add(new UserPhysicalLimitation
            {
                UserId = userId,
                LimitationType = limitationType,
                Description = description,
                Severity = severity,
                ExercisesToAvoid = exercisesToAvoid
            });
            await _userRepository.UpdateAsync(profile);
        }
    }

    public async Task<UserRoutine> SaveUserRoutineAsync(int userId, string routineName, string routineData, string notes = "")
    {
        var profile = await _userRepository.GetByIdAsync(userId);
        if (profile != null)
        {
            var routine = new UserRoutine
            {
                UserId = userId,
                Name = routineName,
                RoutineData = routineData,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            profile.RoutineHistory.Add(routine);
            await _userRepository.UpdateAsync(profile);
            return routine;
        }
        throw new InvalidOperationException($"User with ID {userId} not found");
    }

    public async Task<List<UserRoutine>> GetUserRoutineHistoryAsync(int userId)
    {
        var profile = await _userRepository.GetByIdAsync(userId);
        return profile?.RoutineHistory ?? new List<UserRoutine>();
    }

    public async Task<UserProfile> GetRecommendedProfileForRoutineAsync(string fitnessLevel, int age)
    {
        var profiles = await _userRepository.GetByFitnessLevelAsync(fitnessLevel);
        return profiles.FirstOrDefault(p => Math.Abs(p.Age - age) <= 5) ?? new UserProfile();
    }
}