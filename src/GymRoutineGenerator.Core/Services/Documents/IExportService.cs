using System;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;

namespace GymRoutineGenerator.Core.Services.Documents;

public interface IExportService
{
    Task<ExportResult> ExportRoutineToWordAsync(
        Routine routine,
        string templateId,
        ExportOptions options,
        IProgress<ExportProgress>? progress = null);

    Task<ExportResult> ExportMultipleRoutinesToWordAsync(
        IEnumerable<Routine> routines,
        string templateId,
        ExportOptions options,
        IProgress<ExportProgress>? progress = null);

    Task<string> GetSuggestedFilenameAsync(Routine routine, string extension = "docx");
    Task<string> GetDefaultExportPathAsync();
    Task SetDefaultExportPathAsync(string path);
}

public class ExportOptions
{
    public string? OutputPath { get; set; }
    public string? CustomFilename { get; set; }
    public bool AutoOpenAfterExport { get; set; } = true;
    public bool OverwriteExisting { get; set; } = false;
    public bool CreateBackup { get; set; } = false;
    public ExportFormat Format { get; set; } = ExportFormat.Word;
    public CompressionLevel ImageCompression { get; set; } = CompressionLevel.Medium;
}

public enum ExportFormat
{
    Word,
    Pdf,
    Html
}

public enum CompressionLevel
{
    None,
    Low,
    Medium,
    High
}

public class ExportResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan ExportDuration { get; set; }
    public int ExerciseCount { get; set; }
    public int ImageCount { get; set; }
}

public class ExportProgress
{
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string CurrentOperation { get; set; } = string.Empty;
    public double PercentComplete => TotalSteps > 0 ? (double)CurrentStep / TotalSteps * 100 : 0;
}