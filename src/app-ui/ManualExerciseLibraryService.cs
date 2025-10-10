using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GymRoutineGenerator.UI.Models;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Provides manual lookup helpers for the exercise gallery, wrapping the existing
    /// search service and exposing UI-friendly DTOs and utilities.
    /// </summary>
    public sealed class ManualExerciseLibraryService : IDisposable
    {
        private readonly ExerciseImageSearchService _searchService;
        private readonly SQLiteExerciseImageDatabase _primaryDatabase;
        private readonly SecondaryExerciseDatabase _secondaryDatabase;
        private readonly Lazy<IReadOnlyList<ExerciseIndexEntry>> _primaryIndex;
        private readonly Lazy<IReadOnlyList<ExerciseIndexEntry>> _secondaryIndex;

        private readonly object _cacheLock = new();
        private readonly Dictionary<string, Bitmap> _thumbnailCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> _cacheOrder = new();
        private readonly int _cacheCapacity;

        private static readonly Regex NonAlphaNumericRegex = new("[^a-z0-9\\s]", RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesRegex = new("\\s+", RegexOptions.Compiled);

        public ManualExerciseLibraryService(
            ExerciseImageSearchService? searchService = null,
            SQLiteExerciseImageDatabase? primaryDatabase = null,
            SecondaryExerciseDatabase? secondaryDatabase = null,
            int thumbnailCacheCapacity = 100)
        {
            _searchService = searchService ?? new ExerciseImageSearchService();
            _primaryDatabase = primaryDatabase ?? new SQLiteExerciseImageDatabase();
            _secondaryDatabase = secondaryDatabase ?? new SecondaryExerciseDatabase();
            _cacheCapacity = Math.Max(10, thumbnailCacheCapacity);

            _primaryIndex = new Lazy<IReadOnlyList<ExerciseIndexEntry>>(BuildPrimaryIndex, LazyThreadSafetyMode.ExecutionAndPublication);
            _secondaryIndex = new Lazy<IReadOnlyList<ExerciseIndexEntry>>(BuildSecondaryIndex, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Searches exercises by name, matching partial tokens regardless of casing or diacritics.
        /// </summary>
        public IReadOnlyList<ExerciseGalleryItem> Search(
            string query,
            ManualExerciseDataSource dataSource = ManualExerciseDataSource.Primary,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<ExerciseGalleryItem>();
            }

            var normalizedQuery = Normalize(query);
            if (string.IsNullOrEmpty(normalizedQuery))
            {
                return Array.Empty<ExerciseGalleryItem>();
            }

            var results = new List<ExerciseGalleryItem>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in EnumerateIndex(dataSource))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsMatch(entry, query, normalizedQuery))
                {
                    continue;
                }

                var item = CreateGalleryItem(entry);
                if (item == null)
                {
                    continue;
                }

                if (seenIds.Add(item.Id))
                {
                    results.Add(item);
                }
            }

            return results
                .OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Returns the resolved absolute image path for an exercise, if available.
        /// </summary>
        public string? GetImagePath(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return null;
            }

            var metadata = _searchService.FindExerciseWithImage(exerciseName);
            if (!string.IsNullOrWhiteSpace(metadata?.ImagePath))
            {
                var candidate = Path.GetFullPath(metadata.ImagePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            var secondary = _secondaryDatabase.FindExerciseImage(exerciseName);
            if (!string.IsNullOrWhiteSpace(secondary?.ImagePath))
            {
                var candidate = Path.GetFullPath(secondary.ImagePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the best known image path for a gallery item.
        /// </summary>
        public string? GetImagePath(ExerciseGalleryItem item)
        {
            if (item == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(item.ImagePath))
            {
                var candidate = Path.GetFullPath(item.ImagePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            foreach (var name in new[] { item.Name, item.EnglishName, item.DisplayName })
            {
                var candidate = GetImagePath(name ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to copy the exercise image to the clipboard, handling STA requirements.
        /// </summary>
        public bool TryCopyImageToClipboard(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return false;
            }

            var metadata = _searchService.FindExerciseWithImage(exerciseName);
            if (metadata == null)
            {
                return false;
            }

            Image? image = null;

            try
            {
                image = ResolveImage(metadata);
                if (image == null)
                {
                    return false;
                }

                ExecuteOnStaThread(() =>
                {
                    Clipboard.SetImage(new Bitmap(image));
                });

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error copying image: {ex.Message}");
                return false;
            }
            finally
            {
                image?.Dispose();
            }
        }

        /// <summary>
        /// Attempts to copy the exercise image to the clipboard using gallery metadata.
        /// </summary>
        public bool TryCopyImageToClipboard(ExerciseGalleryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var path = GetImagePath(item);
            if (string.IsNullOrWhiteSpace(path))
            {
                return TryCopyImageToClipboard(item.DisplayName);
            }

            Image? image = null;
            try
            {
                image = LoadImageFromPath(path);
                if (image == null)
                {
                    return false;
                }

                ExecuteOnStaThread(() =>
                {
                    Clipboard.SetImage(new Bitmap(image));
                });

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error copying image (item): {ex.Message}");
                return false;
            }
            finally
            {
                image?.Dispose();
            }
        }

        /// <summary>
        /// Opens the image location in the Windows Explorer, selecting the file when possible.
        /// </summary>
        public bool TryOpenImageLocation(string exerciseName)
        {
            var path = GetImagePath(exerciseName);
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{path}\"",
                    UseShellExecute = true
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error opening explorer: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens the image location for a gallery item.
        /// </summary>
        public bool TryOpenImageLocation(ExerciseGalleryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var path = GetImagePath(item);
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{path}\"",
                    UseShellExecute = true
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error opening explorer (item): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns a thumbnail image for the gallery UI. The caller owns the returned instance.
        /// </summary>
        public Image? LoadThumbnail(ExerciseGalleryItem item, Size targetSize)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (targetSize.Width <= 0 || targetSize.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetSize), "Target size must be greater than zero.");
            }

            if (!item.HasImage || !File.Exists(item.ImagePath))
            {
                return null;
            }

            var cacheKey = $"{item.Id}|{targetSize.Width}x{targetSize.Height}";

            lock (_cacheLock)
            {
                if (_thumbnailCache.TryGetValue(cacheKey, out var cached))
                {
                    return new Bitmap(cached);
                }
            }

            try
            {
                using var stream = new FileStream(item.ImagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var original = Image.FromStream(stream);
                var resized = ResizeImage(original, targetSize);
                StoreInCache(cacheKey, resized);
                return new Bitmap(resized);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error creating thumbnail: {ex.Message}");
                return null;
            }
        }

        public void ClearThumbnailCache()
        {
            lock (_cacheLock)
            {
                foreach (var bitmap in _thumbnailCache.Values)
                {
                    bitmap.Dispose();
                }

                _thumbnailCache.Clear();
                _cacheOrder.Clear();
            }
        }

        public void Dispose()
        {
            ClearThumbnailCache();
        }

        private IReadOnlyList<ExerciseIndexEntry> BuildPrimaryIndex()
        {
            var list = new List<ExerciseIndexEntry>();
            var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var info in _primaryDatabase.GetAllExercises())
            {
                var name = !string.IsNullOrWhiteSpace(info.ExerciseName)
                    ? info.ExerciseName
                    : !string.IsNullOrWhiteSpace(info.Name) ? info.Name : string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var normalized = Normalize(name);
                if (string.IsNullOrEmpty(normalized) || !dedup.Add(normalized))
                {
                    continue;
                }

                list.Add(new ExerciseIndexEntry(
                    name,
                    normalized,
                    info.MuscleGroups ?? Array.Empty<string>(),
                    ManualExerciseDataSource.Primary,
                    info.ImagePath ?? string.Empty));
            }

            return list;
        }

        private IReadOnlyList<ExerciseIndexEntry> BuildSecondaryIndex()
        {
            var list = new List<ExerciseIndexEntry>();
            var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var info in _secondaryDatabase.GetAllExercises())
            {
                var name = !string.IsNullOrWhiteSpace(info.ExerciseName)
                    ? info.ExerciseName
                    : !string.IsNullOrWhiteSpace(info.Name) ? info.Name : string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var normalized = Normalize(name);
                if (string.IsNullOrEmpty(normalized) || !dedup.Add(normalized))
                {
                    continue;
                }

                list.Add(new ExerciseIndexEntry(
                    name,
                    normalized,
                    info.MuscleGroups ?? Array.Empty<string>(),
                    ManualExerciseDataSource.Secondary,
                    info.ImagePath ?? string.Empty));
            }

            return list;
        }

        private static bool IsMatch(ExerciseIndexEntry entry, string rawQuery, string normalizedQuery)
        {
            if (entry.NormalizedName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (entry.OriginalName.Contains(rawQuery, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return entry.MuscleGroups.Any(group =>
                Normalize(group).Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
        }

        private ExerciseGalleryItem? CreateGalleryItem(ExerciseIndexEntry entry)
        {
            try
            {
                var metadata = _searchService.FindExerciseWithImage(entry.OriginalName);

                var resolvedIdBase = Normalize(metadata?.Name ?? entry.OriginalName);
                if (string.IsNullOrEmpty(resolvedIdBase))
                {
                    resolvedIdBase = entry.NormalizedName;
                }

                var resolvedId = string.IsNullOrEmpty(resolvedIdBase)
                    ? $"{entry.Source}:{Guid.NewGuid():N}"
                    : $"{entry.Source}:{resolvedIdBase}";

                if (metadata == null)
                {
                    return new ExerciseGalleryItem(
                        resolvedId,
                        entry.OriginalName,
                        null,
                        entry.MuscleGroups,
                        entry.ImagePath ?? string.Empty,
                        Array.Empty<string>(),
                        entry.Source == ManualExerciseDataSource.Primary ? "BD Principal" : "BD Secundaria");
                }

                var groups = metadata.MuscleGroups?.Length > 0
                    ? metadata.MuscleGroups
                    : entry.MuscleGroups;

                if (groups == null || groups.Count == 0)
                {
                    groups = entry.MuscleGroups;
                }

                var imagePath = !string.IsNullOrWhiteSpace(metadata.ImagePath)
                    ? metadata.ImagePath
                    : entry.ImagePath ?? string.Empty;

                var keywords = metadata.Keywords ?? Array.Empty<string>();
                var sourceLabel = !string.IsNullOrWhiteSpace(metadata.Source)
                    ? metadata.Source
                    : entry.Source == ManualExerciseDataSource.Primary ? "BD Principal" : "BD Secundaria";

                return new ExerciseGalleryItem(
                    resolvedId,
                    metadata.Name ?? entry.OriginalName,
                    metadata.EnglishName,
                    groups,
                    imagePath,
                    keywords,
                    sourceLabel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error resolving metadata: {ex.Message}");
                return null;
            }
        }

        private static Image? ResolveImage(ExerciseWithImage metadata)
        {
            try
            {
                if (metadata.ImageData != null && metadata.ImageData.Length > 0)
                {
                    using var memory = new MemoryStream(metadata.ImageData);
                    using var original = Image.FromStream(memory);
                    return new Bitmap(original);
                }

                if (!string.IsNullOrWhiteSpace(metadata.ImagePath) && File.Exists(metadata.ImagePath))
                {
                    using var stream = new FileStream(metadata.ImagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var original = Image.FromStream(stream);
                    return new Bitmap(original);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error resolving image: {ex.Message}");
            }

            return null;
        }

        private static Image? LoadImageFromPath(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var original = Image.FromStream(stream);
                return new Bitmap(original);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error loading image from path: {ex.Message}");
                return null;
            }
        }

        private void StoreInCache(string key, Bitmap bitmap)
        {
            lock (_cacheLock)
            {
                if (_thumbnailCache.TryGetValue(key, out var existing))
                {
                    _cacheOrder.Remove(key);
                    existing.Dispose();
                }

                _thumbnailCache[key] = bitmap;
                _cacheOrder.AddFirst(key);

                while (_cacheOrder.Count > _cacheCapacity)
                {
                    var last = _cacheOrder.Last?.Value;
                    if (last == null)
                    {
                        break;
                    }

                    _cacheOrder.RemoveLast();

                    if (_thumbnailCache.Remove(last, out var toDispose))
                    {
                        toDispose.Dispose();
                    }
                }
            }
        }

        private static Bitmap ResizeImage(Image original, Size targetSize)
        {
            var ratio = Math.Min(
                (double)targetSize.Width / original.Width,
                (double)targetSize.Height / original.Height);

            var width = Math.Max(1, (int)Math.Round(original.Width * ratio));
            var height = Math.Max(1, (int)Math.Round(original.Height * ratio));

            var bitmap = new Bitmap(width, height);

            using var graphics = Graphics.FromImage(bitmap);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(original, new Rectangle(0, 0, width, height));

            return bitmap;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var lower = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(lower.Length);

            foreach (var character in lower)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            var stripped = builder.ToString();
            stripped = NonAlphaNumericRegex.Replace(stripped, " ");
            stripped = MultipleSpacesRegex.Replace(stripped, " ").Trim();

            return stripped;
        }

        private IEnumerable<ExerciseIndexEntry> EnumerateIndex(ManualExerciseDataSource source)
        {
            return source switch
            {
                ManualExerciseDataSource.Primary => _primaryIndex.Value,
                ManualExerciseDataSource.Secondary => _secondaryIndex.Value,
                ManualExerciseDataSource.Combined => _primaryIndex.Value.Concat(_secondaryIndex.Value),
                _ => _primaryIndex.Value
            };
        }

        private static void ExecuteOnStaThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                action();
                return;
            }

            ExceptionDispatchInfo? captured = null;
            using var done = new ManualResetEventSlim();

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ExceptionDispatchInfo.Capture(ex);
                }
                finally
                {
                    done.Set();
                }
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            done.Wait();

            captured?.Throw();
        }

        private sealed class ExerciseIndexEntry
        {
            public ExerciseIndexEntry(
                string originalName,
                string normalizedName,
                IReadOnlyList<string> muscleGroups,
                ManualExerciseDataSource source,
                string? imagePath)
            {
                OriginalName = originalName;
                NormalizedName = normalizedName;
                MuscleGroups = muscleGroups ?? Array.Empty<string>();
                Source = source;
                ImagePath = imagePath;
            }

            public string OriginalName { get; }

            public string NormalizedName { get; }

            public IReadOnlyList<string> MuscleGroups { get; }

            public ManualExerciseDataSource Source { get; }

            public string? ImagePath { get; }
        }
    }
}
