using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GymRoutineGenerator.Domain.Models;
using System.Runtime.InteropServices;
using GymRoutineGenerator.Infrastructure;

namespace GymRoutineGenerator.Services
{
    /// <summary>
    /// Provides manual lookup helpers for the exercise gallery, wrapping the existing
    /// search service and exposing UI-friendly DTOs and utilities.
    /// </summary>
    public sealed class ManualExerciseLibraryService : IDisposable
    {
        private readonly ExerciseImageSearchService _searchService;
        private readonly SQLiteExerciseImageDatabase _primaryDatabase;
        private readonly Lazy<IReadOnlyList<ExerciseIndexEntry>> _primaryIndex;
        private readonly string? _docsExercisesPath;
        private readonly Lazy<Dictionary<string, string>> _docsImageLookup;

        private readonly object _cacheLock = new();
        private readonly Dictionary<string, Bitmap> _thumbnailCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> _cacheOrder = new();
        private readonly int _cacheCapacity;

        private static readonly Regex NonAlphaNumericRegex = new("[^a-z0-9\\s]", RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesRegex = new("\\s+", RegexOptions.Compiled);

        public ManualExerciseLibraryService(
            ExerciseImageSearchService? searchService = null,
            SQLiteExerciseImageDatabase? primaryDatabase = null,
            int thumbnailCacheCapacity = 100)
        {
            _searchService = searchService ?? new ExerciseImageSearchService();
            _primaryDatabase = primaryDatabase ?? new SQLiteExerciseImageDatabase();
            _cacheCapacity = Math.Max(10, thumbnailCacheCapacity);

            _primaryIndex = new Lazy<IReadOnlyList<ExerciseIndexEntry>>(BuildPrimaryIndex, LazyThreadSafetyMode.ExecutionAndPublication);
            _docsExercisesPath = FindDocsEjerciciosPath(AppDomain.CurrentDomain.BaseDirectory);
            _docsImageLookup = new Lazy<Dictionary<string, string>>(BuildDocsImageLookup, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Searches exercises by name, matching partial tokens regardless of casing or diacritics.
        /// </summary>
        public IReadOnlyList<ExerciseGalleryItem> Search(
            string query,
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

            foreach (var entry in _primaryIndex.Value)
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

            var docsPath = ResolveImageFromDocs(exerciseName);
            if (!string.IsNullOrWhiteSpace(docsPath))
            {
                return docsPath;
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

            var docsPath = ResolveImageFromDocs(item.Name, item.EnglishName, item.DisplayName);
            if (!string.IsNullOrWhiteSpace(docsPath))
            {
                return docsPath;
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
                var fallbackBitmap = TryLoadThumbnailFromShell(item.ImagePath, targetSize);
                if (fallbackBitmap != null)
                {
                    StoreInCache(cacheKey, fallbackBitmap);
                    return new Bitmap(fallbackBitmap);
                }
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
                var fallbackImage = ResolveImageFromDocs(entry.OriginalName);

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
                    var imagePath = entry.ImagePath;
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    imagePath = fallbackImage ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    return null;
                }

                return new ExerciseGalleryItem(
                    resolvedId,
                    entry.OriginalName,
                    null,
                    entry.MuscleGroups,
                        imagePath ?? string.Empty,
                        Array.Empty<string>(),
                        "BD Principal");
                }

                var groups = metadata.MuscleGroups?.Length > 0
                    ? metadata.MuscleGroups
                    : entry.MuscleGroups;

                if (groups == null || groups.Count == 0)
                {
                    groups = entry.MuscleGroups;
                }

                var resolvedImagePath = !string.IsNullOrWhiteSpace(metadata.ImagePath) && File.Exists(metadata.ImagePath)
                    ? metadata.ImagePath
                    : entry.ImagePath;

                if (string.IsNullOrWhiteSpace(resolvedImagePath) || !File.Exists(resolvedImagePath))
                {
                    resolvedImagePath = fallbackImage ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(resolvedImagePath) || !File.Exists(resolvedImagePath))
                {
                    return null;
                }

                var keywords = metadata.Keywords ?? Array.Empty<string>();
                var sourceLabel = "BD Principal";

                return new ExerciseGalleryItem(
                    resolvedId,
                    metadata.Name ?? entry.OriginalName,
                    metadata.EnglishName,
                    groups,
                    resolvedImagePath,
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
            using var normalized = NormalizeBackground(original);

            var ratio = Math.Min(
                (double)targetSize.Width / normalized.Width,
                (double)targetSize.Height / normalized.Height);

            var width = Math.Max(1, (int)Math.Round(normalized.Width * ratio));
            var height = Math.Max(1, (int)Math.Round(normalized.Height * ratio));

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(normalized, new Rectangle(0, 0, width, height));

            return bitmap;
        }

        private static Bitmap NormalizeBackground(Image source)
        {
            var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            if (bitmap.Width == 0 || bitmap.Height == 0)
            {
                return bitmap;
            }

            var key = bitmap.GetPixel(0, 0);
            if (key.A < 240)
            {
                return bitmap;
            }

            ApplyAlphaMask(bitmap, key, 24);
            return bitmap;
        }

        private static void ApplyAlphaMask(Bitmap bitmap, Color key, int tolerance)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);

            unsafe
            {
                for (int y = 0; y < data.Height; y++)
                {
                    var row = (int*)((byte*)data.Scan0 + y * data.Stride);
                    for (int x = 0; x < data.Width; x++)
                    {
                        var pixel = row[x];
                        var a = (pixel >> 24) & 0xFF;
                        var r = (pixel >> 16) & 0xFF;
                        var g = (pixel >> 8) & 0xFF;
                        var b = pixel & 0xFF;

                        if (a >= 200 &&
                            Math.Abs(r - key.R) <= tolerance &&
                            Math.Abs(g - key.G) <= tolerance &&
                            Math.Abs(b - key.B) <= tolerance)
                        {
                            row[x] = 0;
                        }
                    }
                }
            }

            bitmap.UnlockBits(data);
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

        private string FindDocsEjerciciosPath(string startPath)
        {
            try
            {
                var current = new DirectoryInfo(startPath);
                for (int i = 0; i < 10 && current != null; i++)
                {
                    var docsPath = Path.Combine(current.FullName, "docs", "ejercicios");
                    if (Directory.Exists(docsPath))
                    {
                        return docsPath;
                    }

                    current = current.Parent;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error buscando docs/ejercicios: {ex.Message}");
            }

            return string.Empty;
        }

        private Dictionary<string, string> BuildDocsImageLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(_docsExercisesPath) || !Directory.Exists(_docsExercisesPath))
            {
                return lookup;
            }

            try
            {
                foreach (var muscleDir in Directory.GetDirectories(_docsExercisesPath))
                {
                    foreach (var exerciseDir in Directory.GetDirectories(muscleDir))
                    {
                        var exerciseName = Path.GetFileName(exerciseDir);
                        var normalized = Normalize(exerciseName);
                        if (string.IsNullOrEmpty(normalized) || lookup.ContainsKey(normalized))
                        {
                            continue;
                        }

                        var imageFile = Directory.GetFiles(exerciseDir, "*.*")
                            .Where(IsSupportedImage)
                            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                            .FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(imageFile))
                        {
                            lookup[normalized] = Path.GetFullPath(imageFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Error construyendo indice de docs: {ex.Message}");
            }

            return lookup;
        }

        private string? ResolveImageFromDocs(params string?[] names)
        {
            if (_docsImageLookup == null)
            {
                return null;
            }

            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var normalized = Normalize(name);
                if (string.IsNullOrEmpty(normalized))
                {
                    continue;
                }

                try
                {
                    var lookup = _docsImageLookup.Value;
                    if (lookup.TryGetValue(normalized, out var path) && File.Exists(path))
                    {
                        return path;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ManualExerciseLibraryService] Error resolviendo imagen desde docs: {ex.Message}");
                }
            }

            return null;
        }

        private static bool IsSupportedImage(string filePath)
        {
            return filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                   filePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        }

        private static Bitmap? TryLoadThumbnailFromShell(string? path, Size targetSize)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                var guid = typeof(IShellItemImageFactory).GUID;
                SHCreateItemFromParsingName(path, IntPtr.Zero, ref guid, out var factory);
                try
                {
                    var size = new SIZE { cx = targetSize.Width, cy = targetSize.Height };
                    factory.GetImage(size, SIIGBF.SIIGBF_BIGGERSIZEOK | SIIGBF.SIIGBF_RESIZETOFIT | SIIGBF.SIIGBF_SCALEUP, out var hBitmap);
                    if (hBitmap != IntPtr.Zero)
                    {
                        try
                        {
                            using var bmp = Image.FromHbitmap(hBitmap);
                            return ResizeImage(bmp, targetSize);
                        }
                        finally
                        {
                            DeleteObject(hBitmap);
                        }
                    }
                }
                finally
                {
                    if (factory != null)
                    {
                        Marshal.ReleaseComObject(factory);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualExerciseLibraryService] Shell thumbnail fallback failed: {ex.Message}");
            }

            return null;
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

        [ComImport]
        [Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            void GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [Flags]
        private enum SIIGBF
        {
            SIIGBF_RESIZETOFIT = 0x00,
            SIIGBF_BIGGERSIZEOK = 0x01,
            SIIGBF_MEMORYONLY = 0x02,
            SIIGBF_ICONONLY = 0x04,
            SIIGBF_THUMBNAILONLY = 0x08,
            SIIGBF_INCACHEONLY = 0x10,
            SIIGBF_CROPTOBORDER = 0x20,
            SIIGBF_SCALEUP = 0x40
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(string pszPath, IntPtr pbc, ref Guid riid, out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

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
