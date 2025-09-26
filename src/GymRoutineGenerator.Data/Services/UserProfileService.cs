using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public class UserProfileService : IUserProfileService
{
    private readonly GymRoutineContext _context;

    public UserProfileService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<UserProfile> CreateUserProfileAsync(UserProfileCreateRequest request)
    {
        var validation = await ValidateUserProfileAsync(request);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Datos de perfil inválidos: {string.Join(", ", validation.Errors)}");
        }

        var userProfile = new UserProfile
        {
            Name = request.Name.Trim(),
            Gender = request.Gender,
            Age = request.Age,
            TrainingDaysPerWeek = request.TrainingDaysPerWeek,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        return userProfile;
    }

    public async Task<UserProfile?> GetUserProfileByIdAsync(int id)
    {
        return await _context.UserProfiles
            .Include(up => up.EquipmentPreferences)
                .ThenInclude(ep => ep.EquipmentType)
            .Include(up => up.MuscleGroupPreferences)
                .ThenInclude(mgp => mgp.MuscleGroup)
            .Include(up => up.PhysicalLimitations)
            .FirstOrDefaultAsync(up => up.Id == id);
    }

    public async Task<List<UserProfile>> GetAllUserProfilesAsync()
    {
        return await _context.UserProfiles
            .OrderBy(up => up.Name)
            .ToListAsync();
    }

    public async Task<UserProfile> UpdateUserProfileAsync(UserProfileUpdateRequest request)
    {
        var userProfile = await _context.UserProfiles.FindAsync(request.Id);
        if (userProfile == null)
        {
            throw new ArgumentException($"Perfil de usuario con ID {request.Id} no encontrado");
        }

        var validation = await ValidateUserProfileAsync(new UserProfileCreateRequest
        {
            Name = request.Name,
            Gender = request.Gender,
            Age = request.Age,
            TrainingDaysPerWeek = request.TrainingDaysPerWeek
        });

        if (!validation.IsValid)
        {
            throw new ArgumentException($"Datos de perfil inválidos: {string.Join(", ", validation.Errors)}");
        }

        userProfile.Name = request.Name.Trim();
        userProfile.Gender = request.Gender;
        userProfile.Age = request.Age;
        userProfile.TrainingDaysPerWeek = request.TrainingDaysPerWeek;
        userProfile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return userProfile;
    }

    public async Task<bool> DeleteUserProfileAsync(int id)
    {
        var userProfile = await _context.UserProfiles.FindAsync(id);
        if (userProfile == null)
        {
            return false;
        }

        _context.UserProfiles.Remove(userProfile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfileValidationResult> ValidateUserProfileAsync(UserProfileCreateRequest request)
    {
        var result = new UserProfileValidationResult { IsValid = true };

        // Validate name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            result.Errors.Add("El nombre es requerido");
            result.IsValid = false;
        }
        else if (request.Name.Trim().Length < 2)
        {
            result.Errors.Add("El nombre debe tener al menos 2 caracteres");
            result.IsValid = false;
        }
        else if (request.Name.Trim().Length > 100)
        {
            result.Errors.Add("El nombre no puede exceder 100 caracteres");
            result.IsValid = false;
        }

        // Validate age
        if (request.Age < 16 || request.Age > 100)
        {
            result.Errors.Add("La edad debe estar entre 16 y 100 años");
            result.IsValid = false;
        }

        // Validate training days
        if (request.TrainingDaysPerWeek < 1 || request.TrainingDaysPerWeek > 7)
        {
            result.Errors.Add("Los días de entrenamiento deben estar entre 1 y 7");
            result.IsValid = false;
        }

        // Validate gender
        if (!Enum.IsDefined(typeof(Gender), request.Gender))
        {
            result.Errors.Add("Género no válido");
            result.IsValid = false;
        }

        // Check for duplicate names
        var existingProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.Name.ToLower() == request.Name.Trim().ToLower());

        if (existingProfile != null)
        {
            result.Errors.Add("Ya existe un perfil con este nombre");
            result.IsValid = false;
        }

        return result;
    }
}