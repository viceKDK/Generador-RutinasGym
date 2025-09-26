namespace GymRoutineGenerator.Data.Import;

public interface IExerciseImportService
{
    Task<ImportResult> ImportFromJsonAsync(string jsonFilePath);
    Task<ImportResult> ImportFromCsvAsync(string csvFilePath);
    Task<ImportResult> ImportFromDataAsync(IEnumerable<ExerciseImportData> exerciseData);
    Task<ImportValidationResult> ValidateImportDataAsync(IEnumerable<ExerciseImportData> exerciseData);
    Task<ImportResult> ImportBulkSeedDataAsync();
}