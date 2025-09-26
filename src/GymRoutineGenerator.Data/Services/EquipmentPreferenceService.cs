using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public class EquipmentPreferenceService : IEquipmentPreferenceService
{
    private readonly GymRoutineContext _context;

    public EquipmentPreferenceService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<List<UserEquipmentPreference>> SetUserEquipmentPreferencesAsync(int userProfileId, List<int> equipmentTypeIds)
    {
        // Validate user profile exists
        var userProfile = await _context.UserProfiles.FindAsync(userProfileId);
        if (userProfile == null)
        {
            throw new ArgumentException($"Perfil de usuario con ID {userProfileId} no encontrado");
        }

        // Validate equipment types exist
        var validEquipmentTypes = await _context.EquipmentTypes
            .Where(et => equipmentTypeIds.Contains(et.Id))
            .ToListAsync();

        var invalidIds = equipmentTypeIds.Except(validEquipmentTypes.Select(et => et.Id)).ToList();
        if (invalidIds.Any())
        {
            throw new ArgumentException($"Tipos de equipamiento no válidos: {string.Join(", ", invalidIds)}");
        }

        // Clear existing preferences
        await ClearUserEquipmentPreferencesAsync(userProfileId);

        // Create new preferences
        var preferences = equipmentTypeIds.Select(equipmentTypeId => new UserEquipmentPreference
        {
            UserProfileId = userProfileId,
            EquipmentTypeId = equipmentTypeId,
            IsAvailable = true
        }).ToList();

        _context.UserEquipmentPreferences.AddRange(preferences);
        await _context.SaveChangesAsync();

        // Return preferences with equipment type data
        return await GetUserEquipmentPreferencesAsync(userProfileId);
    }

    public async Task<List<UserEquipmentPreference>> GetUserEquipmentPreferencesAsync(int userProfileId)
    {
        return await _context.UserEquipmentPreferences
            .Include(uep => uep.EquipmentType)
            .Where(uep => uep.UserProfileId == userProfileId)
            .OrderBy(uep => uep.EquipmentType.SpanishName)
            .ToListAsync();
    }

    public async Task<List<EquipmentType>> GetAllEquipmentTypesAsync()
    {
        return await _context.EquipmentTypes
            .OrderBy(et => et.SpanishName)
            .ToListAsync();
    }

    public async Task<bool> ClearUserEquipmentPreferencesAsync(int userProfileId)
    {
        var existingPreferences = await _context.UserEquipmentPreferences
            .Where(uep => uep.UserProfileId == userProfileId)
            .ToListAsync();

        if (existingPreferences.Any())
        {
            _context.UserEquipmentPreferences.RemoveRange(existingPreferences);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}