using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GymRoutineGenerator.Infrastructure.Exercises;

public class DocumentExerciseCatalog : IDocumentExerciseCatalog
{
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };

    private readonly Lazy<Dictionary<string, DocumentExerciseRecord>> _records;
    private readonly string? _docsRoot;

    public DocumentExerciseCatalog(string? searchRoot = null)
    {
        var basePath = string.IsNullOrWhiteSpace(searchRoot) ? AppContext.BaseDirectory : searchRoot;
        _docsRoot = ResolveDocsRoot(basePath);
        _records = new Lazy<Dictionary<string, DocumentExerciseRecord>>(LoadRecords, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public DocumentExerciseRecord? FindByName(string name)
    {
        var key = ExerciseNameNormalizer.Normalize(name);
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        var records = _records.Value;
        return records.TryGetValue(key, out var record) ? record : null;
    }

    public IReadOnlyCollection<DocumentExerciseRecord> GetAll()
    {
        return _records.Value.Values.ToList();
    }

    private Dictionary<string, DocumentExerciseRecord> LoadRecords()
    {
        var result = new Dictionary<string, DocumentExerciseRecord>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(_docsRoot) || !Directory.Exists(_docsRoot))
        {
            return result;
        }

        foreach (var muscleDir in Directory.EnumerateDirectories(_docsRoot))
        {
            var muscleName = Path.GetFileName(muscleDir);

            foreach (var exerciseDir in Directory.EnumerateDirectories(muscleDir))
            {
                var exerciseName = Path.GetFileName(exerciseDir);
                var imagePath = Directory.EnumerateFiles(exerciseDir)
                    .FirstOrDefault(IsSupportedImage);

                var description = imagePath is null
                    ? string.Empty
                    : Path.GetFileNameWithoutExtension(imagePath).Replace('_', ' ');

                var record = new DocumentExerciseRecord
                {
                    SpanishName = exerciseName,
                    Description = description,
                    Equipment = string.Empty,
                    MuscleGroups = string.IsNullOrEmpty(muscleName)
                        ? Array.Empty<string>()
                        : new[] { muscleName },
                    ImagePath = imagePath
                };

                AddRecord(result, record, exerciseName);
            }
        }

        return result;
    }

    private static void AddRecord(Dictionary<string, DocumentExerciseRecord> records, DocumentExerciseRecord record, params string?[] names)
    {
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var key = ExerciseNameNormalizer.Normalize(name);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            records.TryAdd(key, record);
        }
    }

    private static string? ResolveDocsRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "docs", "ejercicios");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static bool IsSupportedImage(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        return ImageExtensions.Contains(extension.ToLowerInvariant());
    }
}
