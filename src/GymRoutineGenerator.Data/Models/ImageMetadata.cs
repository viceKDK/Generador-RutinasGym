using System.Text.Json.Serialization;

namespace GymRoutineGenerator.Data.Models;

/// <summary>
/// Metadata stored as JSON in the database for each exercise image
/// </summary>
public class ImageMetadata
{
    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;

    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    [JsonPropertyName("uploadedAt")]
    public DateTime UploadedAt { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("storageMethod")]
    public string StorageMethod { get; set; } = "database_only";

    [JsonPropertyName("validationStatus")]
    public string ValidationStatus { get; set; } = "pending";

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Create metadata from image bytes and upload information
    /// </summary>
    public static ImageMetadata FromImageData(byte[] imageData, string fileName, string contentType, string position, bool isPrimary, string description)
    {
        var metadata = new ImageMetadata
        {
            OriginalFileName = fileName,
            ContentType = contentType,
            FileSize = imageData.Length,
            UploadedAt = DateTime.UtcNow,
            Position = position,
            IsPrimary = isPrimary,
            Description = description,
            StorageMethod = "database_only",
            ValidationStatus = "pending"
        };

        // Extract image dimensions
        try
        {
            using var ms = new MemoryStream(imageData);
            using var image = System.Drawing.Image.FromStream(ms);
            metadata.Width = image.Width;
            metadata.Height = image.Height;
            metadata.ValidationStatus = "valid";
        }
        catch
        {
            metadata.ValidationStatus = "invalid_format";
            metadata.Width = 0;
            metadata.Height = 0;
        }

        return metadata;
    }

    /// <summary>
    /// Get a display-friendly summary of the image
    /// </summary>
    public string GetDisplaySummary()
    {
        if (ValidationStatus != "valid")
            return $"❌ {OriginalFileName} (Invalid)";

        return $"✅ {OriginalFileName} - {Width}x{Height} ({FileSize:N0} bytes)";
    }
}