using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public interface IPhysicalLimitationService
{
    Task<List<UserPhysicalLimitation>> SetUserPhysicalLimitationsAsync(int userProfileId, List<PhysicalLimitationRequest> limitations);
    Task<List<UserPhysicalLimitation>> GetUserPhysicalLimitationsAsync(int userProfileId);
    Task<bool> ClearUserPhysicalLimitationsAsync(int userProfileId);
    Task<List<Exercise>> SearchExercisesForExclusionAsync(string searchTerm, int limit = 10);
    Task<IntensityRecommendation> GetRecommendedIntensityAsync(List<LimitationType> limitations);
    Task<SafetyGuidelines> GetSafetyGuidelinesAsync(List<LimitationType> limitations);
}

public class PhysicalLimitationRequest
{
    public LimitationType LimitationType { get; set; }
    public string? Description { get; set; }
    public string? CustomRestrictions { get; set; }
}

public class IntensityRecommendation
{
    public int RecommendedLevel { get; set; } // 1-5 scale
    public string RecommendationReason { get; set; } = string.Empty;
    public List<string> Precautions { get; set; } = new();
}

public class SafetyGuidelines
{
    public List<string> GeneralPrecautions { get; set; } = new();
    public List<string> ExercisesToAvoid { get; set; } = new();
    public List<string> RecommendedModifications { get; set; } = new();
    public string MedicalDisclaimer { get; set; } = string.Empty;
}