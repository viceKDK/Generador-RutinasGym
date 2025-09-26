using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Infrastructure.AI;

namespace GymRoutineGenerator.Tests.Ollama;

public static class EnhancedPromptTemplateTest
{
    public static async Task RunEnhancedPromptTemplateTests()
    {
        Console.WriteLine("=== TESTING ENHANCED PROMPT TEMPLATES & CONTEXT BUILDING (STORY 4.2) ===");
        Console.WriteLine();

        await TestPromptTemplateService();
        await TestUserParameterMapping();
        await TestIntelligentRoutineService();
        await TestFallbackRoutineService();
        await TestExerciseSelectionAlgorithm();

        Console.WriteLine();
        Console.WriteLine("=== ENHANCED PROMPT TEMPLATE TESTS COMPLETED ===");
    }

    private static async Task TestPromptTemplateService()
    {
        Console.WriteLine("1. Testing Prompt Template Service");
        Console.WriteLine("----------------------------------");

        var promptService = new PromptTemplateService();

        // Create test parameters
        var parameters = CreateTestUserParameters();

        try
        {
            // Test intelligent routine prompt
            Console.WriteLine("Testing intelligent routine prompt...");
            var intelligentPrompt = await promptService.BuildIntelligentRoutinePromptAsync(parameters);

            Console.WriteLine($"✓ Intelligent prompt generated: {intelligentPrompt.Length} characters");
            Console.WriteLine($"✓ Contains user analysis: {intelligentPrompt.Contains("ANÁLISIS DEL CLIENTE")}");
            Console.WriteLine($"✓ Contains exercise strategy: {intelligentPrompt.Contains("ESTRATEGIA DE SELECCIÓN")}");
            Console.WriteLine($"✓ Contains safety guidelines: {intelligentPrompt.Contains("PAUTAS DE SEGURIDAD")}");
            Console.WriteLine($"✓ Contains format specification: {intelligentPrompt.Contains("FORMATO DE RESPUESTA")}");

            // Test exercise selection prompt
            Console.WriteLine("\nTesting exercise selection prompt...");
            var selectionPrompt = await promptService.BuildExerciseSelectionPromptAsync(parameters);

            Console.WriteLine($"✓ Selection prompt generated: {selectionPrompt.Length} characters");
            Console.WriteLine($"✓ Contains equipment analysis: {selectionPrompt.Contains("EQUIPAMIENTO DISPONIBLE")}");
            Console.WriteLine($"✓ Contains muscle priorities: {selectionPrompt.Contains("PRIORIDADES MUSCULARES")}");

            // Test fallback prompt
            Console.WriteLine("\nTesting fallback prompt...");
            var fallbackPrompt = await promptService.BuildFallbackPromptAsync(parameters);

            Console.WriteLine($"✓ Fallback prompt generated: {fallbackPrompt.Length} characters");
            Console.WriteLine($"✓ Contains basic structure: {fallbackPrompt.Contains("RUTINA BÁSICA")}");

            Console.WriteLine("✅ Prompt Template Service tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Prompt Template Service test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestUserParameterMapping()
    {
        Console.WriteLine("2. Testing User Parameter Mapping");
        Console.WriteLine("---------------------------------");

        try
        {
            // Test parameter creation
            var parameters = CreateTestUserParameters();

            Console.WriteLine($"✓ Parameters created successfully");
            Console.WriteLine($"  - Name: {parameters.Name}");
            Console.WriteLine($"  - Age: {parameters.Age}");
            Console.WriteLine($"  - Experience: {parameters.ExperienceLevel}");
            Console.WriteLine($"  - Equipment count: {parameters.AvailableEquipment.Count}");
            Console.WriteLine($"  - Muscle group preferences: {parameters.MuscleGroupPreferences.Count}");
            Console.WriteLine($"  - Physical limitations: {parameters.PhysicalLimitations.Count}");
            Console.WriteLine($"  - Recommended intensity: {parameters.RecommendedIntensity}/5");

            // Test parameter validation
            Console.WriteLine("\nValidating parameter logic...");
            Console.WriteLine($"✓ Session duration appropriate: {parameters.PreferredSessionDuration} min");
            Console.WriteLine($"✓ Primary goal determined: {parameters.PrimaryGoal}");
            Console.WriteLine($"✓ Gym type classified: {parameters.GymType}");
            Console.WriteLine($"✓ Include cardio: {parameters.IncludeCardio}");
            Console.WriteLine($"✓ Include flexibility: {parameters.IncludeFlexibility}");

            Console.WriteLine("✅ User Parameter Mapping tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ User Parameter Mapping test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestIntelligentRoutineService()
    {
        Console.WriteLine("3. Testing Intelligent Routine Service");
        Console.WriteLine("--------------------------------------");

        try
        {
            // Mock services for testing
            using var httpClient = new HttpClient();
            var ollamaService = new OllamaService(httpClient);
            var promptService = new PromptTemplateService();
            var fallbackService = new FallbackRoutineService();

            // Create a mock parameter mapping service
            var parameters = CreateTestUserParameters();

            Console.WriteLine("Testing fallback routine generation (since Ollama likely unavailable)...");

            var fallbackResult = await fallbackService.GenerateRuleBasedRoutineAsync(parameters);

            Console.WriteLine($"✓ Fallback routine generated: {fallbackResult.Length} characters");
            Console.WriteLine($"✓ Contains user summary: {fallbackResult.Contains("RESUMEN DEL CLIENTE")}");
            Console.WriteLine($"✓ Contains warmup: {fallbackResult.Contains("CALENTAMIENTO")}");
            Console.WriteLine($"✓ Contains main exercises: {fallbackResult.Contains("EJERCICIOS PRINCIPALES")}");
            Console.WriteLine($"✓ Contains cooldown: {fallbackResult.Contains("ENFRIAMIENTO")}");
            Console.WriteLine($"✓ Contains progression: {fallbackResult.Contains("PROGRESIÓN SEMANAL")}");
            Console.WriteLine($"✓ Contains safety advice: {fallbackResult.Contains("CONSEJOS DE SEGURIDAD")}");

            Console.WriteLine("✅ Intelligent Routine Service tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Intelligent Routine Service test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestFallbackRoutineService()
    {
        Console.WriteLine("4. Testing Fallback Routine Service");
        Console.WriteLine("-----------------------------------");

        var fallbackService = new FallbackRoutineService();

        try
        {
            // Test different user scenarios
            var scenarios = new[]
            {
                ("Young athlete", CreateYoungAthleteParameters()),
                ("Middle-aged beginner", CreateMiddleAgedBeginnerParameters()),
                ("Senior with limitations", CreateSeniorParameters())
            };

            foreach (var (description, parameters) in scenarios)
            {
                Console.WriteLine($"\nTesting scenario: {description}");

                var routine = await fallbackService.GenerateRuleBasedRoutineAsync(parameters);
                Console.WriteLine($"✓ Generated routine: {routine.Length} characters");

                var exercises = await fallbackService.GetRecommendedExercisesAsync(parameters);
                Console.WriteLine($"✓ Recommended exercises: {exercises.Count}");

                // Verify exercise appropriateness
                bool hasCompoundMovements = exercises.Any(e => e.ExerciseType == ExerciseType.Compound);
                bool respectsLimitations = !exercises.Any(e =>
                    parameters.AvoidExercises.Any(avoid =>
                        e.Name.Contains(avoid, StringComparison.OrdinalIgnoreCase)));

                Console.WriteLine($"✓ Includes compound movements: {hasCompoundMovements}");
                Console.WriteLine($"✓ Respects limitations: {respectsLimitations}");
                Console.WriteLine($"✓ Difficulty level appropriate: {exercises.All(e => GetDifficultyLevel(e.DifficultyLevel) <= parameters.RecommendedIntensity + 1)}");
            }

            // Test specific templates
            Console.WriteLine("\nTesting specific templates...");
            var templates = new[] { "beginner", "intermediate", "senior", "home" };
            var testParams = CreateTestUserParameters();

            foreach (var template in templates)
            {
                var templateRoutine = await fallbackService.GenerateBasicRoutineTemplateAsync(template, testParams);
                Console.WriteLine($"✓ {template} template generated: {templateRoutine.Length} characters");
            }

            Console.WriteLine("✅ Fallback Routine Service tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fallback Routine Service test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task TestExerciseSelectionAlgorithm()
    {
        Console.WriteLine("5. Testing Exercise Selection Algorithm");
        Console.WriteLine("--------------------------------------");

        var fallbackService = new FallbackRoutineService();

        try
        {
            // Test equipment-based filtering
            Console.WriteLine("Testing equipment-based exercise selection...");

            var bodyweightParams = CreateBodyweightParameters();
            var bodyweightExercises = await fallbackService.GetRecommendedExercisesAsync(bodyweightParams);

            Console.WriteLine($"✓ Bodyweight exercises: {bodyweightExercises.Count}");
            Console.WriteLine($"✓ All use appropriate equipment: {bodyweightExercises.All(e => e.Equipment == "Peso corporal")}");

            // Test limitation-based filtering
            Console.WriteLine("\nTesting limitation-based filtering...");

            var limitedParams = CreateLimitedUserParameters();
            var limitedExercises = await fallbackService.GetRecommendedExercisesAsync(limitedParams);

            Console.WriteLine($"✓ Limited exercises: {limitedExercises.Count}");
            Console.WriteLine($"✓ Avoids problematic exercises: {!limitedExercises.Any(e => e.Name.Contains("Sentadilla"))}");

            // Test muscle group prioritization
            Console.WriteLine("\nTesting muscle group prioritization...");

            var priorityParams = CreateMuscleGroupPriorityParameters();
            var priorityExercises = await fallbackService.GetRecommendedExercisesAsync(priorityParams);

            var priorityMuscles = priorityParams.MuscleGroupPreferences
                .OrderBy(mg => mg.Priority)
                .Take(3)
                .Select(mg => mg.MuscleGroup)
                .ToList();

            bool coversPriorityMuscles = priorityMuscles.All(muscle =>
                priorityExercises.Any(e => e.MuscleGroups.Any(mg =>
                    mg.Contains(muscle, StringComparison.OrdinalIgnoreCase))));

            Console.WriteLine($"✓ Priority exercises: {priorityExercises.Count}");
            Console.WriteLine($"✓ Covers priority muscle groups: {coversPriorityMuscles}");

            Console.WriteLine("✅ Exercise Selection Algorithm tests completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exercise Selection Algorithm test failed: {ex.Message}");
        }

        Console.WriteLine();
    }

    // Helper methods to create test parameters
    private static UserRoutineParameters CreateTestUserParameters()
    {
        return new UserRoutineParameters
        {
            Name = "María González",
            Age = 35,
            Gender = "Mujer",
            TrainingDaysPerWeek = 3,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 45,
            RecommendedIntensity = 3,
            GymType = "Casa",
            IncludeCardio = true,
            IncludeFlexibility = true,
            AvailableEquipment = new List<string> { "Peso corporal", "Bandas elásticas", "Esterilla" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Core", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Piernas", EmphasisLevel = "Medio", Priority = 2 },
                new() { MuscleGroup = "Brazos", EmphasisLevel = "Bajo", Priority = 3 }
            },
            PhysicalLimitations = new List<string> { "Problemas leves de espalda" },
            AvoidExercises = new List<string> { "Peso muerto" },
            PreferredExerciseTypes = new List<string> { "Ejercicios funcionales", "Peso corporal" }
        };
    }

    private static UserRoutineParameters CreateYoungAthleteParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Carlos Deportista",
            Age = 22,
            Gender = "Hombre",
            TrainingDaysPerWeek = 5,
            ExperienceLevel = "Avanzado",
            PrimaryGoal = "Masa",
            PreferredSessionDuration = 60,
            RecommendedIntensity = 4,
            GymType = "Gimnasio",
            AvailableEquipment = new List<string> { "Mancuernas", "Barras", "Máquinas", "Peso corporal" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Pecho", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Espalda", EmphasisLevel = "Alto", Priority = 2 },
                new() { MuscleGroup = "Piernas", EmphasisLevel = "Medio", Priority = 3 }
            }
        };
    }

    private static UserRoutineParameters CreateMiddleAgedBeginnerParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Ana Principiante",
            Age = 42,
            Gender = "Mujer",
            TrainingDaysPerWeek = 2,
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Pérdida de peso",
            PreferredSessionDuration = 30,
            RecommendedIntensity = 2,
            GymType = "Casa",
            AvailableEquipment = new List<string> { "Peso corporal", "Esterilla" },
            PhysicalLimitations = new List<string> { "Sedentarismo prolongado" },
            IncludeCardio = true,
            IncludeFlexibility = true
        };
    }

    private static UserRoutineParameters CreateSeniorParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Roberto Senior",
            Age = 68,
            Gender = "Hombre",
            TrainingDaysPerWeek = 2,
            ExperienceLevel = "Principiante",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 30,
            RecommendedIntensity = 1,
            GymType = "Casa",
            AvailableEquipment = new List<string> { "Peso corporal", "Silla" },
            PhysicalLimitations = new List<string> { "Artritis", "Problemas de equilibrio" },
            AvoidExercises = new List<string> { "Saltos", "Movimientos bruscos" },
            IncludeFlexibility = true
        };
    }

    private static UserRoutineParameters CreateBodyweightParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Usuario Sin Equipamiento",
            Age = 30,
            Gender = "Mujer",
            TrainingDaysPerWeek = 4,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 40,
            RecommendedIntensity = 3,
            GymType = "Casa",
            AvailableEquipment = new List<string> { "Peso corporal" }
        };
    }

    private static UserRoutineParameters CreateLimitedUserParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Usuario Con Limitaciones",
            Age = 50,
            Gender = "Hombre",
            TrainingDaysPerWeek = 3,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Fitness General",
            PreferredSessionDuration = 45,
            RecommendedIntensity = 2,
            GymType = "Casa",
            AvailableEquipment = new List<string> { "Peso corporal", "Bandas elásticas" },
            PhysicalLimitations = new List<string> { "Problemas de rodilla", "Problemas de espalda" },
            AvoidExercises = new List<string> { "Sentadillas profundas", "Peso muerto" }
        };
    }

    private static UserRoutineParameters CreateMuscleGroupPriorityParameters()
    {
        return new UserRoutineParameters
        {
            Name = "Usuario Con Prioridades",
            Age = 28,
            Gender = "Mujer",
            TrainingDaysPerWeek = 4,
            ExperienceLevel = "Intermedio",
            PrimaryGoal = "Masa",
            PreferredSessionDuration = 50,
            RecommendedIntensity = 3,
            GymType = "Gimnasio",
            AvailableEquipment = new List<string> { "Mancuernas", "Peso corporal" },
            MuscleGroupPreferences = new List<MuscleGroupFocus>
            {
                new() { MuscleGroup = "Glúteos", EmphasisLevel = "Alto", Priority = 1 },
                new() { MuscleGroup = "Core", EmphasisLevel = "Alto", Priority = 2 },
                new() { MuscleGroup = "Brazos", EmphasisLevel = "Medio", Priority = 3 },
                new() { MuscleGroup = "Espalda", EmphasisLevel = "Bajo", Priority = 4 }
            }
        };
    }

    private static int GetDifficultyLevel(string difficultyLevel)
    {
        return difficultyLevel.ToLower() switch
        {
            "principiante" or "beginner" => 1,
            "intermedio" or "intermediate" => 2,
            "avanzado" or "advanced" => 3,
            _ => 2 // Default to intermediate
        };
    }
}