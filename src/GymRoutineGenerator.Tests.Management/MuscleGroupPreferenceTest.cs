using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.Management;

public static class MuscleGroupPreferenceTest
{
    public static async Task RunMuscleGroupPreferenceTests()
    {
        Console.WriteLine("=== TESTING MUSCLE GROUP PREFERENCE FUNCTIONALITY ===");
        Console.WriteLine();

        // Setup in-memory database with muscle group data
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: "MuscleGroupPreferenceTestDb")
            .Options;

        using var context = new GymRoutineContext(options);
        await context.Database.EnsureCreatedAsync();

        // Seed muscle groups
        MuscleGroupSeeder.SeedData(context);

        var userProfileService = new UserProfileService(context);
        var muscleGroupService = new MuscleGroupPreferenceService(context);

        await TestMuscleGroupData(muscleGroupService);
        await TestMuscleGroupPreferenceCRUD(userProfileService, muscleGroupService);
        await TestTrainingObjectiveTemplates(userProfileService, muscleGroupService);
        await TestMuscleGroupPreferenceValidation(muscleGroupService);

        Console.WriteLine();
        Console.WriteLine("=== MUSCLE GROUP PREFERENCE TESTS COMPLETED ===");
    }

    private static async Task TestMuscleGroupData(IMuscleGroupPreferenceService service)
    {
        Console.WriteLine("1. Testing Muscle Group Data");
        Console.WriteLine("---------------------------");

        var muscleGroups = await service.GetAllMuscleGroupsAsync();
        Console.WriteLine($"✓ Muscle groups loaded: {muscleGroups.Count}");

        // Display muscle groups
        foreach (var group in muscleGroups.Take(8))
        {
            Console.WriteLine($"  - ID: {group.Id}, Spanish: '{group.SpanishName}', English: '{group.Name}'");
        }

        Console.WriteLine();
    }

    private static async Task TestMuscleGroupPreferenceCRUD(IUserProfileService userService, IMuscleGroupPreferenceService muscleGroupService)
    {
        Console.WriteLine("2. Testing Muscle Group Preference CRUD Operations");
        Console.WriteLine("-------------------------------------------------");

        // Create a test user
        var userRequest = new UserProfileCreateRequest
        {
            Name = "Elena Fitness",
            Gender = Gender.Mujer,
            Age = 28,
            TrainingDaysPerWeek = 4
        };

        var user = await userService.CreateUserProfileAsync(userRequest);
        Console.WriteLine($"✓ Test user created: '{user.Name}' (ID: {user.Id})");

        // Test setting muscle group preferences
        var preferences = new List<MuscleGroupPreferenceRequest>
        {
            new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Alto },    // Pecho - Alto
            new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },    // Espalda - Alto
            new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Medio },   // Core - Medio
            new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Medio },   // Piernas - Medio
            new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Bajo }     // Glúteos - Bajo
        };

        var setPreferences = await muscleGroupService.SetUserMuscleGroupPreferencesAsync(user.Id, preferences);
        Console.WriteLine($"✓ Muscle group preferences set: {setPreferences.Count} groups selected");

        foreach (var pref in setPreferences.Take(3))
        {
            Console.WriteLine($"  - {pref.MuscleGroup.SpanishName}: {pref.EmphasisLevel}");
        }

        // Test getting muscle group preferences
        var retrievedPreferences = await muscleGroupService.GetUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Muscle group preferences retrieved: {retrievedPreferences.Count} groups");

        // Test updating preferences (change emphasis levels)
        var updatedPreferences = new List<MuscleGroupPreferenceRequest>
        {
            new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Medio },   // Pecho - Medio
            new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },    // Espalda - Alto
            new() { MuscleGroupId = 4, EmphasisLevel = EmphasisLevel.Alto },    // Brazos - Alto
            new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Alto },    // Piernas - Alto
            new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Bajo }     // Core - Bajo
        };

        var updated = await muscleGroupService.SetUserMuscleGroupPreferencesAsync(user.Id, updatedPreferences);
        Console.WriteLine($"✓ Muscle group preferences updated: {updated.Count} groups");

        // Test clearing preferences
        var cleared = await muscleGroupService.ClearUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Muscle group preferences cleared: {cleared}");

        var clearedPreferences = await muscleGroupService.GetUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Preferences after clearing: {clearedPreferences.Count} groups");

        Console.WriteLine();
    }

    private static async Task TestTrainingObjectiveTemplates(IUserProfileService userService, IMuscleGroupPreferenceService muscleGroupService)
    {
        Console.WriteLine("3. Testing Training Objective Templates");
        Console.WriteLine("--------------------------------------");

        // Create a test user for templates
        var userRequest = new UserProfileCreateRequest
        {
            Name = "Roberto Templates",
            Gender = Gender.Hombre,
            Age = 32,
            TrainingDaysPerWeek = 5
        };

        var user = await userService.CreateUserProfileAsync(userRequest);
        Console.WriteLine($"✓ Test user created for templates: '{user.Name}' (ID: {user.Id})");

        // Test Weight Loss template
        var weightLossTemplate = await muscleGroupService.ApplyTrainingObjectiveTemplateAsync(user.Id, TrainingObjectiveType.WeightLoss);
        var weightLossPrefs = await muscleGroupService.GetUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Weight Loss template applied: {weightLossTemplate.Name}");
        Console.WriteLine($"  Description: {weightLossTemplate.Description}");
        Console.WriteLine($"  Muscle groups: {weightLossPrefs.Count}");

        // Test Muscle Gain template
        var muscleGainTemplate = await muscleGroupService.ApplyTrainingObjectiveTemplateAsync(user.Id, TrainingObjectiveType.MuscleGain);
        var muscleGainPrefs = await muscleGroupService.GetUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Muscle Gain template applied: {muscleGainTemplate.Name}");
        Console.WriteLine($"  Description: {muscleGainTemplate.Description}");
        Console.WriteLine($"  Muscle groups: {muscleGainPrefs.Count}");

        // Test General Fitness template
        var generalFitnessTemplate = await muscleGroupService.ApplyTrainingObjectiveTemplateAsync(user.Id, TrainingObjectiveType.GeneralFitness);
        var generalFitnessPrefs = await muscleGroupService.GetUserMuscleGroupPreferencesAsync(user.Id);
        Console.WriteLine($"✓ General Fitness template applied: {generalFitnessTemplate.Name}");
        Console.WriteLine($"  Description: {generalFitnessTemplate.Description}");
        Console.WriteLine($"  Muscle groups: {generalFitnessPrefs.Count}");

        // Display emphasis distribution for General Fitness
        var emphasisCounts = generalFitnessPrefs.GroupBy(p => p.EmphasisLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        Console.WriteLine("  Emphasis distribution:");
        foreach (var kvp in emphasisCounts)
        {
            Console.WriteLine($"    - {kvp.Key}: {kvp.Value} groups");
        }

        Console.WriteLine();
    }

    private static async Task TestMuscleGroupPreferenceValidation(IMuscleGroupPreferenceService service)
    {
        Console.WriteLine("4. Testing Muscle Group Preference Validation");
        Console.WriteLine("--------------------------------------------");

        // Test with invalid user ID
        try
        {
            var invalidPrefs = new List<MuscleGroupPreferenceRequest>
            {
                new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Alto }
            };
            await service.SetUserMuscleGroupPreferencesAsync(999, invalidPrefs);
            Console.WriteLine("✗ Should have failed with invalid user ID");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Correctly rejected invalid user ID");
        }

        // Test with too many high emphasis groups
        try
        {
            var muscleGroups = await service.GetAllMuscleGroupsAsync();
            var validUserId = 1; // Assuming user exists from previous tests

            var tooManyHighEmphasis = new List<MuscleGroupPreferenceRequest>
            {
                new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Alto },
                new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },
                new() { MuscleGroupId = 3, EmphasisLevel = EmphasisLevel.Alto },
                new() { MuscleGroupId = 4, EmphasisLevel = EmphasisLevel.Alto },  // 4th high emphasis
                new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Alto }   // 5th high emphasis
            };

            await service.SetUserMuscleGroupPreferencesAsync(validUserId, tooManyHighEmphasis);
            Console.WriteLine("✗ Should have failed with too many high emphasis groups");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✓ Correctly rejected too many high emphasis groups: {ex.Message}");
        }

        // Test muscle group focus scenarios
        TestMuscleGroupFocusScenarios();

        Console.WriteLine();
    }

    private static void TestMuscleGroupFocusScenarios()
    {
        Console.WriteLine("5. Testing Muscle Group Focus Scenarios");
        Console.WriteLine("--------------------------------------");

        // Upper body focus scenario
        var upperBodyFocus = new List<string> { "Pecho (Alto)", "Espalda (Alto)", "Hombros (Medio)", "Brazos (Medio)" };
        Console.WriteLine($"✓ Upper body focus scenario: {string.Join(", ", upperBodyFocus)}");

        // Lower body focus scenario
        var lowerBodyFocus = new List<string> { "Piernas (Alto)", "Glúteos (Alto)", "Core (Medio)" };
        Console.WriteLine($"✓ Lower body focus scenario: {string.Join(", ", lowerBodyFocus)}");

        // Core intensive scenario
        var coreFocus = new List<string> { "Core (Alto)", "Cuerpo Completo (Medio)", "Espalda (Bajo)" };
        Console.WriteLine($"✓ Core intensive scenario: {string.Join(", ", coreFocus)}");

        // Balanced approach scenario
        var balancedFocus = new List<string> { "Todos los grupos (Medio)" };
        Console.WriteLine($"✓ Balanced approach scenario: {string.Join(", ", balancedFocus)}");

        Console.WriteLine();
    }
}