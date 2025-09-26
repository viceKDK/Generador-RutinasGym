using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;

namespace GymRoutineGenerator.Business.Services;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;

    public ExerciseService(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<List<Exercise>> GetAllExercisesAsync()
    {
        return await _exerciseRepository.GetAllAsync();
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
    {
        return await _exerciseRepository.GetByIdAsync(id);
    }
}