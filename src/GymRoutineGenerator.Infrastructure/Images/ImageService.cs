using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace GymRoutineGenerator.Infrastructure.Images;

public class ImageService : IImageService
{
    private readonly string _baseImageDirectory;
    private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
    private readonly string[] _imagePositions = { "start", "mid", "end", "demonstration", "default" };

    public ImageService(string? baseImageDirectory = null)
    {
        _baseImageDirectory = baseImageDirectory ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "GymRoutineGenerator", "Images");
    }

    public async Task EnsureImageDirectoriesExistAsync()
    {
        // Create main images directory
        Directory.CreateDirectory(_baseImageDirectory);

        // Create subdirectories for organization
        Directory.CreateDirectory(Path.Combine(_baseImageDirectory, "exercises"));
        Directory.CreateDirectory(Path.Combine(_baseImageDirectory, "placeholders"));
        Directory.CreateDirectory(Path.Combine(_baseImageDirectory, "temp"));

        // Create placeholder image if it doesn't exist
        await CreatePlaceholderImageIfNeededAsync();
    }

    public async Task<string> SaveImageAsync(string imagePath, string exerciseName, string position = "default")
    {
        if (!await ValidateImageAsync(imagePath))
            throw new ArgumentException("Invalid image file");

        await EnsureImageDirectoriesExistAsync();

        var sanitizedName = SanitizeFileName(exerciseName);
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        var fileName = $"{sanitizedName}_{position}{extension}";
        var destinationPath = Path.Combine(_baseImageDirectory, "exercises", fileName);

        // Optimize and save image
        var optimizedBytes = await OptimizeImageAsync(imagePath);
        await File.WriteAllBytesAsync(destinationPath, optimizedBytes);

        return destinationPath;
    }

    public async Task<string> SaveImageFromBytesAsync(byte[] imageBytes, string exerciseName, string position = "default", string extension = "jpg")
    {
        await EnsureImageDirectoriesExistAsync();

        var sanitizedName = SanitizeFileName(exerciseName);
        var fileName = $"{sanitizedName}_{position}.{extension.TrimStart('.')}";
        var destinationPath = Path.Combine(_baseImageDirectory, "exercises", fileName);

        // Create temporary file for validation and optimization
        var tempPath = Path.Combine(_baseImageDirectory, "temp", Guid.NewGuid().ToString() + ".tmp");
        await File.WriteAllBytesAsync(tempPath, imageBytes);

        try
        {
            if (!await ValidateImageAsync(tempPath))
                throw new ArgumentException("Invalid image data");

            var optimizedBytes = await OptimizeImageAsync(tempPath);
            await File.WriteAllBytesAsync(destinationPath, optimizedBytes);

            return destinationPath;
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public async Task<byte[]> OptimizeImageAsync(string imagePath, int maxWidth = 800, int quality = 85)
    {
        // Simulate async operation
        await Task.Delay(1);

        using var originalImage = Image.FromFile(imagePath);

        // Calculate new dimensions maintaining aspect ratio
        var ratio = Math.Min((double)maxWidth / originalImage.Width, (double)maxWidth / originalImage.Height);
        var newWidth = (int)(originalImage.Width * ratio);
        var newHeight = (int)(originalImage.Height * ratio);

        // Create optimized image
        using var resizedImage = new Bitmap(newWidth, newHeight);
        using var graphics = Graphics.FromImage(resizedImage);

        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);

        // Save to memory stream with quality settings
        using var memoryStream = new MemoryStream();
        var codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

        resizedImage.Save(memoryStream, codec, encoderParams);
        return memoryStream.ToArray();
    }

    public async Task<bool> ValidateImageAsync(string imagePath)
    {
        try
        {
            // Simulate async operation
            await Task.Delay(1);

            if (!File.Exists(imagePath))
                return false;

            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            if (!_supportedExtensions.Contains(extension))
                return false;

            var fileInfo = new FileInfo(imagePath);
            if (fileInfo.Length > 10 * 1024 * 1024) // 10MB limit
                return false;

            // Try to load the image to validate format
            using var image = Image.FromFile(imagePath);

            // Check minimum dimensions
            if (image.Width < 100 || image.Height < 100)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetImagePath(string exerciseName, string position = "default")
    {
        var sanitizedName = SanitizeFileName(exerciseName);
        var searchPattern = $"{sanitizedName}_{position}.*";
        var exerciseDir = Path.Combine(_baseImageDirectory, "exercises");

        if (!Directory.Exists(exerciseDir))
            return GetPlaceholderImagePath();

        var matchingFiles = Directory.GetFiles(exerciseDir, searchPattern);
        return matchingFiles.FirstOrDefault() ?? GetPlaceholderImagePath();
    }

    public string GetPlaceholderImagePath()
    {
        return Path.Combine(_baseImageDirectory, "placeholders", "exercise_placeholder.png");
    }

    public Task<bool> ImageExistsAsync(string exerciseName, string position = "default")
    {
        var imagePath = GetImagePath(exerciseName, position);
        return Task.FromResult(!imagePath.Equals(GetPlaceholderImagePath()) && File.Exists(imagePath));
    }

    public Task<List<string>> GetExerciseImagesAsync(string exerciseName)
    {
        var sanitizedName = SanitizeFileName(exerciseName);
        var exerciseDir = Path.Combine(_baseImageDirectory, "exercises");
        var images = new List<string>();

        if (!Directory.Exists(exerciseDir))
            return Task.FromResult(images);

        foreach (var position in _imagePositions)
        {
            var imagePath = GetImagePath(exerciseName, position);
            if (!imagePath.Equals(GetPlaceholderImagePath()))
            {
                images.Add(imagePath);
            }
        }

        return Task.FromResult(images);
    }

    public Task<bool> DeleteImageAsync(string exerciseName, string position = "default")
    {
        var imagePath = GetImagePath(exerciseName, position);

        if (imagePath.Equals(GetPlaceholderImagePath()))
            return Task.FromResult(false);

        try
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                return Task.FromResult(true);
            }
        }
        catch
        {
            // Log error if needed
        }

        return Task.FromResult(false);
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Additional cleanup for Spanish characters and spaces
        sanitized = Regex.Replace(sanitized, @"[^\w\-_]", "_");
        sanitized = Regex.Replace(sanitized, @"_+", "_");
        sanitized = sanitized.Trim('_').ToLowerInvariant();

        return string.IsNullOrEmpty(sanitized) ? "exercise" : sanitized;
    }

    private Task CreatePlaceholderImageIfNeededAsync()
    {
        var placeholderPath = GetPlaceholderImagePath();

        if (File.Exists(placeholderPath))
            return Task.CompletedTask;

        try
        {
            // Create a simple placeholder image
            using var bitmap = new Bitmap(400, 300);
            using var graphics = Graphics.FromImage(bitmap);

            // Fill with light gray background
            graphics.Clear(Color.LightGray);

            // Draw border
            using var pen = new Pen(Color.DarkGray, 2);
            graphics.DrawRectangle(pen, 10, 10, bitmap.Width - 20, bitmap.Height - 20);

            // Draw text
            var text = "Imagen no\ndisponible";
            var font = new Font("Arial", 16, FontStyle.Bold);
            var brush = new SolidBrush(Color.DarkGray);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var rect = new RectangleF(0, 0, bitmap.Width, bitmap.Height);
            graphics.DrawString(text, font, brush, rect, format);

            // Save placeholder
            bitmap.Save(placeholderPath, ImageFormat.Png);
        }
        catch
        {
            // If we can't create placeholder, that's OK - we'll handle missing images gracefully
        }

        return Task.CompletedTask;
    }
}