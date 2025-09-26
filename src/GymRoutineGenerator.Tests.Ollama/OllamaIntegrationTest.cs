using GymRoutineGenerator.Infrastructure.AI;
using GymRoutineGenerator.Infrastructure.AI.Models;

namespace GymRoutineGenerator.Tests.Ollama;

public static class OllamaIntegrationTest
{
    public static async Task RunOllamaIntegrationTests()
    {
        Console.WriteLine("=== TESTING OLLAMA INTEGRATION (STORY 4.1) ===");
        Console.WriteLine();

        using var httpClient = new HttpClient();
        var ollamaService = new OllamaService(httpClient);

        await TestOllamaHealthStatus(ollamaService);
        await TestAvailableModels(ollamaService);
        await TestBasicRoutineGeneration(ollamaService);
        await TestSpanishPromptEngineering(ollamaService);
        await TestErrorHandling(ollamaService);

        Console.WriteLine();
        Console.WriteLine("=== OLLAMA INTEGRATION TESTS COMPLETED ===");
    }

    private static async Task TestOllamaHealthStatus(IOllamaService ollamaService)
    {
        Console.WriteLine("1. Testing Ollama Health Status");
        Console.WriteLine("-------------------------------");

        try
        {
            var health = await ollamaService.GetHealthStatusAsync();

            Console.WriteLine($"✓ Health Status Retrieved:");
            Console.WriteLine($"  - Is Healthy: {health.IsHealthy}");
            Console.WriteLine($"  - Status: {health.Status}");
            Console.WriteLine($"  - Version: {health.Version}");
            Console.WriteLine($"  - Response Time: {health.ResponseTime.TotalMilliseconds:F0}ms");
            Console.WriteLine($"  - Available Models: {health.AvailableModels.Count}");

            if (health.AvailableModels.Any())
            {
                Console.WriteLine("  - Models:");
                foreach (var model in health.AvailableModels)
                {
                    Console.WriteLine($"    • {model}");
                }
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Health Status Error: {ex.Message}");
            Console.WriteLine();
        }
    }

    private static async Task TestAvailableModels(IOllamaService ollamaService)
    {
        Console.WriteLine("2. Testing Available Models");
        Console.WriteLine("---------------------------");

        try
        {
            var models = await ollamaService.GetAvailableModelsAsync();

            Console.WriteLine($"✓ Models Retrieved: {models.Count}");
            foreach (var model in models)
            {
                Console.WriteLine($"  • {model}");
            }

            var hasMistral = models.Any(m => m.Contains("mistral"));
            Console.WriteLine($"✓ Mistral Available: {(hasMistral ? "Sí" : "No")}");

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Models Error: {ex.Message}");
            Console.WriteLine();
        }
    }

    private static async Task TestBasicRoutineGeneration(IOllamaService ollamaService)
    {
        Console.WriteLine("3. Testing Basic Routine Generation");
        Console.WriteLine("----------------------------------");

        try
        {
            var isAvailable = await ollamaService.IsAvailableAsync();
            Console.WriteLine($"✓ Service Available: {isAvailable}");

            if (!isAvailable)
            {
                Console.WriteLine("⚠️ Ollama not available - skipping generation tests");
                Console.WriteLine("   Make sure Ollama is running and Mistral model is installed:");
                Console.WriteLine("   1. Install Ollama: winget install Ollama.Ollama");
                Console.WriteLine("   2. Start Ollama: ollama serve");
                Console.WriteLine("   3. Download model: ollama pull mistral:7b");
                Console.WriteLine();
                return;
            }

            var basicPrompt = "Crea una rutina básica de 30 minutos para principiante con ejercicios corporales";

            Console.WriteLine($"✓ Generating routine with prompt: '{basicPrompt}'");
            Console.WriteLine("✓ Please wait, this may take 30-60 seconds...");

            var routine = await ollamaService.GenerateRoutineAsync(basicPrompt);

            Console.WriteLine($"✓ Routine Generated ({routine.Length} characters):");
            Console.WriteLine("  " + routine.Replace("\n", "\n  "));

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Generation Error: {ex.Message}");
            Console.WriteLine();
        }
    }

    private static async Task TestSpanishPromptEngineering(IOllamaService ollamaService)
    {
        Console.WriteLine("4. Testing Spanish Prompt Engineering");
        Console.WriteLine("------------------------------------");

        try
        {
            var isAvailable = await ollamaService.IsAvailableAsync();
            if (!isAvailable)
            {
                Console.WriteLine("⚠️ Ollama not available - skipping prompt engineering tests");
                Console.WriteLine();
                return;
            }

            var scenarios = new List<(string description, string prompt)>
            {
                ("Elderly with limitations", "Soy una mujer de 65 años con artritis en las rodillas. Necesito ejercicios suaves para mantenerme activa."),
                ("Young bodybuilder", "Soy un hombre de 25 años, quiero ganar masa muscular. Tengo acceso a gimnasio completo."),
                ("Home workout", "Trabajo desde casa y solo tengo 20 minutos. Necesito ejercicios sin equipamiento.")
            };

            foreach (var (description, prompt) in scenarios)
            {
                Console.WriteLine($"Testing: {description}");
                Console.WriteLine($"Prompt: {prompt}");

                try
                {
                    var routine = await ollamaService.GenerateRoutineAsync(prompt);
                    var isSpanish = routine.Contains("ejercicio") || routine.Contains("rutina") || routine.Contains("entrenamiento");
                    var hasStructure = routine.Contains("RUTINA") || routine.Contains("CALENTAMIENTO") || routine.Contains("📋");

                    Console.WriteLine($"  ✓ Generated: {routine.Length} characters");
                    Console.WriteLine($"  ✓ Spanish content: {isSpanish}");
                    Console.WriteLine($"  ✓ Structured format: {hasStructure}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Error: {ex.Message}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Prompt Engineering Error: {ex.Message}");
            Console.WriteLine();
        }
    }

    private static async Task TestErrorHandling(IOllamaService ollamaService)
    {
        Console.WriteLine("5. Testing Error Handling");
        Console.WriteLine("-------------------------");

        try
        {
            // Test with empty prompt
            Console.WriteLine("Testing empty prompt...");
            try
            {
                var result = await ollamaService.GenerateRoutineAsync("");
                Console.WriteLine($"✓ Empty prompt handled: {result.Substring(0, Math.Min(50, result.Length))}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Empty prompt error handled: {ex.Message}");
            }

            // Test with very long prompt
            Console.WriteLine("Testing very long prompt...");
            try
            {
                var longPrompt = string.Join(" ", Enumerable.Repeat("ejercicio", 500));
                var result = await ollamaService.GenerateRoutineAsync(longPrompt);
                Console.WriteLine($"✓ Long prompt handled: {result.Length} characters");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Long prompt error handled: {ex.Message}");
            }

            // Test timeout behavior
            Console.WriteLine("Testing timeout handling...");
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                var result = await ollamaService.GenerateRoutineAsync("test", cts.Token);
                Console.WriteLine($"✓ Quick response: {result.Length} characters");
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"✓ Timeout properly handled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Cancellation handled: {ex.GetType().Name}");
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error Handling Test Failed: {ex.Message}");
            Console.WriteLine();
        }
    }
}