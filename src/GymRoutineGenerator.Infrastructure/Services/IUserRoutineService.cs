using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Infrastructure.Services
{
    public interface IUserRoutineService
    {
        // Gestión de rutinas
        Task<List<UserRoutine>> GetUserRoutinesAsync(string userId);
        Task<UserRoutine?> GetUserRoutineByIdAsync(int routineId);
        Task<UserRoutine> SaveRoutineAsync(UserRoutine routine);
        Task<UserRoutine> UpdateRoutineAsync(UserRoutine routine);
        Task<bool> DeleteRoutineAsync(int routineId);

        // Búsqueda y filtrado
        Task<List<UserRoutine>> SearchUserRoutinesAsync(string userId, string searchTerm);
        Task<List<UserRoutine>> GetRoutinesByStatusAsync(string userId, RoutineStatus status);
        Task<List<UserRoutine>> GetFavoriteRoutinesAsync(string userId);

        // Modificaciones con IA
        Task<RoutineModification> SaveModificationAsync(RoutineModification modification);
        Task<List<RoutineModification>> GetRoutineModificationsAsync(int routineId);
        Task<UserRoutine> ApplyModificationAsync(int routineId, int modificationId);

        // Estadísticas
        Task<int> GetUserRoutineCountAsync(string userId);
        Task<UserRoutine?> GetMostRecentRoutineAsync(string userId);
        Task<DateTime?> GetLastGenerationDateAsync(string userId);

        // Conversión y utilidades
        Task<UserRoutine> ConvertWorkoutToUserRoutineAsync(
            string userId,
            string userName,
            List<object> workoutDays,
            Dictionary<string, object> userProfile);
    }
}