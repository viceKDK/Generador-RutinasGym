using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Infrastructure.AI.Models;

namespace GymRoutineGenerator.Infrastructure.AI;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl = "http://localhost:11434";
    private readonly JsonSerializerOptions _jsonOptions;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Extended timeout for AI generation

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    public async Task<string> GenerateRoutineAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Enhance prompt with fitness context and Spanish instructions
            var enhancedPrompt = BuildFitnessPrompt(prompt);

            var request = new OllamaRequest
            {
                Model = "mistral:7b",
                Prompt = enhancedPrompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.7,
                    Top_p = 1,
                    Max_tokens = 2000,
                    Stop = new[] { "[FIN]", "[END]" }
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ollama API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, _jsonOptions);

            if (ollamaResponse?.Response == null)
            {
                throw new InvalidOperationException("Respuesta vacía de Ollama");
            }

            return CleanRoutineResponse(ollamaResponse.Response);
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("La generación de rutina tomó demasiado tiempo");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error de conexión con Ollama: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generando rutina: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await GetHealthStatusAsync(cancellationToken);
            return healthStatus.IsHealthy && healthStatus.AvailableModels.Contains("mistral:7b");
        }
        catch
        {
            return false;
        }
    }

    public async Task<OllamaHealth> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var health = new OllamaHealth();

        try
        {
            // Check if Ollama is running
            var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/version", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var versionContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var versionData = JsonSerializer.Deserialize<JsonElement>(versionContent);

                health.IsHealthy = true;
                health.Status = "Conectado";
                health.Version = versionData.TryGetProperty("version", out var version) ? version.GetString() ?? "unknown" : "unknown";

                // Get available models
                health.AvailableModels = await GetAvailableModelsAsync(cancellationToken);
            }
            else
            {
                health.IsHealthy = false;
                health.Status = $"Error de conexión: {response.StatusCode}";
            }
        }
        catch (TaskCanceledException)
        {
            health.IsHealthy = false;
            health.Status = "Timeout de conexión";
        }
        catch (HttpRequestException)
        {
            health.IsHealthy = false;
            health.Status = "Ollama no está ejecutándose";
        }
        catch (Exception ex)
        {
            health.IsHealthy = false;
            health.Status = $"Error: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            health.ResponseTime = stopwatch.Elapsed;
        }

        return health;
    }

    public async Task<List<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var modelsData = JsonSerializer.Deserialize<JsonElement>(content);

            var models = new List<string>();

            if (modelsData.TryGetProperty("models", out var modelsArray))
            {
                foreach (var model in modelsArray.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var nameProperty))
                    {
                        var modelName = nameProperty.GetString();
                        if (!string.IsNullOrEmpty(modelName))
                        {
                            models.Add(modelName);
                        }
                    }
                }
            }

            return models;
        }
        catch
        {
            return new List<string>();
        }
    }

    private string BuildFitnessPrompt(string userPrompt)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("Eres un entrenador personal experto en fitness y nutrición. Tu especialidad es crear rutinas de ejercicio personalizadas y seguras.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("INSTRUCCIONES IMPORTANTES:");
        promptBuilder.AppendLine("- Responde SOLO en español");
        promptBuilder.AppendLine("- Crea rutinas seguras y progresivas");
        promptBuilder.AppendLine("- Incluye calentamiento y enfriamiento");
        promptBuilder.AppendLine("- Especifica series, repeticiones y descansos");
        promptBuilder.AppendLine("- Considera limitaciones físicas mencionadas");
        promptBuilder.AppendLine("- Usa equipamiento disponible mencionado");
        promptBuilder.AppendLine("- Enfócate en grupos musculares prioritarios");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("FORMATO DE RESPUESTA:");
        promptBuilder.AppendLine("📋 RUTINA DE ENTRENAMIENTO");
        promptBuilder.AppendLine("🎯 Objetivo: [objetivo principal]");
        promptBuilder.AppendLine("⏱️ Duración: [tiempo estimado]");
        promptBuilder.AppendLine("🔥 CALENTAMIENTO (5-10 min)");
        promptBuilder.AppendLine("[ejercicios de calentamiento]");
        promptBuilder.AppendLine("💪 EJERCICIOS PRINCIPALES");
        promptBuilder.AppendLine("[ejercicios con series, reps, descanso]");
        promptBuilder.AppendLine("🧘 ENFRIAMIENTO (5-10 min)");
        promptBuilder.AppendLine("[estiramientos y relajación]");
        promptBuilder.AppendLine("⚠️ CONSEJOS DE SEGURIDAD");
        promptBuilder.AppendLine("[recomendaciones importantes]");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("SOLICITUD DEL USUARIO:");
        promptBuilder.AppendLine(userPrompt);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Genera una rutina detallada siguiendo el formato especificado:");

        return promptBuilder.ToString();
    }

    private string CleanRoutineResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return "❌ No se pudo generar una rutina válida";
        }

        // Remove common AI artifacts
        var cleaned = response
            .Replace("[FIN]", "")
            .Replace("[END]", "")
            .Trim();

        // Ensure minimum content
        if (cleaned.Length < 100)
        {
            return "❌ La rutina generada es demasiado corta. Intenta ser más específico en tu solicitud.";
        }

        return cleaned;
    }
}