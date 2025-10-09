using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Management;
using GymRoutineGenerator.Infrastructure.Images;
using GymRoutineGenerator.Core.Enums;
using System.Drawing;
using System.Drawing.Imaging;

namespace GymRoutineGenerator.Tests;

[TestClass]
public class ImageDisplayDiagnosticTests
{
    private ServiceProvider _serviceProvider;
    private GymRoutineContext _context;
    private IExerciseManagementService _managementService;
    private IImageService _imageService;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IExerciseManagementService, ExerciseManagementService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<GymRoutineContext>();
        _managementService = _serviceProvider.GetRequiredService<IExerciseManagementService>();
        _imageService = _serviceProvider.GetRequiredService<IImageService>();

        SeedTestData();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    private void SeedTestData()
    {
        var muscleGroup = new Data.Entities.MuscleGroup
        {
            Id = 1,
            Name = "Chest",
            SpanishName = "Pecho",
            Description = "Chest muscles"
        };
        _context.MuscleGroups.Add(muscleGroup);

        var equipmentType = new Data.Entities.EquipmentType
        {
            Id = 1,
            Name = "Bodyweight",
            SpanishName = "Peso corporal",
            Description = "No equipment needed"
        };
        _context.EquipmentTypes.Add(equipmentType);

        var exercise = new Exercise
        {
            Id = 1,
            Name = "Push-up",
            SpanishName = "Flexiones",
            Description = "Basic push-up exercise for testing image display",
            Instructions = "Lower your body until chest nearly touches floor, then push back up",
            PrimaryMuscleGroupId = 1,
            EquipmentTypeId = 1,
            DifficultyLevel = DifficultyLevel.Beginner,
            ExerciseType = ExerciseType.Strength,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Exercises.Add(exercise);

        _context.SaveChanges();
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_NoImagesScenario()
    {
        // Test: Exercise with no images (simulates the issue)
        var exerciseId = 1;
        var exercise = await _managementService.GetExerciseByIdAsync(exerciseId);

        Console.WriteLine("=== DIAGNOSTIC: Exercise with no images ===");
        Console.WriteLine($"Exercise ID: {exercise.Id}");
        Console.WriteLine($"Spanish Name: {exercise.SpanishName}");
        Console.WriteLine($"Name: {exercise.Name}");

        // Check what the management service returns for images
        var images = await _managementService.GetExerciseImagesAsync(exerciseId);
        Console.WriteLine($"Images from management service: {images.Count}");

        // Check what happens with the image selection logic from the UI
        var preferred = images
            .OrderBy(i => i.ImagePosition == "demonstration" ? 0 : 1)
            .ThenBy(i => i.IsPrimary ? 0 : 1)
            .FirstOrDefault();

        Console.WriteLine($"Preferred image: {(preferred != null ? "Found" : "Not found")}");

        if (preferred == null)
        {
            Console.WriteLine("ISSUE IDENTIFIED: No images available for exercise");
            Console.WriteLine("Expected behavior: Should show placeholder or allow user to add image");
        }

        // Test image service fallback behavior
        var imageServicePath = _imageService.GetImagePath(exercise.SpanishName, "demonstration");
        var placeholderPath = _imageService.GetPlaceholderImagePath();
        var isPlaceholder = imageServicePath.Equals(placeholderPath);

        Console.WriteLine($"Image service path: {imageServicePath}");
        Console.WriteLine($"Is placeholder: {isPlaceholder}");
        Console.WriteLine($"File exists: {File.Exists(imageServicePath)}");

        Assert.IsTrue(isPlaceholder, "Should return placeholder path when no image exists");
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_WithDatabaseImage()
    {
        // Test: Exercise with image stored in database
        var exerciseId = 1;
        var exercise = await _managementService.GetExerciseByIdAsync(exerciseId);

        Console.WriteLine("=== DIAGNOSTIC: Exercise with database image ===");

        // Add a test image to the database
        var testImageBytes = CreateTestImageBytes("Database Test Image");
        var imageUpload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "flexiones_test.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Test image stored in database"
        };

        var addResult = await _managementService.AddExerciseImageAsync(exerciseId, imageUpload);
        Console.WriteLine($"Image addition result: Success={addResult.Success}, Message={addResult.Message}");

        // Now test the retrieval logic that the UI uses
        var images = await _managementService.GetExerciseImagesAsync(exerciseId);
        Console.WriteLine($"Images found: {images.Count}");

        foreach (var img in images)
        {
            Console.WriteLine($"Image ID: {img.Id}");
            Console.WriteLine($"Position: {img.ImagePosition}");
            Console.WriteLine($"IsPrimary: {img.IsPrimary}");
            Console.WriteLine($"ImageData size: {img.ImageData?.Length ?? 0} bytes");
            Console.WriteLine($"ImagePath: {img.ImagePath ?? "null"}");
            Console.WriteLine($"File exists at path: {(!string.IsNullOrEmpty(img.ImagePath) && File.Exists(img.ImagePath))}");
            Console.WriteLine("---");
        }

        // Test the UI's image selection logic
        var preferred = images
            .OrderBy(i => i.ImagePosition == "demonstration" ? 0 : 1)
            .ThenBy(i => i.IsPrimary ? 0 : 1)
            .FirstOrDefault();

        Assert.IsNotNull(preferred, "Should find preferred image");
        Console.WriteLine($"Preferred image selected: ID={preferred.Id}, Position={preferred.ImagePosition}");

        // Test loading image from bytes (what the UI should do)
        if (preferred.ImageData != null && preferred.ImageData.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(preferred.ImageData);
                using var image = Image.FromStream(ms);
                Console.WriteLine($"SUCCESS: Image loaded from database bytes - Size: {image.Width}x{image.Height}");

                // This is what the UI should do to display the image
                using var bitmap = new Bitmap(image);
                Console.WriteLine("SUCCESS: Bitmap created successfully for display");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading image from bytes: {ex.Message}");
                Assert.Fail($"Failed to load image from database bytes: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("ISSUE: Image has no binary data in database");
        }
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_WithFileSystemImage()
    {
        // Test: Exercise with image stored only as file path
        var exerciseId = 1;
        var exercise = await _managementService.GetExerciseByIdAsync(exerciseId);

        Console.WriteLine("=== DIAGNOSTIC: Exercise with file system image ===");

        // Create a temporary image file
        var tempImagePath = Path.Combine(Path.GetTempPath(), "test_flexiones_demo.png");
        var testImageBytes = CreateTestImageBytes("File System Test");
        await File.WriteAllBytesAsync(tempImagePath, testImageBytes);

        try
        {
            // Manually add image record with only file path (simulating older data)
            var exerciseImage = new ExerciseImage
            {
                ExerciseId = exerciseId,
                ImagePath = tempImagePath,
                ImageData = null, // No binary data - only file path
                ImagePosition = "demonstration",
                IsPrimary = true,
                Description = "Test image with file path only"
            };

            _context.ExerciseImages.Add(exerciseImage);
            await _context.SaveChangesAsync();

            // Test retrieval
            var images = await _managementService.GetExerciseImagesAsync(exerciseId);
            Console.WriteLine($"Images found: {images.Count}");

            var fileImage = images.FirstOrDefault();
            Assert.IsNotNull(fileImage, "Should find file-based image");

            Console.WriteLine($"Image path: {fileImage.ImagePath}");
            Console.WriteLine($"File exists: {File.Exists(fileImage.ImagePath)}");
            Console.WriteLine($"Has binary data: {fileImage.ImageData != null && fileImage.ImageData.Length > 0}");

            // Test the UI's fallback logic for file-based images
            if (fileImage.ImageData == null || fileImage.ImageData.Length == 0)
            {
                if (!string.IsNullOrEmpty(fileImage.ImagePath) && File.Exists(fileImage.ImagePath))
                {
                    try
                    {
                        // This is the fallback logic the UI should use
                        using var fs = new FileStream(fileImage.ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var image = Image.FromStream(fs);
                        Console.WriteLine($"SUCCESS: Image loaded from file path - Size: {image.Width}x{image.Height}");

                        using var bitmap = new Bitmap(image);
                        Console.WriteLine("SUCCESS: Bitmap created from file for display");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR loading image from file: {ex.Message}");
                        Assert.Fail($"Failed to load image from file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("ISSUE: Image file path is invalid or file doesn't exist");
                }
            }
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempImagePath))
                File.Delete(tempImagePath);
        }
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_ServiceResolutionProblem()
    {
        // Test: Simulating the service resolution issue from the UI
        Console.WriteLine("=== DIAGNOSTIC: Service Resolution Issue ===");

        try
        {
            // Test service resolution similar to how the UI does it
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GymRoutineContext>();
            var mgmt = scope.ServiceProvider.GetRequiredService<IExerciseManagementService>();

            Console.WriteLine("SUCCESS: Services resolved correctly");

            // Test finding exercise by name (like the UI does)
            var exerciseName = "Flexiones";
            var exercise = await db.Exercises.AsNoTracking()
                .FirstOrDefaultAsync(e => e.SpanishName.ToLower() == exerciseName.ToLower() ||
                                         e.Name.ToLower() == exerciseName.ToLower());

            if (exercise != null)
            {
                Console.WriteLine($"SUCCESS: Found exercise by name - ID: {exercise.Id}, Name: {exercise.SpanishName}");

                // Test getting images for this exercise
                var images = await mgmt.GetExerciseImagesAsync(exercise.Id);
                Console.WriteLine($"Images retrieved: {images.Count}");
            }
            else
            {
                Console.WriteLine($"ISSUE: Could not find exercise with name '{exerciseName}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in service resolution: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Assert.Fail($"Service resolution failed: {ex.Message}");
        }
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_MemoryStreamProblem()
    {
        // Test: Specific issue with MemoryStream and Image.FromStream
        Console.WriteLine("=== DIAGNOSTIC: MemoryStream handling ===");

        var testImageBytes = CreateTestImageBytes("Memory Stream Test");

        // Test different ways of loading image from bytes
        Console.WriteLine("Testing direct MemoryStream approach (what UI uses):");
        try
        {
            using var ms = new MemoryStream(testImageBytes);
            using var image = Image.FromStream(ms);
            using var bitmap = new Bitmap(image);
            Console.WriteLine($"SUCCESS: Direct approach - Size: {image.Width}x{image.Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR with direct approach: {ex.Message}");
        }

        Console.WriteLine("\nTesting copy approach (safer for UI):");
        try
        {
            using var ms = new MemoryStream(testImageBytes);
            using var tempImage = Image.FromStream(ms);
            using var bitmap = new Bitmap(tempImage); // This creates a copy
            Console.WriteLine($"SUCCESS: Copy approach - Size: {bitmap.Width}x{bitmap.Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR with copy approach: {ex.Message}");
        }

        Console.WriteLine("\nTesting temp file approach (most reliable):");
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, testImageBytes);
            using var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var image = Image.FromStream(fs);
            using var bitmap = new Bitmap(image);
            Console.WriteLine($"SUCCESS: Temp file approach - Size: {image.Width}x{image.Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR with temp file approach: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void DiagnoseImageDisplayIssue_AppServicesPattern()
    {
        // Test: Simulating the AppServices pattern used in the UI
        Console.WriteLine("=== DIAGNOSTIC: AppServices Pattern ===");

        // This simulates what the UI might be doing wrong
        try
        {
            // The UI uses AppServices.Get<T>() which might not be set up correctly
            // Let's test our service provider setup
            var context = _serviceProvider.GetRequiredService<GymRoutineContext>();
            var management = _serviceProvider.GetRequiredService<IExerciseManagementService>();

            Console.WriteLine("SUCCESS: Services available through provider");

            // Test multiple scope creation (what happens when UI creates multiple instances)
            using var scope1 = _serviceProvider.CreateScope();
            using var scope2 = _serviceProvider.CreateScope();

            var ctx1 = scope1.ServiceProvider.GetRequiredService<GymRoutineContext>();
            var ctx2 = scope2.ServiceProvider.GetRequiredService<GymRoutineContext>();

            Console.WriteLine($"Scope 1 context: {ctx1.GetHashCode()}");
            Console.WriteLine($"Scope 2 context: {ctx2.GetHashCode()}");
            Console.WriteLine("SUCCESS: Multiple scopes work correctly");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR with AppServices pattern: {ex.Message}");
            Assert.Fail($"AppServices pattern failed: {ex.Message}");
        }
    }

    [TestMethod]
    public async Task DiagnoseImageDisplayIssue_FullWorkflow()
    {
        // Test: Complete workflow from adding image to displaying it
        Console.WriteLine("=== DIAGNOSTIC: Full Image Workflow ===");

        var exerciseId = 1;
        var exerciseName = "Flexiones";

        // Step 1: Add image via management service
        Console.WriteLine("Step 1: Adding image to database...");
        var testImageBytes = CreateTestImageBytes("Full Workflow Test");
        var imageUpload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "workflow_test.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Full workflow test image"
        };

        var addResult = await _managementService.AddExerciseImageAsync(exerciseId, imageUpload);
        Console.WriteLine($"Add result: Success={addResult.Success}");
        if (!addResult.Success)
        {
            Console.WriteLine($"Add errors: {string.Join(", ", addResult.Errors)}");
            Assert.Fail("Failed to add image");
        }

        // Step 2: Retrieve images (like UI does)
        Console.WriteLine("\nStep 2: Retrieving images...");
        var images = await _managementService.GetExerciseImagesAsync(exerciseId);
        Console.WriteLine($"Images found: {images.Count}");

        // Step 3: Apply UI selection logic
        Console.WriteLine("\nStep 3: Applying UI selection logic...");
        var preferred = images
            .OrderBy(i => i.ImagePosition == "demonstration" ? 0 : 1)
            .ThenBy(i => i.IsPrimary ? 0 : 1)
            .FirstOrDefault();

        Assert.IsNotNull(preferred, "Should have preferred image");
        Console.WriteLine($"Preferred image: Position={preferred.ImagePosition}, Primary={preferred.IsPrimary}");

        // Step 4: Load image for display (simulate UI)
        Console.WriteLine("\nStep 4: Loading image for display...");
        try
        {
            if (preferred.ImageData != null && preferred.ImageData.Length > 0)
            {
                using var ms = new MemoryStream(preferred.ImageData);
                using var tempImage = Image.FromStream(ms);

                // Create a copy for the PictureBox (simulating UI)
                var displayBitmap = new Bitmap(tempImage);

                Console.WriteLine($"SUCCESS: Image ready for display - {displayBitmap.Width}x{displayBitmap.Height}");
                Console.WriteLine("Image can be assigned to PictureBox.Image");

                displayBitmap.Dispose();
            }
            else if (!string.IsNullOrEmpty(preferred.ImagePath) && File.Exists(preferred.ImagePath))
            {
                using var fs = new FileStream(preferred.ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var tempImage = Image.FromStream(fs);
                var displayBitmap = new Bitmap(tempImage);

                Console.WriteLine($"SUCCESS: Image loaded from file - {displayBitmap.Width}x{displayBitmap.Height}");

                displayBitmap.Dispose();
            }
            else
            {
                Console.WriteLine("ISSUE: No valid image data or file path");
                Assert.Fail("No valid image data");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR displaying image: {ex.Message}");
            Assert.Fail($"Failed to display image: {ex.Message}");
        }

        Console.WriteLine("\nWorkflow completed successfully!");
    }

    private byte[] CreateTestImageBytes(string text)
    {
        using var bitmap = new Bitmap(300, 200);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.Clear(Color.LightBlue);
        using var pen = new Pen(Color.DarkBlue, 2);
        graphics.DrawRectangle(pen, 5, 5, 290, 190);

        using var font = new Font("Arial", 14, FontStyle.Bold);
        using var brush = new SolidBrush(Color.DarkBlue);

        var rect = new RectangleF(10, 10, 280, 180);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(text, font, brush, rect, format);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}