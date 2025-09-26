using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public interface IEquipmentPreferenceService
{
    Task<List<UserEquipmentPreference>> SetUserEquipmentPreferencesAsync(int userProfileId, List<int> equipmentTypeIds);
    Task<List<UserEquipmentPreference>> GetUserEquipmentPreferencesAsync(int userProfileId);
    Task<List<EquipmentType>> GetAllEquipmentTypesAsync();
    Task<bool> ClearUserEquipmentPreferencesAsync(int userProfileId);
}

public class EquipmentPreferenceRequest
{
    public int UserProfileId { get; set; }
    public List<int> EquipmentTypeIds { get; set; } = new();
}