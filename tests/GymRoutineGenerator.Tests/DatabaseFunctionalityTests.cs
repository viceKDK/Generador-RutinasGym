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
public class DatabaseFunctionalityTests
{
    private ServiceProvider _serviceProvider;
    private GymRoutineContext _context;
    private IExerciseManagementService _managementService;
    private IImageService _imageService;

    [TestInitialize]
    public void Setup()
    {
        // Configure services similar to the app
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Add in-memory database for testing
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add services
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
        // Add test muscle groups
        var muscleGroups = new[]
        {
            new Data.Entities.MuscleGroup { Id = 1, Name = "Chest", SpanishName = "Pecho", Description = "Chest muscles" },
            new Data.Entities.MuscleGroup { Id = 2, Name = "Back", SpanishName = "Espalda", Description = "Back muscles" },
            new Data.Entities.MuscleGroup { Id = 3, Name = "Legs", SpanishName = "Piernas", Description = "Leg muscles" }
        };
        _context.MuscleGroups.AddRange(muscleGroups);

        // Add test equipment types
        var equipmentTypes = new[]
        {
            new Data.Entities.EquipmentType { Id = 1, Name = "Bodyweight", SpanishName = "Peso corporal", Description = "No equipment needed" },
            new Data.Entities.EquipmentType { Id = 2, Name = "Dumbbells", SpanishName = "Mancuernas", Description = "Dumbbell exercises" },
            new Data.Entities.EquipmentType { Id = 3, Name = "Barbell", SpanishName = "Barra", Description = "Barbell exercises" }
        };
        _context.EquipmentTypes.AddRange(equipmentTypes);

        // Add test exercises
        var exercises = new[]
        {
            new Exercise
            {
                Id = 1,
                Name = "Push-up",
                SpanishName = "Flexiones",
                Description = "Basic push-up exercise",
                Instructions = "Lower your body until chest nearly touches floor, then push back up",
                PrimaryMuscleGroupId = 1,
                EquipmentTypeId = 1,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Exercise
            {
                Id = 2,
                Name = "Pull-up",
                SpanishName = "Dominadas",
                Description = "Pull-up exercise",
                Instructions = "Hang from bar and pull yourself up until chin is over bar",
                PrimaryMuscleGroupId = 2,
                EquipmentTypeId = 1,
                DifficultyLevel = DifficultyLevel.Intermediate,
                ExerciseType = ExerciseType.Strength,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _context.Exercises.AddRange(exercises);

        _context.SaveChanges();
    }

    [TestMethod]
    public async Task DatabaseConnection_ShouldWork()
    {
        // Arrange & Act
        var exerciseCount = await _context.Exercises.CountAsync();
        var muscleGroupCount = await _context.MuscleGroups.CountAsync();
        var equipmentCount = await _context.EquipmentTypes.CountAsync();

        // Assert
        Assert.IsTrue(exerciseCount > 0, "Should have exercises in database");
        Assert.IsTrue(muscleGroupCount > 0, "Should have muscle groups in database");
        Assert.IsTrue(equipmentCount > 0, "Should have equipment types in database");

        Console.WriteLine($"Database contains: {exerciseCount} exercises, {muscleGroupCount} muscle groups, {equipmentCount} equipment types");
    }

    [TestMethod]
    public async Task ExerciseManagementService_ShouldRetrieveExercises()
    {
        // Act
        var exercises = await _managementService.GetAllExercisesAsync();

        // Assert
        Assert.IsNotNull(exercises, "Exercises should not be null");
        Assert.IsTrue(exercises.Count > 0, "Should retrieve exercises from database");

        var firstExercise = exercises.First();
        Assert.IsNotNull(firstExercise.SpanishName, "Exercise should have Spanish name");
        Assert.IsNotNull(firstExercise.Description, "Exercise should have description");

        Console.WriteLine($"Retrieved {exercises.Count} exercises");
        foreach (var exercise in exercises)
        {
            Console.WriteLine($"- {exercise.SpanishName}: {exercise.Description}");
        }
    }

    [TestMethod]
    public async Task ExerciseManagementService_ShouldRetrieveExerciseById()
    {
        // Act
        var exercise = await _managementService.GetExerciseByIdAsync(1);

        // Assert
        Assert.IsNotNull(exercise, "Should retrieve exercise by ID");
        Assert.AreEqual("Flexiones", exercise.SpanishName);
        Assert.AreEqual("Basic push-up exercise", exercise.Description);
        Assert.IsNotNull(exercise.PrimaryMuscleGroup, "Should include primary muscle group");
        Assert.IsNotNull(exercise.EquipmentType, "Should include equipment type");

        Console.WriteLine($"Retrieved exercise: {exercise.SpanishName} - {exercise.Description}");
        Console.WriteLine($"Primary muscle: {exercise.PrimaryMuscleGroup.SpanishName}");
        Console.WriteLine($"Equipment: {exercise.EquipmentType.SpanishName}");
    }

    [TestMethod]
    public async Task ImageManagement_ShouldAddImageToDatabase()
    {
        // Arrange
        var testImageBytes = CreateTestImageBytes();
        var exerciseId = 1;

        var imageUpload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "test_flexiones.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Test image for flexiones"
        };

        // Act
        var result = await _managementService.AddExerciseImageAsync(exerciseId, imageUpload);

        // Assert
        Assert.IsTrue(result.Success, $"Image addition should succeed. Errors: {string.Join(", ", result.Errors)}");
        Assert.IsTrue(string.IsNullOrEmpty(result.Message) || result.Message.Contains("exitosamente"),
            $"Should have success message: {result.Message}");

        Console.WriteLine($"Image addition result: {result.Message}");
        if (result.Warnings.Any())
        {
            Console.WriteLine($"Warnings: {string.Join(", ", result.Warnings)}");
        }
    }

    [TestMethod]
    public async Task ImageManagement_ShouldRetrieveImagesFromDatabase()
    {
        // Arrange - First add an image
        var testImageBytes = CreateTestImageBytes();
        var exerciseId = 1;

        var imageUpload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "test_flexiones_demo.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Test demonstration image"
        };

        await _managementService.AddExerciseImageAsync(exerciseId, imageUpload);

        // Act
        var images = await _managementService.GetExerciseImagesAsync(exerciseId);

        // Assert
        Assert.IsNotNull(images, "Images collection should not be null");
        Assert.IsTrue(images.Count > 0, "Should have at least one image");

        var firstImage = images.First();
        Assert.IsNotNull(firstImage.ImageData, "Image should have binary data");
        Assert.IsTrue(firstImage.ImageData.Length > 0, "Image data should not be empty");
        Assert.AreEqual("demonstration", firstImage.ImagePosition, "Image position should match");
        Assert.IsTrue(firstImage.IsPrimary, "Image should be marked as primary");
        Assert.IsNotNull(firstImage.ImagePath, "Image should have a file path");

        Console.WriteLine($"Retrieved {images.Count} images for exercise {exerciseId}");
        foreach (var image in images)
        {
            Console.WriteLine($"- Position: {image.ImagePosition}, Primary: {image.IsPrimary}, " +
                            $"Data size: {image.ImageData?.Length ?? 0} bytes, Path: {image.ImagePath}");
        }
    }

    [TestMethod]
    public async Task ImageManagement_ShouldUpdateExistingImage()
    {
        // Arrange - Add initial image
        var initialImageBytes = CreateTestImageBytes();
        var exerciseId = 2;

        var initialUpload = new ExerciseImageUpload
        {
            ImageData = initialImageBytes,
            FileName = "initial_dominadas.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Initial test image"
        };

        var addResult = await _managementService.AddExerciseImageAsync(exerciseId, initialUpload);
        Assert.IsTrue(addResult.Success, "Initial image addition should succeed");

        var images = await _managementService.GetExerciseImagesAsync(exerciseId);
        var imageId = images.First().Id;

        // Act - Update the image
        var updatedImageBytes = CreateTestImageBytes(Color.Blue);
        var updateUpload = new ExerciseImageUpload
        {
            ImageData = updatedImageBytes,
            FileName = "updated_dominadas.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Updated test image"
        };

        var updateResult = await _managementService.UpdateExerciseImageAsync(imageId, updateUpload);

        // Assert
        Assert.IsTrue(updateResult.Success, $"Image update should succeed. Errors: {string.Join(", ", updateResult.Errors)}");

        // Verify the image was actually updated
        var updatedImages = await _managementService.GetExerciseImagesAsync(exerciseId);
        var updatedImage = updatedImages.First();
        Assert.AreEqual("Updated test image", updatedImage.Description, "Description should be updated");
        Assert.AreNotEqual(initialImageBytes.Length, updatedImage.ImageData.Length, "Image data should be different");

        Console.WriteLine($"Image update result: {updateResult.Message}");
        Console.WriteLine($"Updated image: {updatedImage.Description}, Data size: {updatedImage.ImageData?.Length ?? 0} bytes");
    }

    [TestMethod]
    public async Task ServiceResolution_ShouldWorkCorrectly()
    {
        // Act & Assert - Test service resolution similar to how the app does it
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<GymRoutineContext>();
        Assert.IsNotNull(dbContext, "Should resolve database context");

        var managementService = scope.ServiceProvider.GetRequiredService<IExerciseManagementService>();
        Assert.IsNotNull(managementService, "Should resolve exercise management service");

        var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
        Assert.IsNotNull(imageService, "Should resolve image service");

        // Test that services work together
        var exercises = await managementService.GetAllExercisesAsync();
        Assert.IsTrue(exercises.Count > 0, "Services should work together to retrieve data");

        Console.WriteLine("Service resolution test passed - all services resolved correctly");
    }

    [TestMethod]
    public async Task ImageService_ShouldValidateImages()
    {
        // Arrange
        var validImageBytes = CreateTestImageBytes();
        var invalidImageBytes = new byte[] { 0x01, 0x02, 0x03 }; // Invalid image data

        var tempValidFile = Path.GetTempFileName();
        var tempInvalidFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(tempValidFile + ".png", validImageBytes);
            await File.WriteAllBytesAsync(tempInvalidFile + ".txt", invalidImageBytes);

            // Act & Assert
            var validResult = await _imageService.ValidateImageAsync(tempValidFile + ".png");
            var invalidResult = await _imageService.ValidateImageAsync(tempInvalidFile + ".txt");

            Assert.IsTrue(validResult, "Valid image should pass validation");
            Assert.IsFalse(invalidResult, "Invalid image should fail validation");

            Console.WriteLine($"Image validation: Valid={validResult}, Invalid={invalidResult}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempValidFile + ".png")) File.Delete(tempValidFile + ".png");
            if (File.Exists(tempInvalidFile + ".txt")) File.Delete(tempInvalidFile + ".txt");
            if (File.Exists(tempValidFile)) File.Delete(tempValidFile);
            if (File.Exists(tempInvalidFile)) File.Delete(tempInvalidFile);
        }
    }

    /// <summary>
    /// Creates test image bytes for testing purposes
    /// </summary>
    private byte[] CreateTestImageBytes(Color? backgroundColor = null)
    {
        var color = backgroundColor ?? Color.Green;

        using var bitmap = new Bitmap(200, 150);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.Clear(color);

        // Draw some test content
        using var pen = new Pen(Color.Black, 2);
        graphics.DrawRectangle(pen, 10, 10, 180, 130);

        using var font = new Font("Arial", 12);
        graphics.DrawString("TEST IMAGE", font, Brushes.Black, 50, 65);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}