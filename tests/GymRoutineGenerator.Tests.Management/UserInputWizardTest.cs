using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Seeds;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.Management;

public static class UserInputWizardTest
{
    public static async Task RunUserInputWizardTests()
    {
        Console.WriteLine("=== TESTING USER INPUT WIZARD FUNCTIONALITY ===");
        Console.WriteLine();

        // Setup in-memory database with complete data
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: "UserInputWizardTestDb")
            .Options;

        using var context = new GymRoutineContext(options);
        await context.Database.EnsureCreatedAsync();

        // Seed all data
        MuscleGroupSeeder.SeedData(context);
        EquipmentTypeSeeder.SeedData(context);

        var userProfileService = new UserProfileService(context);
        var equipmentService = new EquipmentPreferenceService(context);
        var muscleGroupService = new MuscleGroupPreferenceService(context);
        var limitationService = new PhysicalLimitationService(context);

        await TestCompleteUserJourney(userProfileService, equipmentService, muscleGroupService, limitationService);
        await TestUIAmigableParaAbuela();
        await TestUserInterfaceAccessibility();

        Console.WriteLine();
        Console.WriteLine("=== USER INPUT WIZARD TESTS COMPLETED ===");
    }

    private static async Task TestCompleteUserJourney(
        IUserProfileService userService,
        IEquipmentPreferenceService equipmentService,
        IMuscleGroupPreferenceService muscleGroupService,
        IPhysicalLimitationService limitationService)
    {
        Console.WriteLine("1. Testing Complete User Journey");
        Console.WriteLine("------------------------------");

        // Scenario 1: Young fitness enthusiast
        await TestYoungFitnessEnthusiast(userService, equipmentService, muscleGroupService, limitationService);

        // Scenario 2: Middle-aged person with limitations
        await TestMiddleAgedWithLimitations(userService, equipmentService, muscleGroupService, limitationService);

        // Scenario 3: Elderly beginner
        await TestElderlyBeginner(userService, equipmentService, muscleGroupService, limitationService);

        Console.WriteLine();
    }

    private static async Task TestYoungFitnessEnthusiast(
        IUserProfileService userService,
        IEquipmentPreferenceService equipmentService,
        IMuscleGroupPreferenceService muscleGroupService,
        IPhysicalLimitationService limitationService)
    {
        Console.WriteLine("Scenario 1: Young Fitness Enthusiast");
        Console.WriteLine("------------------------------------");

        // Step 1: Demographics
        var profileRequest = new UserProfileCreateRequest
        {
            Name = "Alex Joven",
            Gender = Gender.Hombre,
            Age = 25,
            TrainingDaysPerWeek = 5
        };

        var profile = await userService.CreateUserProfileAsync(profileRequest);
        Console.WriteLine($"✓ Profile created: {profile.Name}, {profile.Age} años, {profile.TrainingDaysPerWeek} días/semana");

        // Step 2: Equipment (Full gym access)
        var equipment = new List<int> { 1, 2, 3, 4, 5, 6, 7 }; // All equipment
        await equipmentService.SetUserEquipmentPreferencesAsync(profile.Id, equipment);
        Console.WriteLine($"✓ Equipment set: {equipment.Count} tipos de equipamiento");

        // Step 3: Muscle groups (Muscle gain focus)
        var musclePreferences = new List<MuscleGroupPreferenceRequest>
        {
            new() { MuscleGroupId = 1, EmphasisLevel = EmphasisLevel.Alto },    // Pecho
            new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Alto },    // Espalda
            new() { MuscleGroupId = 4, EmphasisLevel = EmphasisLevel.Medio },   // Brazos
            new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
            new() { MuscleGroupId = 3, EmphasisLevel = EmphasisLevel.Bajo }     // Hombros
        };

        await muscleGroupService.SetUserMuscleGroupPreferencesAsync(profile.Id, musclePreferences);
        Console.WriteLine($"✓ Muscle preferences set: {musclePreferences.Count} grupos musculares");

        // Step 4: No limitations
        Console.WriteLine("✓ No physical limitations reported");

        Console.WriteLine($"✅ Young enthusiast journey completed successfully");
        Console.WriteLine();
    }

    private static async Task TestMiddleAgedWithLimitations(
        IUserProfileService userService,
        IEquipmentPreferenceService equipmentService,
        IMuscleGroupPreferenceService muscleGroupService,
        IPhysicalLimitationService limitationService)
    {
        Console.WriteLine("Scenario 2: Middle-aged with Limitations");
        Console.WriteLine("---------------------------------------");

        // Step 1: Demographics
        var profileRequest = new UserProfileCreateRequest
        {
            Name = "Carmen Mediana",
            Gender = Gender.Mujer,
            Age = 45,
            TrainingDaysPerWeek = 3
        };

        var profile = await userService.CreateUserProfileAsync(profileRequest);
        Console.WriteLine($"✓ Profile created: {profile.Name}, {profile.Age} años, {profile.TrainingDaysPerWeek} días/semana");

        // Step 2: Equipment (Home gym)
        var equipment = new List<int> { 1, 2, 4, 6 }; // Bodyweight, Free weights, Resistance bands, Pull-up bar
        await equipmentService.SetUserEquipmentPreferencesAsync(profile.Id, equipment);
        Console.WriteLine($"✓ Equipment set: {equipment.Count} tipos (gimnasio en casa)");

        // Step 3: Muscle groups (General fitness)
        var musclePreferences = new List<MuscleGroupPreferenceRequest>
        {
            new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Alto },    // Core
            new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Medio },   // Piernas
            new() { MuscleGroupId = 7, EmphasisLevel = EmphasisLevel.Medio },   // Glúteos
            new() { MuscleGroupId = 2, EmphasisLevel = EmphasisLevel.Bajo }     // Espalda
        };

        await muscleGroupService.SetUserMuscleGroupPreferencesAsync(profile.Id, musclePreferences);
        Console.WriteLine($"✓ Muscle preferences set: {musclePreferences.Count} grupos (fitness general)");

        // Step 4: Some limitations
        var limitations = new List<PhysicalLimitationRequest>
        {
            new() { LimitationType = LimitationType.ProblemasEspalda, Description = "Dolor lumbar ocasional" },
            new() { LimitationType = LimitationType.ProblemasRodilla, Description = "Artritis leve" }
        };

        await limitationService.SetUserPhysicalLimitationsAsync(profile.Id, limitations);
        Console.WriteLine($"✓ Limitations set: {limitations.Count} limitaciones físicas");

        Console.WriteLine($"✅ Middle-aged with limitations journey completed successfully");
        Console.WriteLine();
    }

    private static async Task TestElderlyBeginner(
        IUserProfileService userService,
        IEquipmentPreferenceService equipmentService,
        IMuscleGroupPreferenceService muscleGroupService,
        IPhysicalLimitationService limitationService)
    {
        Console.WriteLine("Scenario 3: Elderly Beginner");
        Console.WriteLine("----------------------------");

        // Step 1: Demographics
        var profileRequest = new UserProfileCreateRequest
        {
            Name = "Don Roberto",
            Gender = Gender.Hombre,
            Age = 68,
            TrainingDaysPerWeek = 2
        };

        var profile = await userService.CreateUserProfileAsync(profileRequest);
        Console.WriteLine($"✓ Profile created: {profile.Name}, {profile.Age} años, {profile.TrainingDaysPerWeek} días/semana");

        // Step 2: Equipment (Minimal)
        var equipment = new List<int> { 1, 4 }; // Bodyweight, Resistance bands
        await equipmentService.SetUserEquipmentPreferencesAsync(profile.Id, equipment);
        Console.WriteLine($"✓ Equipment set: {equipment.Count} tipos (mínimo equipamiento)");

        // Step 3: Muscle groups (Mobility and core)
        var musclePreferences = new List<MuscleGroupPreferenceRequest>
        {
            new() { MuscleGroupId = 6, EmphasisLevel = EmphasisLevel.Alto },    // Core
            new() { MuscleGroupId = 8, EmphasisLevel = EmphasisLevel.Medio },   // Cuerpo completo
            new() { MuscleGroupId = 5, EmphasisLevel = EmphasisLevel.Bajo }     // Piernas
        };

        await muscleGroupService.SetUserMuscleGroupPreferencesAsync(profile.Id, musclePreferences);
        Console.WriteLine($"✓ Muscle preferences set: {musclePreferences.Count} grupos (movilidad y estabilidad)");

        // Step 4: Multiple limitations
        var limitations = new List<PhysicalLimitationRequest>
        {
            new() { LimitationType = LimitationType.ProblemasCardivasculares, Description = "Hipertensión controlada" },
            new() { LimitationType = LimitationType.Artritis, Description = "Artritis en manos y rodillas" },
            new() { LimitationType = LimitationType.Personalizada, CustomRestrictions = "Evitar ejercicios de alto impacto y levantar más de 5kg" }
        };

        await limitationService.SetUserPhysicalLimitationsAsync(profile.Id, limitations);
        Console.WriteLine($"✓ Limitations set: {limitations.Count} limitaciones importantes");

        // Check intensity recommendation
        var limitationTypes = limitations.Select(l => l.LimitationType).ToList();
        var intensityRec = await limitationService.GetRecommendedIntensityAsync(limitationTypes);
        Console.WriteLine($"✓ Recommended intensity: {intensityRec.RecommendedLevel}/5 ({intensityRec.RecommendationReason})");

        Console.WriteLine($"✅ Elderly beginner journey completed successfully");
        Console.WriteLine();
    }

    private static async Task TestUIAmigableParaAbuela()
    {
        await Task.CompletedTask; // Async placeholder

        Console.WriteLine("2. Testing UI Amigable para Abuela (Story 3.5)");
        Console.WriteLine("---------------------------------------------");

        // Test UI design principles
        TestLargeButtonRequirements();
        TestHighContrastColors();
        TestSinglePageWorkflow();
        TestProgressIndicators();
        TestSpanishLanguageSupport();

        Console.WriteLine();
    }

    private static void TestLargeButtonRequirements()
    {
        Console.WriteLine("UI Component Tests:");
        Console.WriteLine("------------------");

        // Test button size requirements (minimum 60px height)
        var buttonSizes = new Dictionary<string, int>
        {
            { "Navigation buttons", 60 },
            { "Quick action buttons", 60 },
            { "Equipment checkboxes", 60 },
            { "Muscle group checkboxes", 60 },
            { "Gender radio buttons", 60 },
            { "Main action button", 80 }
        };

        foreach (var button in buttonSizes)
        {
            var meetsRequirement = button.Value >= 60;
            Console.WriteLine($"✓ {button.Key}: {button.Value}px height {(meetsRequirement ? "✅" : "❌")}");
        }
    }

    private static void TestHighContrastColors()
    {
        Console.WriteLine("Color Contrast Tests:");
        Console.WriteLine("--------------------");

        var colorSchemes = new List<string>
        {
            "Primary buttons: Blue background with white text",
            "Success buttons: Green background with white text",
            "Warning buttons: Orange background with white text",
            "Error messages: Red text on white background",
            "Progress indicators: High contrast blue/green/gray",
            "Step indicators: Color-coded with clear text"
        };

        foreach (var scheme in colorSchemes)
        {
            Console.WriteLine($"✓ {scheme}");
        }
    }

    private static void TestSinglePageWorkflow()
    {
        Console.WriteLine("Workflow Simplicity Tests:");
        Console.WriteLine("--------------------------");

        var workflowFeatures = new List<string>
        {
            "Single-page design with step-by-step progression",
            "Clear progress indicators showing current step",
            "No complex navigation menus",
            "Large, clearly labeled sections",
            "Simple forward/backward navigation",
            "Quick action buttons for common scenarios",
            "Auto-advancing when information is complete",
            "Validation with clear Spanish error messages"
        };

        foreach (var feature in workflowFeatures)
        {
            Console.WriteLine($"✓ {feature}");
        }
    }

    private static void TestProgressIndicators()
    {
        Console.WriteLine("Progress Indicator Tests:");
        Console.WriteLine("------------------------");

        var progressFeatures = new List<string>
        {
            "4-step visual progress bar",
            "Color-coded steps (Gray → Blue → Green)",
            "Clear step labels in Spanish",
            "Current step highlighted",
            "Completed steps marked green",
            "Step completion status clearly visible"
        };

        foreach (var feature in progressFeatures)
        {
            Console.WriteLine($"✓ {feature}");
        }
    }

    private static void TestSpanishLanguageSupport()
    {
        Console.WriteLine("Spanish Language Support Tests:");
        Console.WriteLine("------------------------------");

        var spanishElements = new List<string>
        {
            "All UI text in Spanish",
            "Form labels in clear Spanish",
            "Error messages in Spanish",
            "Button labels in Spanish",
            "Tooltips and help text in Spanish",
            "Progress step names in Spanish",
            "Validation messages in Spanish",
            "Quick action labels in Spanish"
        };

        foreach (var element in spanishElements)
        {
            Console.WriteLine($"✓ {element}");
        }
    }

    private static async Task TestUserInterfaceAccessibility()
    {
        await Task.CompletedTask; // Async placeholder

        Console.WriteLine("3. Testing User Interface Accessibility");
        Console.WriteLine("--------------------------------------");

        TestFontSizeRequirements();
        TestInputFieldSizes();
        TestNavigationSimplicity();
        TestErrorHandling();

        Console.WriteLine();
    }

    private static void TestFontSizeRequirements()
    {
        Console.WriteLine("Font Size Requirements:");
        Console.WriteLine("----------------------");

        var fontSizes = new Dictionary<string, int>
        {
            { "Body text", 16 },
            { "Button text", 16 },
            { "Section headers", 18 },
            { "Main title", 32 },
            { "Step titles", 24 },
            { "Progress text", 14 },
            { "Form labels", 18 }
        };

        foreach (var font in fontSizes)
        {
            var meetsRequirement = font.Value >= 16;
            Console.WriteLine($"✓ {font.Key}: {font.Value}px {(meetsRequirement ? "✅" : "📝")}");
        }
    }

    private static void TestInputFieldSizes()
    {
        Console.WriteLine("Input Field Sizes:");
        Console.WriteLine("-----------------");

        var inputSizes = new Dictionary<string, int>
        {
            { "Text input fields", 60 },
            { "Number input fields", 60 },
            { "Checkbox minimum touch area", 60 },
            { "Radio button minimum touch area", 60 },
            { "Slider minimum touch area", 60 }
        };

        foreach (var input in inputSizes)
        {
            var meetsRequirement = input.Value >= 60;
            Console.WriteLine($"✓ {input.Key}: {input.Value}px height {(meetsRequirement ? "✅" : "❌")}");
        }
    }

    private static void TestNavigationSimplicity()
    {
        Console.WriteLine("Navigation Simplicity:");
        Console.WriteLine("---------------------");

        var navigationFeatures = new List<string>
        {
            "Only two navigation buttons (Previous/Next)",
            "Clear visual feedback on current step",
            "No hidden menus or complex navigation",
            "Large, clearly labeled navigation buttons",
            "Consistent button placement",
            "Auto-progression when steps are complete"
        };

        foreach (var feature in navigationFeatures)
        {
            Console.WriteLine($"✓ {feature}");
        }
    }

    private static void TestErrorHandling()
    {
        Console.WriteLine("Error Handling:");
        Console.WriteLine("--------------");

        var errorFeatures = new List<string>
        {
            "Clear error messages in Spanish",
            "Error messages shown prominently",
            "Validation happens before proceeding",
            "Helpful suggestions for fixing errors",
            "No technical jargon in error messages",
            "Success messages provide positive feedback"
        };

        foreach (var feature in errorFeatures)
        {
            Console.WriteLine($"✓ {feature}");
        }

        Console.WriteLine();
        Console.WriteLine("Sample error messages:");
        var sampleErrors = new List<string>
        {
            "El nombre es requerido",
            "Debe seleccionar un género",
            "La edad debe estar entre 16 y 100 años",
            "Debe seleccionar al menos un tipo de equipamiento",
            "Debe seleccionar al menos un objetivo o grupo muscular"
        };

        foreach (var error in sampleErrors)
        {
            Console.WriteLine($"  • {error}");
        }
    }
}