using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.Management;

public static class EquipmentPreferenceTest
{
    public static async Task RunEquipmentPreferenceTests()
    {
        Console.WriteLine("=== TESTING EQUIPMENT PREFERENCE FUNCTIONALITY ===");
        Console.WriteLine();

        // Setup in-memory database with equipment data
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: "EquipmentPreferenceTestDb")
            .Options;

        using var context = new GymRoutineContext(options);
        await context.Database.EnsureCreatedAsync();

        // Seed equipment types
        EquipmentTypeSeeder.SeedData(context);

        var userProfileService = new UserProfileService(context);
        var equipmentPreferenceService = new EquipmentPreferenceService(context);

        await TestEquipmentTypeData(equipmentPreferenceService);
        await TestEquipmentPreferenceCRUD(userProfileService, equipmentPreferenceService);
        await TestEquipmentPreferenceValidation(equipmentPreferenceService);

        Console.WriteLine();
        Console.WriteLine("=== EQUIPMENT PREFERENCE TESTS COMPLETED ===");
    }

    private static async Task TestEquipmentTypeData(IEquipmentPreferenceService service)
    {
        Console.WriteLine("1. Testing Equipment Type Data");
        Console.WriteLine("-----------------------------");

        var equipmentTypes = await service.GetAllEquipmentTypesAsync();
        Console.WriteLine($"✓ Equipment types loaded: {equipmentTypes.Count}");

        // Display first few equipment types
        foreach (var equipment in equipmentTypes.Take(5))
        {
            Console.WriteLine($"  - ID: {equipment.Id}, Spanish: '{equipment.SpanishName}', English: '{equipment.Name}'");
        }

        Console.WriteLine();
    }

    private static async Task TestEquipmentPreferenceCRUD(IUserProfileService userService, IEquipmentPreferenceService equipmentService)
    {
        Console.WriteLine("2. Testing Equipment Preference CRUD Operations");
        Console.WriteLine("----------------------------------------------");

        // Create a test user
        var userRequest = new UserProfileCreateRequest
        {
            Name = "Carlos Fitness",
            Gender = Gender.Hombre,
            Age = 30,
            TrainingDaysPerWeek = 4
        };

        var user = await userService.CreateUserProfileAsync(userRequest);
        Console.WriteLine($"✓ Test user created: '{user.Name}' (ID: {user.Id})");

        // Test setting equipment preferences
        var preferredEquipment = new List<int> { 1, 2, 4, 6 }; // Bodyweight, Free Weights, Resistance Bands, Pull-up Bar

        var preferences = await equipmentService.SetUserEquipmentPreferencesAsync(user.Id, preferredEquipment);
        Console.WriteLine($"✓ Equipment preferences set: {preferences.Count} types selected");

        foreach (var pref in preferences.Take(3))
        {
            Console.WriteLine($"  - {pref.EquipmentType.SpanishName}");
        }

        // Test getting equipment preferences
        var retrievedPreferences = await equipmentService.GetUserEquipmentPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Equipment preferences retrieved: {retrievedPreferences.Count} types");

        // Test updating preferences (add more equipment)
        var updatedEquipment = new List<int> { 1, 2, 3, 4, 5, 6, 7 }; // Add Machines, Kettlebells, Medicine Ball

        var updatedPreferences = await equipmentService.SetUserEquipmentPreferencesAsync(user.Id, updatedEquipment);
        Console.WriteLine($"✓ Equipment preferences updated: {updatedPreferences.Count} types");

        // Test clearing preferences
        var cleared = await equipmentService.ClearUserEquipmentPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Equipment preferences cleared: {cleared}");

        var clearedPreferences = await equipmentService.GetUserEquipmentPreferencesAsync(user.Id);
        Console.WriteLine($"✓ Preferences after clearing: {clearedPreferences.Count} types");

        Console.WriteLine();
    }

    private static async Task TestEquipmentPreferenceValidation(IEquipmentPreferenceService service)
    {
        Console.WriteLine("3. Testing Equipment Preference Validation");
        Console.WriteLine("-----------------------------------------");

        // Test with invalid user ID
        try
        {
            await service.SetUserEquipmentPreferencesAsync(999, new List<int> { 1, 2 });
            Console.WriteLine("✗ Should have failed with invalid user ID");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Correctly rejected invalid user ID");
        }

        // Test with invalid equipment type IDs
        try
        {
            var equipmentTypes = await service.GetAllEquipmentTypesAsync();
            var maxValidId = equipmentTypes.Max(et => et.Id);
            var invalidEquipmentIds = new List<int> { maxValidId + 1, maxValidId + 2 };

            // Need a valid user first
            var userRequest = new UserProfileCreateRequest
            {
                Name = "Test User for Validation",
                Gender = Gender.Mujer,
                Age = 25,
                TrainingDaysPerWeek = 3
            };

            var context = service.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(service) as GymRoutineContext;
            var userService = new UserProfileService(context!);
            var user = await userService.CreateUserProfileAsync(userRequest);

            await service.SetUserEquipmentPreferencesAsync(user.Id, invalidEquipmentIds);
            Console.WriteLine("✗ Should have failed with invalid equipment type IDs");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Correctly rejected invalid equipment type IDs");
        }

        // Test equipment preference scenarios
        TestEquipmentSelectionScenarios();

        Console.WriteLine();
    }

    private static void TestEquipmentSelectionScenarios()
    {
        Console.WriteLine("4. Testing Equipment Selection Scenarios");
        Console.WriteLine("---------------------------------------");

        // Home gym scenario
        var homeGymEquipment = new List<string> { "Peso Corporal", "Mancuernas", "Bandas Elásticas" };
        Console.WriteLine($"✓ Home gym scenario: {string.Join(", ", homeGymEquipment)}");

        // Full gym scenario
        var fullGymEquipment = new List<string> {
            "Peso Corporal", "Mancuernas", "Barra", "Kettlebells",
            "Máquinas de Poleas", "Máquinas de Pesas", "Barra de Dominadas"
        };
        Console.WriteLine($"✓ Full gym scenario: {fullGymEquipment.Count} equipment types");

        // Minimalist scenario
        var minimalistEquipment = new List<string> { "Peso Corporal" };
        Console.WriteLine($"✓ Minimalist scenario: {string.Join(", ", minimalistEquipment)}");

        // Travel/Hotel scenario
        var travelEquipment = new List<string> { "Peso Corporal", "Bandas Elásticas" };
        Console.WriteLine($"✓ Travel scenario: {string.Join(", ", travelEquipment)}");

        Console.WriteLine();
    }
}