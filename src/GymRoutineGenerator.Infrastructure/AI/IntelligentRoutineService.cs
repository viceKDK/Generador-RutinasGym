using System.Diagnostics;
using GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Infrastructure.AI;

public class IntelligentRoutineService : IIntelligentRoutineService
{
    private readonly IOllamaService _ollamaService;
    private readonly IPromptTemplateService _promptTemplateService;
    private readonly IFallbackRoutineService _fallbackRoutineService;

    public IntelligentRoutineService(
        IOllamaService ollamaService,
        IPromptTemplateService promptTemplateService,
        IFallbackRoutineService fallbackRoutineService)
    {
        _ollamaService = ollamaService;
        _promptTemplateService = promptTemplateService;
        _fallbackRoutineService = fallbackRoutineService;
    }

    public async Task<RoutineGenerationResult> GenerateIntelligentRoutineAsync(int userProfileId, CancellationToken cancellationToken = default)
    {
        // Simulate async operation with a small delay
        await Task.Delay(1, cancellationToken);

        // For now, return a message indicating that parameter mapping service is needed
        return new RoutineGenerationResult
        {
            IsSuccess = false,
            ErrorMessage = "Parameter mapping service not available in this context. Use GenerateIntelligentRoutineAsync(UserRoutineParameters) instead.",
            Source = GenerationSource.Error_Occurred
        };
    }

    public async Task<RoutineGenerationResult> GenerateIntelligentRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new RoutineGenerationResult();

        try
        {
            // First, check if AI service is available
            var isAiAvailable = await _ollamaService.IsAvailableAsync(cancellationToken);

            if (isAiAvailable)
            {
                result = await GenerateWithAIAsync(parameters, cancellationToken);
            }
            else
            {
                result = await GenerateWithFallbackAsync(parameters, cancellationToken);
                result.Warnings.Add("IA no disponible - se usó algoritmo de respaldo");
            }
        }
        catch (TimeoutException)
        {
            result = await GenerateWithFallbackAsync(parameters, cancellationToken);
            result.Warnings.Add("Timeout de IA - se usó algoritmo de respaldo");
        }
        catch (Exception ex)
        {
            result = await GenerateWithFallbackAsync(parameters, cancellationToken);
            result.Warnings.Add($"Error de IA - se usó algoritmo de respaldo: {ex.Message}");
        }

        stopwatch.Stop();
        result.GenerationTime = stopwatch.Elapsed;

        // Add metadata
        result.Metadata = BuildRoutineMetadata(parameters, result);

        return result;
    }

    public async Task<RoutineGenerationResult> GenerateAlternativeRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        // Modify parameters slightly to generate variation
        var alternativeParameters = CreateAlternativeParameters(parameters);

        var result = await GenerateIntelligentRoutineAsync(alternativeParameters, cancellationToken);

        if (result.Metadata != null)
        {
            result.Metadata.PromptUsed += " [VARIACIÓN]";
        }

        return result;
    }

    public async Task<List<ExerciseAlternative>> GetExerciseAlternativesAsync(string exerciseName, UserRoutineParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAiAvailable = await _ollamaService.IsAvailableAsync(cancellationToken);

            if (isAiAvailable)
            {
                return await GetAIExerciseAlternativesAsync(exerciseName, parameters, cancellationToken);
            }
            else
            {
                return GetRuleBasedExerciseAlternatives(exerciseName, parameters);
            }
        }
        catch
        {
            return GetRuleBasedExerciseAlternatives(exerciseName, parameters);
        }
    }

    private async Task<RoutineGenerationResult> GenerateWithAIAsync(UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        var prompt = await _promptTemplateService.BuildIntelligentRoutinePromptAsync(parameters, cancellationToken);

        var aiResponse = await _ollamaService.GenerateRoutineAsync(prompt, cancellationToken);

        // Validate AI response
        var validationResult = ValidateAIResponse(aiResponse, parameters);

        return new RoutineGenerationResult
        {
            IsSuccess = validationResult.IsValid,
            GeneratedRoutine = validationResult.IsValid ? aiResponse : validationResult.CorrectedResponse,
            Source = GenerationSource.AI_Generated,
            Warnings = validationResult.Warnings
        };
    }

    private async Task<RoutineGenerationResult> GenerateWithFallbackAsync(UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        var fallbackRoutine = await _fallbackRoutineService.GenerateRuleBasedRoutineAsync(parameters, cancellationToken);

        return new RoutineGenerationResult
        {
            IsSuccess = true,
            GeneratedRoutine = fallbackRoutine,
            Source = GenerationSource.Fallback_Rules
        };
    }

    private async Task<List<ExerciseAlternative>> GetAIExerciseAlternativesAsync(string exerciseName, UserRoutineParameters parameters, CancellationToken cancellationToken)
    {
        var prompt = BuildExerciseAlternativePrompt(exerciseName, parameters);
        var aiResponse = await _ollamaService.GenerateRoutineAsync(prompt, cancellationToken);

        return ParseExerciseAlternatives(aiResponse);
    }

    private List<ExerciseAlternative> GetRuleBasedExerciseAlternatives(string exerciseName, UserRoutineParameters parameters)
    {
        // Simple rule-based alternatives based on exercise patterns
        var alternatives = new List<ExerciseAlternative>();

        // Basic pattern matching for common exercises
        var exerciseLower = exerciseName.ToLower();

        if (exerciseLower.Contains("sentadilla"))
        {
            alternatives.AddRange(new[]
            {
                new ExerciseAlternative { ExerciseName = "Sentadilla con peso corporal", Description = "Versión sin peso", EquipmentRequired = "Ninguno", DifficultyLevel = 2, Reason = "Menor impacto en rodillas" },
                new ExerciseAlternative { ExerciseName = "Sentadilla en silla", Description = "Apoyo con silla", EquipmentRequired = "Silla", DifficultyLevel = 1, Reason = "Mayor estabilidad" }
            });
        }
        else if (exerciseLower.Contains("press") && exerciseLower.Contains("pecho"))
        {
            alternatives.AddRange(new[]
            {
                new ExerciseAlternative { ExerciseName = "Flexiones de pecho", Description = "Versión con peso corporal", EquipmentRequired = "Ninguno", DifficultyLevel = 3, Reason = "Sin equipamiento necesario" },
                new ExerciseAlternative { ExerciseName = "Flexiones inclinadas", Description = "Manos elevadas", EquipmentRequired = "Superficie elevada", DifficultyLevel = 2, Reason = "Menor dificultad" }
            });
        }

        return alternatives;
    }

    private ResponseValidationResult ValidateAIResponse(string response, UserRoutineParameters parameters)
    {
        var result = new ResponseValidationResult { IsValid = true, CorrectedResponse = response };

        // Check if response is in Spanish
        if (!IsResponseInSpanish(response))
        {
            result.Warnings.Add("Respuesta no completamente en español");
        }

        // Check if response has proper structure
        if (!HasProperStructure(response))
        {
            result.Warnings.Add("Estructura de respuesta incompleta");
        }

        // Check for forbidden exercises
        foreach (var avoidExercise in parameters.AvoidExercises)
        {
            if (response.ToLower().Contains(avoidExercise.ToLower()))
            {
                result.Warnings.Add($"Rutina contiene ejercicio a evitar: {avoidExercise}");
            }
        }

        // Check for equipment constraints
        var mentionedEquipment = ExtractMentionedEquipment(response);
        var unavailableEquipment = mentionedEquipment.Except(parameters.AvailableEquipment, StringComparer.OrdinalIgnoreCase).ToList();

        if (unavailableEquipment.Any())
        {
            result.Warnings.Add($"Rutina menciona equipamiento no disponible: {string.Join(", ", unavailableEquipment)}");
        }

        return result;
    }

    private bool IsResponseInSpanish(string response)
    {
        // Simple check for Spanish content
        var spanishIndicators = new[] { "ejercicio", "rutina", "entrenamiento", "series", "repeticiones", "descanso" };
        return spanishIndicators.Any(indicator => response.ToLower().Contains(indicator));
    }

    private bool HasProperStructure(string response)
    {
        // Check for key sections
        var requiredSections = new[] { "calentamiento", "ejercicios", "enfriamiento" };
        return requiredSections.All(section => response.ToLower().Contains(section));
    }

    private List<string> ExtractMentionedEquipment(string response)
    {
        var equipment = new List<string>();
        var responseLower = response.ToLower();

        var equipmentKeywords = new Dictionary<string, string>
        {
            { "mancuernas", "Mancuernas" },
            { "barra", "Barra" },
            { "máquina", "Máquinas" },
            { "bandas", "Bandas elásticas" },
            { "pelota", "Pelota de ejercicio" },
            { "esterilla", "Esterilla" }
        };

        foreach (var keyword in equipmentKeywords)
        {
            if (responseLower.Contains(keyword.Key))
            {
                equipment.Add(keyword.Value);
            }
        }

        return equipment.Distinct().ToList();
    }

    private UserRoutineParameters CreateAlternativeParameters(UserRoutineParameters original)
    {
        var alternative = new UserRoutineParameters
        {
            Name = original.Name,
            Age = original.Age,
            Gender = original.Gender,
            TrainingDaysPerWeek = original.TrainingDaysPerWeek,
            ExperienceLevel = original.ExperienceLevel,
            AvailableEquipment = original.AvailableEquipment,
            GymType = original.GymType,
            PrimaryGoal = original.PrimaryGoal,
            PhysicalLimitations = original.PhysicalLimitations,
            RecommendedIntensity = original.RecommendedIntensity,
            AvoidExercises = original.AvoidExercises,
            PreferredSessionDuration = original.PreferredSessionDuration,
            IncludeCardio = original.IncludeCardio,
            IncludeFlexibility = original.IncludeFlexibility,
            PreferredExerciseTypes = original.PreferredExerciseTypes,

            // Create variations in muscle group preferences
            MuscleGroupPreferences = original.MuscleGroupPreferences
                .Select(mgp => new MuscleGroupFocus
                {
                    MuscleGroup = mgp.MuscleGroup,
                    EmphasisLevel = mgp.EmphasisLevel,
                    Priority = mgp.Priority + (Random.Shared.Next(0, 2) == 0 ? 1 : -1) // Slight variation
                })
                .ToList()
        };

        return alternative;
    }

    private string BuildExerciseAlternativePrompt(string exerciseName, UserRoutineParameters parameters)
    {
        return $@"
Proporciona 3-5 ejercicios alternativos para: {exerciseName}

Parámetros del usuario:
- Equipamiento disponible: {string.Join(", ", parameters.AvailableEquipment)}
- Limitaciones: {string.Join(", ", parameters.PhysicalLimitations)}
- Nivel: {parameters.ExperienceLevel}

Para cada alternativa incluye:
- Nombre del ejercicio
- Músculos trabajados
- Equipamiento necesario
- Nivel de dificultad (1-5)
- Razón por la cual es una buena alternativa

Responde en español con formato de lista.";
    }

    private List<ExerciseAlternative> ParseExerciseAlternatives(string aiResponse)
    {
        // Simple parsing - in a real implementation, this would be more sophisticated
        var alternatives = new List<ExerciseAlternative>();
        var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        ExerciseAlternative? current = null;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("-") || trimmedLine.StartsWith("•") || char.IsDigit(trimmedLine[0]))
            {
                if (current != null)
                {
                    alternatives.Add(current);
                }

                current = new ExerciseAlternative
                {
                    ExerciseName = trimmedLine.TrimStart('-', '•', '1', '2', '3', '4', '5', '.', ' '),
                    DifficultyLevel = 3 // Default
                };
            }
            else if (current != null && trimmedLine.ToLower().Contains("músculos"))
            {
                current.Description = trimmedLine;
            }
        }

        if (current != null)
        {
            alternatives.Add(current);
        }

        return alternatives;
    }

    private RoutineMetadata BuildRoutineMetadata(UserRoutineParameters parameters, RoutineGenerationResult result)
    {
        return new RoutineMetadata
        {
            UserName = parameters.Name,
            PrimaryGoal = parameters.PrimaryGoal,
            EstimatedDuration = parameters.PreferredSessionDuration,
            DifficultyLevel = parameters.RecommendedIntensity,
            MuscleGroupsCovered = parameters.MuscleGroupPreferences.Select(mgp => mgp.MuscleGroup).ToList(),
            EquipmentUsed = parameters.AvailableEquipment,
            GeneratedAt = DateTime.UtcNow,
            PromptUsed = result.Source.ToString()
        };
    }

    private class ResponseValidationResult
    {
        public bool IsValid { get; set; }
        public string CorrectedResponse { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
    }
}