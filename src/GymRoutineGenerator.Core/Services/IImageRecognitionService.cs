using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface IImageRecognitionService
    {
        Task<ImageRecognitionResult> AnalyzeExerciseImageAsync(byte[] imageData);
        Task<List<Exercise>> FindSimilarExercisesAsync(byte[] imageData);
        Task<Exercise?> IdentifyExerciseAsync(string imagePath);
        Task<bool> ValidateExerciseImageAsync(byte[] imageData);
        Task<List<string>> ExtractExerciseFeaturesAsync(byte[] imageData);
    }
}