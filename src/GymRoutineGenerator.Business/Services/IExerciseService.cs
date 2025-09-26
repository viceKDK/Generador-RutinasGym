using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Business.Services;

public interface IExerciseService
{
    Task<List<Exercise>> GetAllExercisesAsync();
    Task<Exercise?> GetExerciseByIdAsync(int id);
}