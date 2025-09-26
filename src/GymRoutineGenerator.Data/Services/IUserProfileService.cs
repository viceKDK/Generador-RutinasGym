using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public interface IUserProfileService
{
    Task<UserProfile> CreateUserProfileAsync(UserProfileCreateRequest request);
    Task<UserProfile?> GetUserProfileByIdAsync(int id);
    Task<List<UserProfile>> GetAllUserProfilesAsync();
    Task<UserProfile> UpdateUserProfileAsync(UserProfileUpdateRequest request);
    Task<bool> DeleteUserProfileAsync(int id);
    Task<UserProfileValidationResult> ValidateUserProfileAsync(UserProfileCreateRequest request);
}

public class UserProfileCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int TrainingDaysPerWeek { get; set; }
}

public class UserProfileUpdateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int TrainingDaysPerWeek { get; set; }
}

public class UserProfileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}