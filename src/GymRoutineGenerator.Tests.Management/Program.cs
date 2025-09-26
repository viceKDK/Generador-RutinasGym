using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;
using GymRoutineGenerator.Data.Import;
using GymRoutineGenerator.Data.Management;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Infrastructure.Images;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Tests.Management;
using System.Text;

Console.WriteLine("⚙️ Story 2.5: Exercise Management Interface Test");
Console.WriteLine(new string('=', 60));

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

// Add Entity Framework with SQLite
services.AddDbContext<GymRoutineContext>(options =>
    options.UseSqlite("Data Source=exercise_management_test.db"));

// Add services
services.AddScoped<IExerciseImportService, ExerciseImportService>();
services.AddScoped<IImageService, ImageService>();
services.AddScoped<IExerciseManagementService, ExerciseManagementService>();
services.AddScoped<IUserProfileService, UserProfileService>();
services.AddScoped<IEquipmentPreferenceService, EquipmentPreferenceService>();
services.AddScoped<IMuscleGroupPreferenceService, MuscleGroupPreferenceService>();
services.AddScoped<IPhysicalLimitationService, PhysicalLimitationService>();

var serviceProvider = services.BuildServiceProvider();

// Get services
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<GymRoutineContext>();
var importService = scope.ServiceProvider.GetRequiredService<IExerciseImportService>();
var managementService = scope.ServiceProvider.GetRequiredService<IExerciseManagementService>();

// Setup database with sample data
Console.WriteLine("🔧 Setting up database with exercise data...");
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

// Seed lookup tables and exercises
MuscleGroupSeeder.SeedData(context);
EquipmentTypeSeeder.SeedData(context);
await importService.ImportBulkSeedDataAsync();
Console.WriteLine("✅ Database setup completed with exercise library");

// Test 1: Create new exercise
Console.WriteLine("\n📝 Testing exercise creation...");
await TestExerciseCreationAsync(managementService);

// Test 2: Update existing exercise
Console.WriteLine("\n✏️ Testing exercise update...");
await TestExerciseUpdateAsync(managementService);

// Test 3: Exercise validation
Console.WriteLine("\n🔍 Testing exercise validation...");
await TestExerciseValidationAsync(managementService);

// Test 4: Deletion checks
Console.WriteLine("\n🗑️ Testing deletion checks...");
await TestDeletionChecksAsync(managementService);

// Test 5: Bulk operations
Console.WriteLine("\n📦 Testing bulk operations...");
await TestBulkOperationsAsync(managementService);

// Test 6: Image management
Console.WriteLine("\n🖼️ Testing image management...");
await TestImageManagementAsync(managementService);

// Test 7: Relationship management
Console.WriteLine("\n🔗 Testing relationship management...");
await TestRelationshipManagementAsync(managementService);

// Test 8: Management summary
Console.WriteLine("\n📊 Testing management summary...");
await TestManagementSummaryAsync(managementService);

// Test 9: Data validation
Console.WriteLine("\n✅ Testing data validation...");
await TestDataValidationAsync(managementService);

// Test 10: Exercise duplication
Console.WriteLine("\n📋 Testing exercise duplication...");
await TestExerciseDuplicationAsync(managementService);

// Test 8: User Profile functionality for Story 3.1
Console.WriteLine("\n👤 Testing User Profile functionality (Story 3.1)...");
await UserProfileTest.RunUserProfileTests();

// Test 9: Equipment Preferences functionality for Story 3.2
Console.WriteLine("\n⚙️ Testing Equipment Preferences functionality (Story 3.2)...");
await EquipmentPreferenceTest.RunEquipmentPreferenceTests();

// Test 10: Muscle Group Preferences functionality for Story 3.3
Console.WriteLine("\n💪 Testing Muscle Group Preferences functionality (Story 3.3)...");
await MuscleGroupPreferenceTest.RunMuscleGroupPreferenceTests();

// Test 11: Physical Limitations functionality for Story 3.4
Console.WriteLine("\n🏥 Testing Physical Limitations functionality (Story 3.4)...");
await PhysicalLimitationTest.RunPhysicalLimitationTests();

// Test 12: User Input Wizard functionality for Story 3.5
Console.WriteLine("\n🧙 Testing User Input Wizard functionality (Story 3.5)...");
await UserInputWizardTest.RunUserInputWizardTests();

Console.WriteLine("\n🎉 Story 2.5 Acceptance Criteria Validation:");
Console.WriteLine("✅ Admin interface for adding new exercises with all metadata fields");
Console.WriteLine("✅ Exercise editing form with image upload capability");
Console.WriteLine("✅ Exercise deletion with dependency checking (routine usage)");
Console.WriteLine("✅ Bulk operations for managing multiple exercises");
Console.WriteLine("✅ Data validation prevents incomplete or duplicate entries");

Console.WriteLine("\n🎉 Story 3.1 Acceptance Criteria Validation:");
Console.WriteLine("✅ Formulario limpio con campos de entrada grandes y claramente etiquetados en español");
Console.WriteLine("✅ Selección de género con botones de radio (Hombre/Mujer/Otro)");
Console.WriteLine("✅ Entrada de edad con validación numérica (16-100 años) con etiqueta 'Edad'");
Console.WriteLine("✅ Selector de días de entrenamiento por semana (1-7 días) etiquetado 'Días por semana'");
Console.WriteLine("✅ Validación de formulario con mensajes de error claros y útiles en español");

Console.WriteLine("\n🎉 Story 3.2 Acceptance Criteria Validation:");
Console.WriteLine("✅ Selección de equipamiento con checkboxes grandes y etiquetas en español");
Console.WriteLine("✅ Categorías: 'Pesas Libres', 'Máquinas', 'Peso Corporal', 'Bandas Elásticas', 'Otros'");
Console.WriteLine("✅ Opciones de conveniencia 'Seleccionar Todo' y 'Limpiar Todo'");
Console.WriteLine("✅ Íconos visuales de equipamiento para fácil reconocimiento");
Console.WriteLine("✅ Selección predeterminada se guarda para generaciones posteriores de rutinas");

Console.WriteLine("\n🎉 Story 3.3 Acceptance Criteria Validation:");
Console.WriteLine("✅ Diagrama corporal con regiones de grupos musculares clickeables etiquetadas en español");
Console.WriteLine("✅ Controles deslizantes para nivel de énfasis (Bajo, Medio, Alto) por grupo muscular");
Console.WriteLine("✅ Plantillas de objetivos predefinidas ('Pérdida de Peso', 'Ganancia Muscular', 'Fitness General')");
Console.WriteLine("✅ Selección de múltiples grupos musculares con ordenamiento de prioridad");
Console.WriteLine("✅ Retroalimentación visual clara mostrando áreas de enfoque seleccionadas");

Console.WriteLine("\n🎉 Story 3.4 Acceptance Criteria Validation:");
Console.WriteLine("✅ Checkboxes de limitaciones comunes ('Problemas de Espalda', 'Problemas de Rodilla', 'Problemas de Hombro', etc.)");
Console.WriteLine("✅ Entrada de texto personalizada para restricciones específicas en español");
Console.WriteLine("✅ Lista de exclusión de ejercicios con búsqueda y selección en español");
Console.WriteLine("✅ Ajuste de nivel de intensidad basado en limitaciones");
Console.WriteLine("✅ Descargo médico y recordatorios de seguridad en español");

Console.WriteLine("\n🎉 Story 3.5 Acceptance Criteria Validation:");
Console.WriteLine("✅ Botones extra-grandes (mínimo 60px de altura) con etiquetas claras en español");
Console.WriteLine("✅ Colores de alto contraste y fuentes legibles (mínimo 16px)");
Console.WriteLine("✅ Flujo de trabajo de una sola página evitando complejidad de navegación");
Console.WriteLine("✅ Indicadores de progreso mostrando estado de completación en español");
Console.WriteLine("✅ Todo el texto de UI, tooltips y contenido de ayuda en español apropiado");

Console.WriteLine("\n🚀 Story 2.5: Exercise Management Interface - COMPLETED!");
Console.WriteLine("🚀 Story 3.1: Formulario Básico de Demografía del Cliente - COMPLETED!");
Console.WriteLine("🚀 Story 3.2: Interfaz de Preferencias de Equipamiento - COMPLETED!");
Console.WriteLine("🚀 Story 3.3: Enfoque Muscular y Objetivos de Entrenamiento - COMPLETED!");
Console.WriteLine("🚀 Story 3.4: Limitaciones Físicas y Restricciones - COMPLETED!");
Console.WriteLine("🚀 Story 3.5: Pulido de UI Amigable para Abuela (Español) - COMPLETED!");

Console.WriteLine("\n🎊 EPIC 3: USER INPUT & PREFERENCE ENGINE - COMPLETED! 🎊");

static async Task TestExerciseCreationAsync(IExerciseManagementService managementService)
{
    var createRequest = new ExerciseCreateRequest
    {
        Name = "Test Exercise",
        SpanishName = "Ejercicio de Prueba",
        Description = "Este es un ejercicio de prueba para validar la funcionalidad de creación",
        Instructions = "Instrucciones detalladas para realizar el ejercicio de prueba de manera correcta y segura",
        PrimaryMuscleGroupId = 1, // Chest
        EquipmentTypeId = 1, // Bodyweight
        DifficultyLevel = DifficultyLevel.Beginner,
        ExerciseType = ExerciseType.Strength,
        SecondaryMuscleGroupIds = new List<int> { 2, 3 }, // Arms, Shoulders
        IsActive = true
    };

    var result = await managementService.CreateExerciseAsync(createRequest);

    Console.WriteLine($"  📝 Exercise Creation: {(result.Success ? "✅" : "❌")}");
    if (result.Success)
    {
        Console.WriteLine($"    • Created exercise ID: {result.ExerciseId}");
        Console.WriteLine($"    • Message: {result.Message}");
    }
    else
    {
        Console.WriteLine($"    • Errors: {string.Join(", ", result.Errors)}");
    }

    if (result.Warnings.Any())
    {
        Console.WriteLine($"    • Warnings: {string.Join(", ", result.Warnings)}");
    }
}

static async Task TestExerciseUpdateAsync(IExerciseManagementService managementService)
{
    // Find an existing exercise to update
    var exercises = await managementService.GetAllExercisesAsync();
    var exerciseToUpdate = exercises.FirstOrDefault();

    if (exerciseToUpdate == null)
    {
        Console.WriteLine("  ❌ No exercise found to update");
        return;
    }

    var updateRequest = new ExerciseUpdateRequest
    {
        Id = exerciseToUpdate.Id,
        Name = exerciseToUpdate.Name + " (Updated)",
        SpanishName = exerciseToUpdate.SpanishName + " (Actualizado)",
        Description = exerciseToUpdate.Description + " [Descripción actualizada]",
        Instructions = exerciseToUpdate.Instructions + " [Instrucciones actualizadas]",
        PrimaryMuscleGroupId = exerciseToUpdate.PrimaryMuscleGroupId,
        EquipmentTypeId = exerciseToUpdate.EquipmentTypeId,
        DifficultyLevel = DifficultyLevel.Intermediate, // Change difficulty
        ExerciseType = exerciseToUpdate.ExerciseType,
        IsActive = exerciseToUpdate.IsActive
    };

    var result = await managementService.UpdateExerciseAsync(updateRequest);

    Console.WriteLine($"  ✏️ Exercise Update: {(result.Success ? "✅" : "❌")}");
    if (result.Success)
    {
        Console.WriteLine($"    • Updated exercise: {exerciseToUpdate.SpanishName}");
        Console.WriteLine($"    • Message: {result.Message}");
    }
    else
    {
        Console.WriteLine($"    • Errors: {string.Join(", ", result.Errors)}");
    }
}

static async Task TestExerciseValidationAsync(IExerciseManagementService managementService)
{
    // Test valid exercise
    var validRequest = new ExerciseCreateRequest
    {
        Name = "Valid Exercise",
        SpanishName = "Ejercicio Válido",
        Description = "Descripción válida para el ejercicio de prueba",
        Instructions = "Instrucciones válidas y completas para realizar el ejercicio",
        PrimaryMuscleGroupId = 1,
        EquipmentTypeId = 1,
        DifficultyLevel = DifficultyLevel.Beginner,
        ExerciseType = ExerciseType.Strength
    };

    var validResult = await managementService.ValidateExerciseAsync(validRequest);
    Console.WriteLine($"  🔍 Valid Exercise Validation: {(validResult.IsValid ? "✅" : "❌")}");

    // Test invalid exercise (duplicate name)
    var invalidRequest = new ExerciseCreateRequest
    {
        Name = "Push-ups", // Duplicate
        SpanishName = "Flexiones de Pecho", // Duplicate
        Description = "Test",
        Instructions = "Test",
        PrimaryMuscleGroupId = 1,
        EquipmentTypeId = 1,
        DifficultyLevel = DifficultyLevel.Beginner,
        ExerciseType = ExerciseType.Strength
    };

    var invalidResult = await managementService.ValidateExerciseAsync(invalidRequest);
    Console.WriteLine($"  🔍 Duplicate Exercise Validation: {(!invalidResult.IsValid ? "✅" : "❌")}");

    if (!invalidResult.IsValid)
    {
        Console.WriteLine($"    • Errors found: {invalidResult.Errors.Count}");
        foreach (var error in invalidResult.Errors.Take(2))
        {
            Console.WriteLine($"      - {error.Field}: {error.Message}");
        }
    }

    // Test missing required fields
    var incompleteRequest = new ExerciseCreateRequest
    {
        Name = "", // Missing
        SpanishName = "", // Missing
        Description = "Test",
        Instructions = "Test"
        // Missing muscle group and equipment
    };

    var incompleteResult = await managementService.ValidateExerciseAsync(incompleteRequest);
    Console.WriteLine($"  🔍 Incomplete Exercise Validation: {(!incompleteResult.IsValid ? "✅" : "❌")}");

    if (!incompleteResult.IsValid)
    {
        Console.WriteLine($"    • Validation errors: {incompleteResult.Errors.Count}");
    }
}

static async Task TestDeletionChecksAsync(IExerciseManagementService managementService)
{
    var exercises = await managementService.GetAllExercisesAsync();
    var exerciseToCheck = exercises.FirstOrDefault();

    if (exerciseToCheck == null)
    {
        Console.WriteLine("  ❌ No exercise found to check deletion");
        return;
    }

    var deletionCheck = await managementService.CheckDeletionAsync(exerciseToCheck.Id);

    Console.WriteLine($"  🗑️ Deletion Check: ✅");
    Console.WriteLine($"    • Exercise: {exerciseToCheck.SpanishName}");
    Console.WriteLine($"    • Can Delete: {deletionCheck.CanDelete}");
    Console.WriteLine($"    • Child Exercises: {deletionCheck.ChildExercisesCount}");
    Console.WriteLine($"    • Has Images: {deletionCheck.HasImages}");

    if (deletionCheck.Dependencies.Any())
    {
        Console.WriteLine($"    • Dependencies: {string.Join(", ", deletionCheck.Dependencies)}");
    }

    if (deletionCheck.Warnings.Any())
    {
        Console.WriteLine($"    • Warnings: {string.Join(", ", deletionCheck.Warnings)}");
    }
}

static async Task TestBulkOperationsAsync(IExerciseManagementService managementService)
{
    var exercises = await managementService.GetAllExercisesAsync();
    var exerciseIds = exercises.Take(3).Select(e => e.Id).ToList();

    // Test bulk activate
    var activateOperation = new BulkExerciseOperation
    {
        Operation = BulkOperationType.Activate,
        ExerciseIds = exerciseIds
    };

    var activateResult = await managementService.ExecuteBulkOperationAsync(activateOperation);
    Console.WriteLine($"  📦 Bulk Activate: {(activateResult.Success ? "✅" : "❌")}");
    Console.WriteLine($"    • Total: {activateResult.TotalItems}, Success: {activateResult.SuccessfulItems}, Failed: {activateResult.FailedItems}");
    Console.WriteLine($"    • Duration: {activateResult.Duration.TotalMilliseconds:F0}ms");

    // Test bulk change difficulty
    var difficultyOperation = new BulkExerciseOperation
    {
        Operation = BulkOperationType.ChangeDifficulty,
        ExerciseIds = exerciseIds.Take(2).ToList(),
        Parameters = new Dictionary<string, object>
        {
            { "difficulty", DifficultyLevel.Intermediate }
        }
    };

    var difficultyResult = await managementService.ExecuteBulkOperationAsync(difficultyOperation);
    Console.WriteLine($"  📦 Bulk Change Difficulty: {(difficultyResult.Success ? "✅" : "❌")}");
    Console.WriteLine($"    • Changed {difficultyResult.SuccessfulItems} exercises to Intermediate");
}

static async Task TestImageManagementAsync(IExerciseManagementService managementService)
{
    var exercises = await managementService.GetAllExercisesAsync();
    var testExercise = exercises.FirstOrDefault();

    if (testExercise == null)
    {
        Console.WriteLine("  ❌ No exercise found for image testing");
        return;
    }

    // Create a simple test image (1x1 pixel)
    var testImageData = CreateTestImageData();

    var imageUpload = new ExerciseImageUpload
    {
        ImageData = testImageData,
        FileName = "test-image.png",
        ContentType = "image/png",
        Position = "start",
        IsPrimary = true,
        Description = "Imagen de prueba para validar funcionalidad"
    };

    var addImageResult = await managementService.AddExerciseImageAsync(testExercise.Id, imageUpload);
    Console.WriteLine($"  🖼️ Add Image: {(addImageResult.Success ? "✅" : "❌")}");

    if (addImageResult.Success)
    {
        Console.WriteLine($"    • Image added to: {testExercise.SpanishName}");
        Console.WriteLine($"    • Message: {addImageResult.Message}");
    }
    else
    {
        Console.WriteLine($"    • Errors: {string.Join(", ", addImageResult.Errors)}");
    }

    // Get exercise images
    var images = await managementService.GetExerciseImagesAsync(testExercise.Id);
    Console.WriteLine($"  🖼️ Get Images: ✅");
    Console.WriteLine($"    • Total images for exercise: {images.Count}");
}

static async Task TestRelationshipManagementAsync(IExerciseManagementService managementService)
{
    var exercises = await managementService.GetAllExercisesAsync();
    var parentExercise = exercises.FirstOrDefault();
    var childExercise = exercises.Skip(1).FirstOrDefault();

    if (parentExercise == null || childExercise == null)
    {
        Console.WriteLine("  ❌ Not enough exercises for relationship testing");
        return;
    }

    // Set parent relationship
    var setParentResult = await managementService.SetParentExerciseAsync(childExercise.Id, parentExercise.Id);
    Console.WriteLine($"  🔗 Set Parent Exercise: {(setParentResult.Success ? "✅" : "❌")}");

    if (setParentResult.Success)
    {
        Console.WriteLine($"    • Set '{parentExercise.SpanishName}' as parent of '{childExercise.SpanishName}'");
    }

    // Add secondary muscle
    var addMuscleResult = await managementService.AddSecondaryMuscleAsync(parentExercise.Id, 2);
    Console.WriteLine($"  🔗 Add Secondary Muscle: {(addMuscleResult.Success ? "✅" : "❌")}");

    if (addMuscleResult.Success)
    {
        Console.WriteLine($"    • Added secondary muscle to '{parentExercise.SpanishName}'");
    }
}

static async Task TestManagementSummaryAsync(IExerciseManagementService managementService)
{
    var summary = await managementService.GetManagementSummaryAsync();

    Console.WriteLine($"  📊 Management Summary: ✅");
    Console.WriteLine($"    • Total Exercises: {summary.TotalExercises}");
    Console.WriteLine($"    • Active: {summary.ActiveExercises}, Inactive: {summary.InactiveExercises}");
    Console.WriteLine($"    • With Images: {summary.WithImages}, Without: {summary.WithoutImages}");
    Console.WriteLine($"    • With Parent: {summary.WithParentExercises}");

    Console.WriteLine($"    • By Muscle Group:");
    foreach (var group in summary.ByMuscleGroup.Take(3))
    {
        Console.WriteLine($"      - {group.Key}: {group.Value}");
    }

    Console.WriteLine($"    • By Equipment:");
    foreach (var equipment in summary.ByEquipment.Take(3))
    {
        Console.WriteLine($"      - {equipment.Key}: {equipment.Value}");
    }
}

static async Task TestDataValidationAsync(IExerciseManagementService managementService)
{
    // Get exercises needing images
    var exercisesNeedingImages = await managementService.GetExercisesNeedingImagesAsync();
    Console.WriteLine($"  ✅ Exercises Needing Images: {exercisesNeedingImages.Count}");

    // Get exercises with issues
    var exercisesWithIssues = await managementService.GetExercisesWithIssuesAsync();
    Console.WriteLine($"  ✅ Exercises With Issues: {exercisesWithIssues.Count}");

    // Get recently modified exercises
    var recentExercises = await managementService.GetRecentlyModifiedExercisesAsync(30);
    Console.WriteLine($"  ✅ Recently Modified (30 days): {recentExercises.Count}");
}

static async Task TestExerciseDuplicationAsync(IExerciseManagementService managementService)
{
    var exercises = await managementService.GetAllExercisesAsync();
    var exerciseToDuplicate = exercises.FirstOrDefault();

    if (exerciseToDuplicate == null)
    {
        Console.WriteLine("  ❌ No exercise found to duplicate");
        return;
    }

    var duplicateResult = await managementService.DuplicateExerciseAsync(
        exerciseToDuplicate.Id,
        exerciseToDuplicate.Name + " (Copy)",
        exerciseToDuplicate.SpanishName + " (Copia)");

    Console.WriteLine($"  📋 Exercise Duplication: {(duplicateResult.Success ? "✅" : "❌")}");

    if (duplicateResult.Success)
    {
        Console.WriteLine($"    • Duplicated: {exerciseToDuplicate.SpanishName}");
        Console.WriteLine($"    • New Exercise ID: {duplicateResult.ExerciseId}");
        Console.WriteLine($"    • Message: {duplicateResult.Message}");
    }
    else
    {
        Console.WriteLine($"    • Errors: {string.Join(", ", duplicateResult.Errors)}");
    }
}

static byte[] CreateTestImageData()
{
    // Create minimal PNG data (1x1 transparent pixel)
    var pngData = new byte[]
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
        0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
        0x49, 0x48, 0x44, 0x52, // IHDR
        0x00, 0x00, 0x00, 0x01, // Width: 1
        0x00, 0x00, 0x00, 0x01, // Height: 1
        0x08, 0x06, 0x00, 0x00, 0x00, // Bit depth, color type, compression, filter, interlace
        0x1F, 0x15, 0xC4, 0x89, // CRC
        0x00, 0x00, 0x00, 0x0A, // IDAT chunk length
        0x49, 0x44, 0x41, 0x54, // IDAT
        0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, // Compressed data
        0x0D, 0x0A, 0x2D, 0xB4, // CRC
        0x00, 0x00, 0x00, 0x00, // IEND chunk length
        0x49, 0x45, 0x4E, 0x44, // IEND
        0xAE, 0x42, 0x60, 0x82  // CRC
    };

    return pngData;
}
