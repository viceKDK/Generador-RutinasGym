namespace GymRoutineGenerator.Infrastructure.Images;

public interface IImageService
{
    Task<string> SaveImageAsync(string imagePath, string exerciseName, string position = "default");
    Task<string> SaveImageFromBytesAsync(byte[] imageBytes, string exerciseName, string position = "default", string extension = "jpg");
    Task<byte[]> OptimizeImageAsync(string imagePath, int maxWidth = 800, int quality = 85);
    Task<bool> ValidateImageAsync(string imagePath);
    string GetImagePath(string exerciseName, string position = "default");
    string GetPlaceholderImagePath();
    Task<bool> ImageExistsAsync(string exerciseName, string position = "default");
    Task<List<string>> GetExerciseImagesAsync(string exerciseName);
    Task<bool> DeleteImageAsync(string exerciseName, string position = "default");
    Task EnsureImageDirectoriesExistAsync();
}