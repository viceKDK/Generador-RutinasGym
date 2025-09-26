using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly GymRoutineContext _context;

    public ExerciseRepository(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _context.Exercises.ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _context.Exercises.FindAsync(id);
    }

    public async Task<Exercise> AddAsync(Exercise exercise)
    {
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return exercise;
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        _context.Exercises.Update(exercise);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise != null)
        {
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
        }
    }
}