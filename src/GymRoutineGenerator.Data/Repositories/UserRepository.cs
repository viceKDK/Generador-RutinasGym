using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using CoreModels = GymRoutineGenerator.Core.Models;
using CoreServices = GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly GymRoutineContext _context;

    public UserRepository(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<UserProfile> GetByIdAsync(int userId)
    {
        return await _context.UserProfiles
            .Include(u => u.RoutineHistory)
            .Include(u => u.EquipmentPreferences)
            .Include(u => u.MuscleGroupPreferences)
            .Include(u => u.PhysicalLimitations)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.LastUpdated = DateTime.UtcNow;

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile)
    {
        profile.LastUpdated = DateTime.UtcNow;

        _context.Entry(profile).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<List<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .OrderByDescending(u => u.LastUpdated)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var profile = await _context.UserProfiles.FindAsync(userId);
        if (profile == null) return false;

        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfile?> GetByNameAsync(string name)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(u => u.Name.ToLower() == name.ToLower());
    }

    public async Task<List<UserProfile>> GetByAgeRangeAsync(int minAge, int maxAge)
    {
        return await _context.UserProfiles
            .Where(u => u.Age >= minAge && u.Age <= maxAge)
            .ToListAsync();
    }

    public async Task<List<UserProfile>> GetByFitnessLevelAsync(string fitnessLevel)
    {
        return await _context.UserProfiles
            .Where(u => u.FitnessLevel == fitnessLevel)
            .ToListAsync();
    }

    // User Routine methods
    public async Task<UserRoutine> GetUserRoutineByIdAsync(int routineId)
    {
        var routine = await _context.UserRoutines
            .Include(r => r.User)
            .Include(r => r.Modifications)
            .FirstOrDefaultAsync(r => r.Id == routineId);

        if (routine == null)
            throw new ArgumentException($"UserRoutine with ID {routineId} not found");

        return routine;
    }

    public async Task<UserRoutine> CreateUserRoutineAsync(UserRoutine routine)
    {
        routine.CreatedAt = DateTime.UtcNow;

        _context.UserRoutines.Add(routine);
        await _context.SaveChangesAsync();
        return routine;
    }

    public async Task<UserRoutine> UpdateUserRoutineAsync(UserRoutine routine)
    {
        _context.Entry(routine).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return routine;
    }

    public async Task<List<UserRoutine>> GetUserRoutineHistoryAsync(int userId, int maxCount)
    {
        return await _context.UserRoutines
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(maxCount)
            .ToListAsync();
    }

    // Physical Limitations
    public async Task<List<CoreModels.UserPhysicalLimitation>> GetUserPhysicalLimitationsAsync(int userId)
    {
        // Convert from Data entities to Core models
        // For now, return empty list - TODO: implement conversion
        return new List<CoreModels.UserPhysicalLimitation>();
    }

    // Equipment Preferences
    public async Task<List<CoreModels.UserEquipmentPreference>> GetUserEquipmentPreferencesAsync(int userId)
    {
        // Convert from Data entities to Core models
        // For now, return empty list - TODO: implement conversion
        return new List<CoreModels.UserEquipmentPreference>();
    }

    // Muscle Group Preferences
    public async Task<List<CoreModels.UserMuscleGroupPreference>> GetUserMuscleGroupPreferencesAsync(int userId)
    {
        // Convert from Data entities to Core models
        // For now, return empty list - TODO: implement conversion
        return new List<CoreModels.UserMuscleGroupPreference>();
    }

    // Conversation methods
    public async Task SaveConversationSessionAsync(CoreServices.ConversationSession session)
    {
        // For now, implement as a no-op or simple storage
        // TODO: Add ConversationSession entity to context
        await Task.CompletedTask;
    }

    public async Task SaveConversationTurnAsync(CoreServices.ConversationTurn turn)
    {
        // For now, implement as a no-op or simple storage
        // TODO: Add ConversationTurn entity to context
        await Task.CompletedTask;
    }

    public async Task<List<CoreServices.ConversationTurn>> GetConversationHistoryAsync(string sessionId, int maxTurns)
    {
        // For now, return empty list
        // TODO: Implement proper conversation storage
        return new List<CoreServices.ConversationTurn>();
    }

    public async Task UpdateConversationSessionAsync(CoreServices.ConversationSession session)
    {
        // For now, implement as a no-op
        // TODO: Add ConversationSession entity to context
        await Task.CompletedTask;
    }

    public async Task<CoreServices.ConversationSession?> GetActiveConversationSessionAsync(int userId)
    {
        // For now, return null
        // TODO: Implement proper conversation storage
        return null;
    }

    public async Task<CoreServices.ConversationSession?> GetConversationSessionAsync(string sessionId)
    {
        // For now, return null
        // TODO: Implement proper conversation storage
        return null;
    }

    public async Task<List<CoreServices.ConversationSession>> GetUserConversationSessionsAsync(int userId, int maxSessions)
    {
        // For now, return empty list
        // TODO: Implement proper conversation storage
        return new List<CoreServices.ConversationSession>();
    }

    // Modification history
    public async Task SaveRoutineModificationAsync(RoutineModification historyEntry)
    {
        _context.RoutineModifications.Add(historyEntry);
        await _context.SaveChangesAsync();
    }
}