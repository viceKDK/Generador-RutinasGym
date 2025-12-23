using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GymRoutineGenerator.Domain;
using GymRoutineGenerator.Domain.Models;
using GymRoutineGenerator.Infrastructure;

namespace GymRoutineGenerator.Services
{
    /// <summary>
    /// Resolves exercises and associated images by combining the primary SQLite database,
    /// the secondary fallback database and the filesystem cache. Muscle group names are
    /// kept in Spanish; English variants are only used as optional synonyms for matching.
    /// </summary>
    public class ExerciseImageSearchService
    {
        private readonly SQLiteExerciseImageDatabase _primaryDatabase;
        private readonly AutomaticImageFinder _automaticFinder;

        private static readonly object InitializationLock = new();
        private static readonly object CacheLock = new();

        private static bool _initialized;
        private static List<ExerciseWithImage> _primaryExercises = new();
        private static Dictionary<string, ExerciseWithImage> _exerciseLookup = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, ExerciseWithImage> _resultCache = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _canonicalBySynonym = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<string>> _synonymsLookup = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Regex NonAlphanumericRegex = new("[^a-z0-9\\s]", RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesRegex = new("\\s+", RegexOptions.Compiled);

        public ExerciseImageSearchService()
        {
            _primaryDatabase = new SQLiteExerciseImageDatabase();
            _automaticFinder = new AutomaticImageFinder();
            EnsureInitialized();
        }

        /// <summary>
        /// Returns the list of exercises that belong to the requested muscle group.
        /// </summary>
        public List<ExerciseWithImage> GetExercisesByMuscleGroup(string muscleGroup)
        {
            if (string.IsNullOrWhiteSpace(muscleGroup))
            {
                return new List<ExerciseWithImage>();
            }

            EnsureInitialized();

            var canonical = GetCanonicalMuscleGroup(muscleGroup) ?? muscleGroup.Trim();
            var normalizedCanonical = NormalizeKey(canonical);

            var results = new List<ExerciseWithImage>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Primaria
            foreach (var exercise in _primaryExercises)
            {
                if (exercise.MuscleGroups.Any(group => AreSameMuscleGroup(group, canonical)))
                {
                    if (seen.Add(exercise.Name))
                    {
                        results.Add(CloneExercise(exercise));
                    }
                }
            }

            if (results.Count < 5)
            {
                foreach (var exercise in _primaryExercises)
                {
                    if (seen.Contains(exercise.Name))
                    {
                        continue;
                    }

                    var normalizedName = NormalizeKey(exercise.Name);
                    if (!string.IsNullOrEmpty(normalizedName) &&
                        normalizedName.Contains(normalizedCanonical))
                    {
                        seen.Add(exercise.Name);
                        results.Add(CloneExercise(exercise));
                    }
                }
            }

            return results
                .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Finds information for a specific exercise, including image data wherever possible.
        /// </summary>
        public ExerciseWithImage? FindExerciseWithImage(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return null;
            }

            EnsureInitialized();

            var normalized = NormalizeKey(exerciseName);
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            lock (CacheLock)
            {
                if (_resultCache.TryGetValue(normalized, out var cached))
                {
                    return CloneExercise(cached);
                }
            }

            var variants = GenerateNameVariants(exerciseName).ToList();

            foreach (var variant in variants)
            {
                var metadata = FindMetadata(variant);
                if (metadata == null)
                {
                    continue;
                }

                var resolved = CloneExercise(metadata);

                ResolveImageFromPrimarySources(variant, resolved);
                if (!HasImage(resolved))
                {
                    ResolveImageFromFilesystem(variant, resolved);
                }

                if (HasImage(resolved))
                {
                    CacheResultForNames(resolved, variants);
                    return CloneExercise(resolved);
                }
            }

            foreach (var variant in variants)
            {
                var info = _primaryDatabase.FindExerciseImage(variant);

                if (info != null)
                {
                    var metadata = FindMetadata(info.Name ?? info.ExerciseName ?? variant);

                    var resolved = new ExerciseWithImage
                    {
                        Name = metadata?.Name ?? info.ExerciseName ?? variant,
                        EnglishName = metadata?.EnglishName ?? info.Name ?? variant,
                        Description = metadata?.Description ?? string.Empty,
                        MuscleGroups = metadata?.MuscleGroups?.Length > 0 ? metadata.MuscleGroups : InferMuscleGroups(variant, info.ImagePath),
                        ImageData = info.ImageData,
                        ImagePath = info.ImagePath ?? string.Empty,
                        Source = info.Source ?? "BD Principal",
                        Keywords = metadata?.Keywords ?? Array.Empty<string>()
                    };

                    CacheResultForNames(resolved, variants);
                    return CloneExercise(resolved);
                }

                var autoPath = _automaticFinder.FindImageForExercise(variant);
                if (!string.IsNullOrEmpty(autoPath))
                {
                    var metadata = FindMetadata(variant);

                    var resolved = new ExerciseWithImage
                    {
                        Name = metadata?.Name ?? variant,
                        EnglishName = metadata?.EnglishName ?? variant,
                        Description = metadata?.Description ?? string.Empty,
                        MuscleGroups = metadata?.MuscleGroups?.Length > 0 ? metadata.MuscleGroups : InferMuscleGroups(variant, autoPath),
                        ImagePath = autoPath,
                        Source = "Sistema de archivos",
                        Keywords = metadata?.Keywords ?? Array.Empty<string>()
                    };

                    CacheResultForNames(resolved, variants);
                    return CloneExercise(resolved);
                }
            }

            return null;
        }

        private void ResolveImageFromPrimarySources(string variant, ExerciseWithImage resolved)
        {
            var info = _primaryDatabase.FindExerciseImage(resolved.Name);
            if (info == null && !string.Equals(resolved.Name, variant, StringComparison.OrdinalIgnoreCase))
            {
                info = _primaryDatabase.FindExerciseImage(variant);
            }

            if (info != null)
            {
                resolved.ImageData = info.ImageData ?? resolved.ImageData;
                resolved.ImagePath = !string.IsNullOrEmpty(info.ImagePath) ? info.ImagePath : resolved.ImagePath;
                resolved.Source = info.Source ?? "BD Principal";

                if (resolved.MuscleGroups.Length == 0)
                {
                    var metadata = FindMetadata(info.Name ?? info.ExerciseName ?? resolved.Name);
                    if (metadata?.MuscleGroups?.Length > 0)
                    {
                        resolved.MuscleGroups = metadata.MuscleGroups;
                    }
                }
            }
        }

        private void ResolveImageFromFilesystem(string variant, ExerciseWithImage resolved)
        {
            var path = _automaticFinder.FindImageForExercise(resolved.Name);
            if (string.IsNullOrEmpty(path) && !string.Equals(resolved.Name, variant, StringComparison.OrdinalIgnoreCase))
            {
                path = _automaticFinder.FindImageForExercise(variant);
            }

            if (!string.IsNullOrEmpty(path))
            {
                resolved.ImagePath = path;
                resolved.Source = "Sistema de archivos";

                if (resolved.MuscleGroups.Length == 0)
                {
                    resolved.MuscleGroups = InferMuscleGroups(resolved.Name, path);
                }
            }
        }

        private static ExerciseWithImage CloneExercise(ExerciseWithImage exercise)
        {
            return new ExerciseWithImage
            {
                Name = exercise.Name,
                EnglishName = exercise.EnglishName,
                Description = exercise.Description,
                MuscleGroups = exercise.MuscleGroups.ToArray(),
                ImagePath = exercise.ImagePath,
                ImageData = exercise.ImageData,
                Source = exercise.Source,
                Keywords = exercise.Keywords.ToArray()
            };
        }

        private static bool HasImage(ExerciseWithImage exercise)
        {
            if (exercise.ImageData != null && exercise.ImageData.Length > 0)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(exercise.ImagePath) && File.Exists(exercise.ImagePath);
        }

        private ExerciseWithImage? FindMetadata(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var normalized = NormalizeKey(name);
            return _exerciseLookup.TryGetValue(normalized, out var exercise) ? exercise : null;
        }

        private void CacheResultForNames(ExerciseWithImage resolved, IEnumerable<string> names)
        {
            var normalizedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in names.Append(resolved.Name).Append(resolved.EnglishName ?? string.Empty))
            {
                var normalized = NormalizeKey(name);
                if (!string.IsNullOrEmpty(normalized))
                {
                    normalizedKeys.Add(normalized);
                }
            }

            lock (CacheLock)
            {
                foreach (var key in normalizedKeys)
                {
                    _resultCache[key] = resolved;
                }
            }
        }

        private string[] InferMuscleGroups(string exerciseName, string? imagePath)
        {
            var metadata = FindMetadata(exerciseName);
            if (metadata?.MuscleGroups?.Length > 0)
            {
                return metadata.MuscleGroups;
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    var directory = new DirectoryInfo(Path.GetDirectoryName(imagePath)!);
                    while (directory != null)
                    {
                        var canonical = GetCanonicalMuscleGroup(directory.Name);
                        if (!string.IsNullOrEmpty(canonical))
                        {
                            return new[] { canonical };
                        }

                        if (string.Equals(directory.Name, "ejercicios", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        directory = directory.Parent;
                    }
                }
                catch
                {
                    // Ignorar errores de inferencia
                }
            }

            var canonicalFromName = GetCanonicalMuscleGroup(exerciseName);
            return canonicalFromName != null ? new[] { canonicalFromName } : Array.Empty<string>();
        }

        private static IEnumerable<string> GenerateNameVariants(string name)
        {
            var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void Add(string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    variants.Add(value.Trim());
                }
            }

            Add(name);

            var withoutParenthesis = Regex.Replace(name, @"\s*\(.*?\)\s*", " ").Trim();
            Add(withoutParenthesis);

            if (name.Contains('-'))
            {
                Add(name.Split('-')[0]);
            }

            if (name.Contains(':'))
            {
                Add(name.Split(':')[0]);
            }

            if (name.Contains(','))
            {
                Add(name.Split(',')[0]);
            }

            var withoutWith = Regex.Replace(name, @"\bcon\b.*", "", RegexOptions.IgnoreCase).Trim();
            Add(withoutWith);

            var normalized = NormalizeKey(name);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                Add(normalized);
                var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 2)
                {
                    Add(string.Join(' ', words.Take(2)));
                }
            }

            return variants;
        }

        private static ExerciseWithImage MapFromPrimary(ExerciseImageInfo info)
        {
            var spanishName = !string.IsNullOrWhiteSpace(info.ExerciseName)
                ? info.ExerciseName
                : info.Name ?? string.Empty;

            var englishName = !string.IsNullOrWhiteSpace(info.Name) && !string.Equals(info.Name, spanishName, StringComparison.OrdinalIgnoreCase)
                ? info.Name
                : string.Empty;

            return new ExerciseWithImage
            {
                Name = spanishName,
                EnglishName = englishName,
                Description = info.Description ?? string.Empty,
                ImageData = info.ImageData,
                ImagePath = info.ImagePath ?? string.Empty,
                Source = info.Source ?? "BD Principal",
                MuscleGroups = info.MuscleGroups?.Where(m => !string.IsNullOrWhiteSpace(m)).ToArray() ?? Array.Empty<string>(),
                Keywords = info.Keywords ?? Array.Empty<string>()
            };
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (InitializationLock)
            {
                if (_initialized)
                {
                    return;
                }

                _canonicalBySynonym = BuildCanonicalIndex(out _synonymsLookup);

                try
                {
                    var fromDb = _primaryDatabase.GetAllExercises();
                    _primaryExercises = fromDb.Select(MapFromPrimary).ToList();

                    _exerciseLookup = new Dictionary<string, ExerciseWithImage>(StringComparer.OrdinalIgnoreCase);
                    foreach (var exercise in _primaryExercises)
                    {
                        foreach (var key in GetNameVariantsForIndex(exercise))
                        {
                            if (!_exerciseLookup.ContainsKey(key))
                            {
                                _exerciseLookup[key] = exercise;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExerciseImageSearchService] Error precargando ejercicios: {ex.Message}");
                    _primaryExercises = new List<ExerciseWithImage>();
                    _exerciseLookup = new Dictionary<string, ExerciseWithImage>(StringComparer.OrdinalIgnoreCase);
                }

                _resultCache = new Dictionary<string, ExerciseWithImage>(StringComparer.OrdinalIgnoreCase);
                _initialized = true;
            }
        }

        private static IEnumerable<string> GetNameVariantsForIndex(ExerciseWithImage exercise)
        {
            var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(exercise.Name))
            {
                var normalized = NormalizeKey(exercise.Name);
                if (!string.IsNullOrEmpty(normalized))
                {
                    variants.Add(normalized);
                }
            }

            if (!string.IsNullOrWhiteSpace(exercise.EnglishName))
            {
                var normalized = NormalizeKey(exercise.EnglishName);
                if (!string.IsNullOrEmpty(normalized))
                {
                    variants.Add(normalized);
                }
            }

            foreach (var keyword in exercise.Keywords ?? Array.Empty<string>())
            {
                var normalized = NormalizeKey(keyword);
                if (!string.IsNullOrEmpty(normalized))
                {
                    variants.Add(normalized);
                }
            }

            return variants;
        }

        private static Dictionary<string, string> BuildCanonicalIndex(out Dictionary<string, HashSet<string>> synonymsByCanonical)
        {
            var canonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var synonymsLookup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            void Register(string canonicalName, params string[] synonyms)
            {
                var normalizedCanonical = NormalizeKey(canonicalName);
                if (string.IsNullOrEmpty(normalizedCanonical))
                {
                    return;
                }

                if (!synonymsLookup.TryGetValue(canonicalName, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    synonymsLookup[canonicalName] = set;
                }

                set.Add(canonicalName);
                canonical[normalizedCanonical] = canonicalName;

                foreach (var synonym in synonyms)
                {
                    var normalizedSynonym = NormalizeKey(synonym);
                    if (string.IsNullOrEmpty(normalizedSynonym))
                    {
                        continue;
                    }

                    canonical[normalizedSynonym] = canonicalName;
                    set.Add(synonym);
                }
            }

            Register("Pecho", "Pectorales", "Torax", "Chest", "Upper Chest");
            Register("Espalda", "Back", "Dorsales", "Lat", "Lats", "Latissimus dorsi");
            Register("Piernas", "Legs", "Lower Body", "Quadriceps", "Quads", "Isquiotibiales", "Hamstrings", "Femoral");
            Register("Core", "Abdominales", "Abs", "Abdomen", "Midsection");
            Register("Hombros", "Deltoides", "Shoulders", "Delts");
            Register("Brazos", "Arms", "Biceps", "Triceps", "Antebrazos", "Forearms");
            Register("Gluteos", "Glutes", "Gluteus");
            Register("Pantorrillas", "Pantorrilla", "Gemelos", "Calf", "Calves");
            Register("Cuerpo Completo", "Full Body", "Total Body");
            Register("Cardio", "Aerobico", "Resistencia");
            Register("Trapecios", "Traps", "Trapecio");
            Register("Lumbar", "Espalda baja", "Lower Back");
            Register("Cuello", "Neck");
            Register("Movilidad", "Mobility");
            Register("Calentamiento", "Warm Up", "Warmup");
            Register("Estiramiento", "Stretching", "Stretch");

            synonymsByCanonical = synonymsLookup;
            return canonical;
        }

        private string? GetCanonicalMuscleGroup(string value)
        {
            var normalized = NormalizeKey(value);
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            return _canonicalBySynonym.TryGetValue(normalized, out var canonical) ? canonical : null;
        }

        private bool AreSameMuscleGroup(string groupA, string groupB)
        {
            var canonicalA = GetCanonicalMuscleGroup(groupA) ?? NormalizeKey(groupA);
            var canonicalB = GetCanonicalMuscleGroup(groupB) ?? NormalizeKey(groupB);

            if (string.IsNullOrEmpty(canonicalA) || string.IsNullOrEmpty(canonicalB))
            {
                return string.Equals(canonicalA, canonicalB, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(canonicalA, canonicalB, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetMuscleGroupSearchTerms(string canonicalGroup)
        {
            if (_synonymsLookup.TryGetValue(canonicalGroup, out var synonyms) && synonyms.Count > 0)
            {
                foreach (var synonym in synonyms)
                {
                    yield return synonym;
                }
            }
            else
            {
                yield return canonicalGroup;
            }
        }

        private static string NormalizeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var lower = value.Trim().ToLowerInvariant();
            var normalized = lower.Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            var cleaned = builder.ToString();
            cleaned = NonAlphanumericRegex.Replace(cleaned, " ");
            cleaned = MultipleSpacesRegex.Replace(cleaned, " ").Trim();

            return cleaned;
        }
    }

    /// <summary>
    /// Simple DTO containing exercise metadata plus optional image data/path information.
    /// </summary>
    public class ExerciseWithImage
    {
        public string Name { get; set; } = string.Empty;
        public string? EnglishName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string[] MuscleGroups { get; set; } = Array.Empty<string>();
        public string ImagePath { get; set; } = string.Empty;
        public byte[]? ImageData { get; set; }
        public string Source { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
    }
}
