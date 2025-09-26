using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services;

public interface IFallbackRoutineService
{
    Task<string> GenerateRuleBasedRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<List<Exercise>> GetRecommendedExercisesAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<string> GenerateBasicRoutineTemplateAsync(string templateType, UserRoutineParameters parameters, CancellationToken cancellationToken = default);
}

