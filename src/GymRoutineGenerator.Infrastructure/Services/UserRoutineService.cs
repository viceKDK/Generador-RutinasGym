using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using System.Text.Json;

namespace GymRoutineGenerator.Infrastructure.Services
{
    public class UserRoutineService : IUserRoutineService
    {
        private readonly GymRoutineContext _context;

        public UserRoutineService(GymRoutineContext context)
        {
            _context = context;
        }

        public async Task<List<UserRoutine>> GetUserRoutinesAsync(string userId)
        {
            if (int.TryParse(userId, out int intUserId))
                return await GetUserRoutinesAsync(intUserId);
            return new List<UserRoutine>();
        }

        public async Task<List<UserRoutine>> GetUserRoutinesAsync(int userId)
        {
            return await _context.UserRoutines
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.RoutineExercises)
                .Include(ur => ur.Modifications)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserRoutine?> GetUserRoutineByIdAsync(int routineId)
        {
            return await _context.UserRoutines
                .Include(ur => ur.RoutineExercises)
                .Include(ur => ur.Modifications)
                .FirstOrDefaultAsync(ur => ur.Id == routineId);
        }

        public async Task<UserRoutine> SaveRoutineAsync(UserRoutine routine)
        {
            try
            {
                routine.CreatedAt = DateTime.Now;
                _context.UserRoutines.Add(routine);
                await _context.SaveChangesAsync();
                return routine;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving routine: {ex.Message}");
                throw;
            }
        }

        public async Task<UserRoutine> UpdateRoutineAsync(UserRoutine routine)
        {
            try
            {
                routine.LastModified = DateTime.Now;
                _context.UserRoutines.Update(routine);
                await _context.SaveChangesAsync();
                return routine;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating routine: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteRoutineAsync(int routineId)
        {
            try
            {
                var routine = await _context.UserRoutines.FindAsync(routineId);
                if (routine == null) return false;

                _context.UserRoutines.Remove(routine);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting routine: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserRoutine>> SearchUserRoutinesAsync(string userId, string searchTerm)
        {
            if (int.TryParse(userId, out int intUserId))
                return await SearchUserRoutinesAsync(intUserId, searchTerm);
            return new List<UserRoutine>();
        }

        public async Task<List<UserRoutine>> SearchUserRoutinesAsync(int userId, string searchTerm)
        {
            return await _context.UserRoutines
                .Where(ur => ur.UserId == userId &&
                           (ur.UserName.Contains(searchTerm) ||
                            ur.Notes.Contains(searchTerm) ||
                            ur.Goals.Any(g => g.Contains(searchTerm))))
                .Include(ur => ur.RoutineExercises)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserRoutine>> GetRoutinesByStatusAsync(string userId, RoutineStatus status)
        {
            var userIdInt = int.Parse(userId);
            var statusString = status.ToString();
            return await _context.UserRoutines
                .Where(ur => ur.UserId == userIdInt && ur.Status == statusString)
                .Include(ur => ur.RoutineExercises)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserRoutine>> GetFavoriteRoutinesAsync(string userId)
        {
            var userIdInt = int.Parse(userId);
            return await _context.UserRoutines
                .Where(ur => ur.UserId == userIdInt && ur.IsFavorite)
                .Include(ur => ur.RoutineExercises)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<RoutineModification> SaveModificationAsync(RoutineModification modification)
        {
            try
            {
                modification.ModifiedAt = DateTime.Now;
                _context.RoutineModifications.Add(modification);
                await _context.SaveChangesAsync();
                return modification;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving modification: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RoutineModification>> GetRoutineModificationsAsync(int routineId)
        {
            return await _context.RoutineModifications
                .Where(rm => rm.UserRoutineId == routineId)
                .OrderByDescending(rm => rm.ModifiedAt)
                .ToListAsync();
        }

        public async Task<UserRoutine> ApplyModificationAsync(int routineId, int modificationId)
        {
            try
            {
                var routine = await GetUserRoutineByIdAsync(routineId);
                var modification = await _context.RoutineModifications.FindAsync(modificationId);

                if (routine == null || modification == null)
                    throw new ArgumentException("Routine or modification not found");

                // Aplicar la modificación (implementar lógica específica según el tipo)
                ApplyModificationToRoutine(routine, modification);

                // Marcar modificación como aplicada
                modification.WasApplied = true;
                routine.LastModified = DateTime.Now;

                await _context.SaveChangesAsync();
                return routine;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying modification: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetUserRoutineCountAsync(string userId)
        {
            var userIdInt = int.Parse(userId);
            return await _context.UserRoutines
                .CountAsync(ur => ur.UserId == userIdInt);
        }

        public async Task<UserRoutine?> GetMostRecentRoutineAsync(string userId)
        {
            var userIdInt = int.Parse(userId);
            return await _context.UserRoutines
                .Where(ur => ur.UserId == userIdInt)
                .Include(ur => ur.RoutineExercises)
                .OrderByDescending(ur => ur.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> GetLastGenerationDateAsync(string userId)
        {
            var userIdInt = int.Parse(userId);
            var lastRoutine = await _context.UserRoutines
                .Where(ur => ur.UserId == userIdInt)
                .OrderByDescending(ur => ur.CreatedAt)
                .FirstOrDefaultAsync();

            return lastRoutine?.CreatedAt;
        }

        public async Task<UserRoutine> ConvertWorkoutToUserRoutineAsync(
            string userId,
            string userName,
            List<object> workoutDays,
            Dictionary<string, object> userProfile)
        {
            try
            {
                var routine = new UserRoutine
                {
                    UserId = int.TryParse(userId, out int userIdInt) ? userIdInt : 0,
                    UserName = userName,
                    Age = Convert.ToInt32(userProfile.GetValueOrDefault("Age", 25)),
                    Gender = userProfile.GetValueOrDefault("Gender", "").ToString() ?? "",
                    FitnessLevel = userProfile.GetValueOrDefault("FitnessLevel", "").ToString() ?? "",
                    TrainingDays = Convert.ToInt32(userProfile.GetValueOrDefault("TrainingDays", 3)),
                    Goals = userProfile.GetValueOrDefault("Goals", new List<string>()) as List<string> ?? new List<string>(),
                    RoutineContent = JsonSerializer.Serialize(workoutDays),
                    Status = RoutineStatus.ACTIVE.ToString()
                };

                // Convertir días de entrenamiento a RoutineExercise entities
                for (int dayIndex = 0; dayIndex < workoutDays.Count; dayIndex++)
                {
                    var day = workoutDays[dayIndex];
                    var dayDict = day as Dictionary<string, object>;
                    if (dayDict == null) continue;

                    var dayName = dayDict.GetValueOrDefault("Name", $"Día {dayIndex + 1}").ToString() ?? "";
                    var exercises = dayDict.GetValueOrDefault("Exercises", new List<object>()) as List<object>;
                    if (exercises == null) continue;

                    for (int exerciseIndex = 0; exerciseIndex < exercises.Count; exerciseIndex++)
                    {
                        var exercise = exercises[exerciseIndex] as Dictionary<string, object>;
                        if (exercise == null) continue;

                        var routineExercise = new RoutineExercise
                        {
                            UserRoutine = routine,
                            ExerciseName = exercise.GetValueOrDefault("Name", "").ToString() ?? "",
                            DayNumber = dayIndex + 1,
                            DayName = dayName,
                            OrderInDay = exerciseIndex + 1,
                            SetsAndReps = exercise.GetValueOrDefault("SetsAndReps", "").ToString() ?? "",
                            Instructions = exercise.GetValueOrDefault("Instructions", "").ToString() ?? "",
                            ImageInfo = exercise.GetValueOrDefault("ImageUrl", "").ToString() ?? "",
                            IsCustomExercise = false
                        };

                        routine.RoutineExercises.Add(routineExercise);
                    }
                }

                return await SaveRoutineAsync(routine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting workout to user routine: {ex.Message}");
                throw;
            }
        }

        private void ApplyModificationToRoutine(UserRoutine routine, RoutineModification modification)
        {
            // Implementar lógica para aplicar diferentes tipos de modificaciones
            // Por ahora, simplemente actualizar las notas
            routine.Notes += $"\n[{modification.ModifiedAt:yyyy-MM-dd HH:mm}] {modification.UserRequest}";

            // TODO: Implementar lógica específica para cada tipo de modificación
            // - ExerciseChange: cambiar un ejercicio por otro
            // - IntensityChange: modificar series/repeticiones
            // - AddExercise: agregar nuevo ejercicio
            // - RemoveExercise: quitar ejercicio
        }
    }
}