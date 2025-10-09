using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using GymRoutineGenerator.Infrastructure.Images;
using GymRoutineGenerator.Data.Management;
using GymRoutineGenerator.Data.Context;

namespace GymRoutineGenerator.UI.Controls;

public sealed partial class ImageDropPreview : UserControl
{
    public static readonly DependencyProperty ExerciseNameProperty =
        DependencyProperty.Register(
            nameof(ExerciseName),
            typeof(string),
            typeof(ImageDropPreview),
            new PropertyMetadata(string.Empty, OnExerciseNameChanged));

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(
            nameof(Position),
            typeof(string),
            typeof(ImageDropPreview),
            new PropertyMetadata("demonstration", OnPositionChanged));

    private readonly IImageService _imageService;
    private readonly IExerciseManagementService? _managementService;
    private readonly GymRoutineContext? _db;

    public string ExerciseName
    {
        get => (string)GetValue(ExerciseNameProperty);
        set => SetValue(ExerciseNameProperty, value);
    }
    public static readonly DependencyProperty ExerciseIdProperty =
        DependencyProperty.Register(
            nameof(ExerciseId),
            typeof(int?),
            typeof(ImageDropPreview),
            new PropertyMetadata(null));

    public int? ExerciseId
    {
        get => (int?)GetValue(ExerciseIdProperty);
        set => SetValue(ExerciseIdProperty, value);
    }

    public string Position
    {
        get => (string)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public event EventHandler<string>? ImageSaved; // args: new image path

    public ImageDropPreview()
    {
        this.InitializeComponent();
        _imageService = (IImageService?)App.ServiceProvider.GetService(typeof(IImageService))
                        ?? new GymRoutineGenerator.Infrastructure.Images.ImageService();
        _managementService = (IExerciseManagementService?)App.ServiceProvider.GetService(typeof(IExerciseManagementService));
        _db = (GymRoutineContext?)App.ServiceProvider.GetService(typeof(GymRoutineContext));
        _ = LoadCurrentAsync();
    }

    private static void OnExerciseNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageDropPreview ctrl)
        {
            _ = ctrl.LoadCurrentAsync();
        }
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageDropPreview ctrl)
        {
            _ = ctrl.LoadCurrentAsync();
        }
    }

    private async Task LoadCurrentAsync()
    {
        try
        {
            await _imageService.EnsureImageDirectoriesExistAsync();

            var name = ExerciseName?.Trim();
            string path = string.IsNullOrWhiteSpace(name)
                ? _imageService.GetPlaceholderImagePath()
                : _imageService.GetImagePath(name!, Position);

            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var bmp = new BitmapImage(new Uri(path));
                    PreviewImage.Source = bmp;
                }
                catch
                {
                    PreviewImage.Source = null;
                }
            });
        }
        catch
        {
            // ignore
        }
    }

    private void DropZone_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        Overlay.Visibility = Visibility.Visible;
    }

    private void DropZone_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        Overlay.Visibility = Visibility.Collapsed;
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        var dv = e.DataView;
        if (dv.Contains(StandardDataFormats.StorageItems) || dv.Contains(StandardDataFormats.Bitmap))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
        e.Handled = true;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var file = items.OfType<StorageFile>().FirstOrDefault();
                if (file is null) return;

                var ext = file.FileType.ToLowerInvariant();
                if (ext is not (".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".jfif"))
                {
                    return;
                }

                using var readStream = await file.OpenStreamForReadAsync();
                using var mem = new System.IO.MemoryStream();
                await readStream.CopyToAsync(mem);
                var bytes = mem.ToArray();

                var targetExerciseId = ExerciseId ?? await ResolveExerciseIdByNameAsync();
                if (targetExerciseId.HasValue && _managementService is not null)
                {
                    var existing = await _managementService.GetExerciseImagesAsync(targetExerciseId.Value);
                    var current = existing.FirstOrDefault(i => string.Equals(i.ImagePosition, Position, StringComparison.OrdinalIgnoreCase));
                    var upload = new ExerciseImageUpload
                    {
                        ImageData = bytes,
                        FileName = file.Name,
                        ContentType = GetContentTypeFromExtension(ext),
                        Position = string.IsNullOrWhiteSpace(Position) ? "demonstration" : Position,
                        IsPrimary = Position == "default",
                        Description = "Imagen agregada por arrastrar y soltar"
                    };
                    ExerciseManagementResult result;
                    if (current != null)
                    {
                        result = await _managementService.UpdateExerciseImageAsync(current.Id, upload);
                    }
                    else
                    {
                        result = await _managementService.AddExerciseImageAsync(targetExerciseId.Value, upload);
                    }
                    if (!result.Success)
                    {
                        // fallback to filesystem-only save
                        await SaveToFilesystemFallbackAsync(bytes, file.Name);
                    }
                }
                else
                {
                    await SaveToFilesystemFallbackAsync(bytes, file.Name);
                }

                await LoadCurrentAsync();
            }
            else if (e.DataView.Contains(StandardDataFormats.Bitmap))
            {
                var bmpRef = await e.DataView.GetBitmapAsync();
                using var ras = await bmpRef.OpenReadAsync();
                using var mem = new System.IO.MemoryStream();
                await ras.AsStream().CopyToAsync(mem);
                var bytes = mem.ToArray();
                var targetExerciseId2 = ExerciseId ?? await ResolveExerciseIdByNameAsync();
                if (targetExerciseId2.HasValue && _managementService is not null)
                {
                    var existing2 = await _managementService.GetExerciseImagesAsync(targetExerciseId2.Value);
                    var current2 = existing2.FirstOrDefault(i => string.Equals(i.ImagePosition, Position, StringComparison.OrdinalIgnoreCase));
                    var upload = new ExerciseImageUpload
                    {
                        ImageData = bytes,
                        FileName = "image.jpg",
                        ContentType = "image/jpeg",
                        Position = string.IsNullOrWhiteSpace(Position) ? "demonstration" : Position,
                        IsPrimary = Position == "default",
                        Description = "Imagen agregada por arrastrar y soltar (bitmap)"
                    };
                    ExerciseManagementResult result;
                    if (current2 != null)
                    {
                        result = await _managementService.UpdateExerciseImageAsync(current2.Id, upload);
                    }
                    else
                    {
                        result = await _managementService.AddExerciseImageAsync(targetExerciseId2.Value, upload);
                    }
                    if (!result.Success)
                    {
                        await SaveToFilesystemFallbackAsync(bytes, "image.jpg");
                    }
                }
                else
                {
                    await SaveToFilesystemFallbackAsync(bytes, "image.jpg");
                }
                await LoadCurrentAsync();
            }
        }
        catch
        {
            // Swallow errors to avoid UI crashes
        }
        finally
        {
            Overlay.Visibility = Visibility.Collapsed;
        }
    }

    private async Task SaveToFilesystemFallbackAsync(byte[] data, string fileName)
    {
        var name = string.IsNullOrWhiteSpace(ExerciseName)
            ? System.IO.Path.GetFileNameWithoutExtension(fileName)
            : ExerciseName;
        var ext = System.IO.Path.GetExtension(fileName).Trim('.');
        if (string.IsNullOrWhiteSpace(ext)) ext = "jpg";
        var savedPath = await _imageService.SaveImageFromBytesAsync(data, name, Position, ext);
        ImageSaved?.Invoke(this, savedPath);
    }

    private static string GetContentTypeFromExtension(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".jfif" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }

    private async Task<int?> ResolveExerciseIdByNameAsync()
    {
        try
        {
            if (_db is null) return null;
            var name = ExerciseName?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;
            // Try SpanishName first, then Name
            var match = await _db.Exercises
                .Where(e => e.SpanishName.ToLower() == name.ToLower() || e.Name.ToLower() == name.ToLower())
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
            return match == 0 ? null : match;
        }
        catch { return null; }
    }
}




