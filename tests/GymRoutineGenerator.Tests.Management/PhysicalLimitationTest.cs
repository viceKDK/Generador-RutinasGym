using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.Management;

public static class PhysicalLimitationTest
{
    public static async Task RunPhysicalLimitationTests()
    {
        Console.WriteLine("=== TESTING PHYSICAL LIMITATION FUNCTIONALITY ===");
        Console.WriteLine();

        // Setup in-memory database with exercise data
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: "PhysicalLimitationTestDb")
            .Options;

        using var context = new GymRoutineContext(options);
        await context.Database.EnsureCreatedAsync();

        // Seed data
        MuscleGroupSeeder.SeedData(context);
        EquipmentTypeSeeder.SeedData(context);

        var userProfileService = new UserProfileService(context);
        var limitationService = new PhysicalLimitationService(context);

        await TestPhysicalLimitationCRUD(userProfileService, limitationService);
        await TestExerciseSearchForExclusion(limitationService);
        await TestIntensityRecommendations(limitationService);
        await TestSafetyGuidelines(limitationService);
        await TestLimitationValidationScenarios(limitationService);

        Console.WriteLine();
        Console.WriteLine("=== PHYSICAL LIMITATION TESTS COMPLETED ===");
    }

    private static async Task TestPhysicalLimitationCRUD(IUserProfileService userService, IPhysicalLimitationService limitationService)
    {
        Console.WriteLine("1. Testing Physical Limitation CRUD Operations");
        Console.WriteLine("--------------------------------------------");

        // Create a test user
        var userRequest = new UserProfileCreateRequest
        {
            Name = "María Salud",
            Gender = Gender.Mujer,
            Age = 45,
            TrainingDaysPerWeek = 3
        };

        var user = await userService.CreateUserProfileAsync(userRequest);
        Console.WriteLine($"✓ Test user created: '{user.Name}' (ID: {user.Id})");

        // Test setting physical limitations
        var limitations = new List<PhysicalLimitationRequest>
        {
            new() { LimitationType = LimitationType.ProblemasEspalda, Description = "Hernia lumbar L4-L5" },
            new() { LimitationType = LimitationType.ProblemasRodilla, Description = "Artritis en rodilla derecha" },
            new() { LimitationType = LimitationType.Personalizada, CustomRestrictions = "No puede levantar más de 15kg debido a cirugía reciente" }
        };

        var setLimitations = await limitationService.SetUserPhysicalLimitationsAsync(user.Id, limitations);
        Console.WriteLine($"✓ Physical limitations set: {setLimitations.Count} limitations registered");

        foreach (var limitation in setLimitations.Take(3))
        {
            Console.WriteLine($"  - {limitation.LimitationType}: {limitation.Description}");
        }

        // Test getting physical limitations
        var retrievedLimitations = await limitationService.GetUserPhysicalLimitationsAsync(user.Id);
        Console.WriteLine($"✓ Physical limitations retrieved: {retrievedLimitations.Count} limitations");

        // Test updating limitations (add pregnancy)
        var updatedLimitations = new List<PhysicalLimitationRequest>
        {
            new() { LimitationType = LimitationType.Embarazo, Description = "Segundo trimestre de embarazo" },
            new() { LimitationType = LimitationType.ProblemasEspalda, Description = "Hernia lumbar L4-L5 - mejorada" }
        };

        var updated = await limitationService.SetUserPhysicalLimitationsAsync(user.Id, updatedLimitations);
        Console.WriteLine($"✓ Physical limitations updated: {updated.Count} limitations");

        // Test clearing limitations
        var cleared = await limitationService.ClearUserPhysicalLimitationsAsync(user.Id);
        Console.WriteLine($"✓ Physical limitations cleared: {cleared}");

        var clearedLimitations = await limitationService.GetUserPhysicalLimitationsAsync(user.Id);
        Console.WriteLine($"✓ Limitations after clearing: {clearedLimitations.Count} limitations");

        Console.WriteLine();
    }

    private static async Task TestExerciseSearchForExclusion(IPhysicalLimitationService service)
    {
        Console.WriteLine("2. Testing Exercise Search for Exclusion");
        Console.WriteLine("--------------------------------------");

        // Test search functionality (using mock data since we don't have exercises seeded)
        var searchTerms = new[] { "sentadilla", "peso", "flexion", "salto" };

        foreach (var term in searchTerms)
        {
            var results = await service.SearchExercisesForExclusionAsync(term, 5);
            Console.WriteLine($"✓ Search '{term}': {results.Count} exercises found");
        }

        // Test empty search
        var emptyResults = await service.SearchExercisesForExclusionAsync("", 5);
        Console.WriteLine($"✓ Empty search: {emptyResults.Count} exercises found");

        // Test non-existent exercise
        var noResults = await service.SearchExercisesForExclusionAsync("xyz123", 5);
        Console.WriteLine($"✓ Non-existent search: {noResults.Count} exercises found");

        Console.WriteLine();
    }

    private static async Task TestIntensityRecommendations(IPhysicalLimitationService service)
    {
        Console.WriteLine("3. Testing Intensity Recommendations");
        Console.WriteLine("----------------------------------");

        // Test no limitations
        var noLimitations = new List<LimitationType>();
        var normalRecommendation = await service.GetRecommendedIntensityAsync(noLimitations);
        Console.WriteLine($"✓ No limitations - Intensity: {normalRecommendation.RecommendedLevel}/5");
        Console.WriteLine($"  Reason: {normalRecommendation.RecommendationReason}");

        // Test critical conditions
        var criticalLimitations = new List<LimitationType>
        {
            LimitationType.ProblemasCardivasculares,
            LimitationType.Embarazo
        };
        var criticalRecommendation = await service.GetRecommendedIntensityAsync(criticalLimitations);
        Console.WriteLine($"✓ Critical conditions - Intensity: {criticalRecommendation.RecommendedLevel}/5");
        Console.WriteLine($"  Precautions: {criticalRecommendation.Precautions.Count}");

        // Test multiple limitations
        var multipleLimitations = new List<LimitationType>
        {
            LimitationType.ProblemasEspalda,
            LimitationType.ProblemasRodilla,
            LimitationType.ProblemasHombro,
            LimitationType.Artritis
        };
        var multipleRecommendation = await service.GetRecommendedIntensityAsync(multipleLimitations);
        Console.WriteLine($"✓ Multiple limitations - Intensity: {multipleRecommendation.RecommendedLevel}/5");
        Console.WriteLine($"  Reason: {multipleRecommendation.RecommendationReason}");

        // Test single limitation
        var singleLimitation = new List<LimitationType> { LimitationType.ProblemasEspalda };
        var singleRecommendation = await service.GetRecommendedIntensityAsync(singleLimitation);
        Console.WriteLine($"✓ Single limitation - Intensity: {singleRecommendation.RecommendedLevel}/5");

        Console.WriteLine();
    }

    private static async Task TestSafetyGuidelines(IPhysicalLimitationService service)
    {
        Console.WriteLine("4. Testing Safety Guidelines");
        Console.WriteLine("---------------------------");

        // Test guidelines for back problems
        var backProblems = new List<LimitationType> { LimitationType.ProblemasEspalda };
        var backGuidelines = await service.GetSafetyGuidelinesAsync(backProblems);
        Console.WriteLine($"✓ Back problems guidelines:");
        Console.WriteLine($"  General precautions: {backGuidelines.GeneralPrecautions.Count}");
        Console.WriteLine($"  Exercises to avoid: {backGuidelines.ExercisesToAvoid.Count}");
        Console.WriteLine($"  Recommended modifications: {backGuidelines.RecommendedModifications.Count}");

        // Test guidelines for pregnancy
        var pregnancy = new List<LimitationType> { LimitationType.Embarazo };
        var pregnancyGuidelines = await service.GetSafetyGuidelinesAsync(pregnancy);
        Console.WriteLine($"✓ Pregnancy guidelines:");
        Console.WriteLine($"  Exercises to avoid: {pregnancyGuidelines.ExercisesToAvoid.Count}");
        Console.WriteLine($"  Recommended modifications: {pregnancyGuidelines.RecommendedModifications.Count}");

        // Test guidelines for multiple conditions
        var multipleConditions = new List<LimitationType>
        {
            LimitationType.ProblemasRodilla,
            LimitationType.Artritis
        };
        var multipleGuidelines = await service.GetSafetyGuidelinesAsync(multipleConditions);
        Console.WriteLine($"✓ Multiple conditions guidelines:");
        Console.WriteLine($"  Total exercises to avoid: {multipleGuidelines.ExercisesToAvoid.Count}");
        Console.WriteLine($"  Total modifications: {multipleGuidelines.RecommendedModifications.Count}");

        // Display sample safety information
        if (backGuidelines.ExercisesToAvoid.Any())
        {
            Console.WriteLine($"  Example exercise to avoid: {backGuidelines.ExercisesToAvoid.First()}");
        }

        Console.WriteLine();
    }

    private static async Task TestLimitationValidationScenarios(IPhysicalLimitationService service)
    {
        Console.WriteLine("5. Testing Limitation Validation Scenarios");
        Console.WriteLine("-----------------------------------------");

        // Test invalid limitation type
        try
        {
            var invalidLimitations = new List<PhysicalLimitationRequest>
            {
                new() { LimitationType = (LimitationType)999, Description = "Invalid limitation" }
            };
            await service.SetUserPhysicalLimitationsAsync(1, invalidLimitations);
            Console.WriteLine("✗ Should have failed with invalid limitation type");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Correctly rejected invalid limitation type");
        }

        // Test limitation scenarios
        TestLimitationScenarios();

        Console.WriteLine();
    }

    private static void TestLimitationScenarios()
    {
        Console.WriteLine("6. Testing Limitation Scenarios");
        Console.WriteLine("------------------------------");

        // Elderly with multiple conditions
        var elderlyConditions = new List<string>
        {
            "Problemas de Espalda", "Artritis", "Problemas Cardiovasculares"
        };
        Console.WriteLine($"✓ Elderly scenario: {string.Join(", ", elderlyConditions)} - Very Low Intensity");

        // Pregnant woman
        var pregnancyConditions = new List<string> { "Embarazo (Segundo Trimestre)" };
        Console.WriteLine($"✓ Pregnancy scenario: {string.Join(", ", pregnancyConditions)} - Very Low Intensity");

        // Recent injury recovery
        var injuryConditions = new List<string> { "Lesión Reciente", "Problemas de Rodilla" };
        Console.WriteLine($"✓ Injury recovery scenario: {string.Join(", ", injuryConditions)} - Low Intensity");

        // Single limitation
        var singleCondition = new List<string> { "Problemas de Hombro" };
        Console.WriteLine($"✓ Single limitation scenario: {string.Join(", ", singleCondition)} - Low/Moderate Intensity");

        // No limitations
        Console.WriteLine($"✓ Healthy individual scenario: Sin limitaciones - Normal Intensity");

        // Custom restrictions
        var customScenario = "No puede levantar más de 10kg, evitar ejercicios de torsión";
        Console.WriteLine($"✓ Custom restrictions scenario: {customScenario}");

        Console.WriteLine();
    }
}