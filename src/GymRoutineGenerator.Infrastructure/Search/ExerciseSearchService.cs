using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;
using GymRoutineGenerator.Infrastructure.Mapping;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using UserProfile = GymRoutineGenerator.Core.Models.UserProfile;
using DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel;
using Exercise = GymRoutineGenerator.Core.Models.Exercise;

namespace GymRoutineGenerator.Infrastructure.Search
{
    public class ExerciseSearchService : IExerciseSearchService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IOllamaService _ollamaService;
        private readonly ISmartPromptService _promptService;
        private readonly ILogger<ExerciseSearchService> _logger;

        // Diccionario de sinónimos para búsqueda en español
        private static readonly Dictionary<string, List<string>> _synonyms = new()
        {
            ["pecho"] = new() { "pectoral", "torso", "busto" },
            ["espalda"] = new() { "dorsal", "latissimus", "trapecio" },
            ["piernas"] = new() { "extremidades inferiores", "muslos", "cuadriceps", "femoral" },
            ["brazos"] = new() { "extremidades superiores", "biceps", "triceps" },
            ["abdomen"] = new() { "core", "abdominal", "vientre", "cintura" },
            ["hombros"] = new() { "deltoides", "hombro" },
            ["gluteos"] = new() { "glúteo", "cola", "nalgas" },
            ["pantorrillas"] = new() { "gemelos", "pantorrilla", "sóleo" }
        };

        // Patrones de movimiento comunes
        private static readonly Dictionary<string, MovementType> _movementPatterns = new()
        {
            ["empujar"] = MovementType.Push,
            ["empuje"] = MovementType.Push,
            ["press"] = MovementType.Push,
            ["tirar"] = MovementType.Pull,
            ["jalar"] = MovementType.Pull,
            ["remo"] = MovementType.Pull,
            ["sentadilla"] = MovementType.Squat,
            ["sentarse"] = MovementType.Squat,
            ["agacharse"] = MovementType.Squat,
            ["peso muerto"] = MovementType.Hinge,
            ["bisagra"] = MovementType.Hinge,
            ["plancha"] = MovementType.Isometric,
            ["isométrico"] = MovementType.Isometric,
            ["saltar"] = MovementType.Explosive,
            ["explosivo"] = MovementType.Explosive
        };

        public ExerciseSearchService(
            IExerciseRepository exerciseRepository,
            IOllamaService ollamaService,
            ISmartPromptService promptService,
            ILogger<ExerciseSearchService> logger)
        {
            _exerciseRepository = exerciseRepository;
            _ollamaService = ollamaService;
            _promptService = promptService;
            _logger = logger;
        }

        public async Task<SearchResult> SearchExercisesAsync(SearchQuery query)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var results = new List<ExerciseSearchResult>();
                var allExercises = await _exerciseRepository.GetAllAsync();

                // Apply filters
                var mappedExercises = allExercises.Select(MapToExerciseModel).ToList();
                var filteredExercises = ApplyFilters(mappedExercises, query);

                // Score and rank exercises
                foreach (var exercise in filteredExercises)
                {
                    var score = CalculateRelevanceScore(exercise, query);
                    if (score > 0)
                    {
                        results.Add(new ExerciseSearchResult
                        {
                            Exercise = exercise,
                            RelevanceScore = score,
                            MatchReasons = GetMatchReasons(exercise, query),
                            Snippet = GenerateSnippet(exercise, query),
                            HighlightedTerms = GetHighlightedTerms(exercise, query),
                            Source = DataSource.Database
                        });
                    }
                }

                // Sort by relevance
                results = results.OrderByDescending(r => r.RelevanceScore)
                               .Take(query.MaxResults)
                               .ToList();

                // Generate search metadata
                var metadata = GenerateSearchMetadata(results);

                stopwatch.Stop();

                var searchResult = new SearchResult
                {
                    Exercises = results,
                    TotalCount = results.Count,
                    Metadata = metadata,
                    Suggestions = await GenerateSearchSuggestions(query, results),
                    QueryInterpretation = await InterpretQuery(query),
                    SearchDuration = stopwatch.Elapsed
                };

                _logger.LogInformation($"Search completed in {stopwatch.ElapsedMilliseconds}ms, found {results.Count} results");
                return searchResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing exercise search");
                stopwatch.Stop();
                return new SearchResult
                {
                    SearchDuration = stopwatch.Elapsed,
                    Suggestions = new List<string> { "Error en la búsqueda. Intenta con términos más simples." }
                };
            }
        }

        public async Task<List<Exercise>> SearchByNaturalLanguageAsync(string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                    return new List<Exercise>();

                _logger.LogInformation($"Starting natural language search for: {description}");

                // Use AI to interpret the natural language description
                var interpretationPrompt = $@"
Analiza la siguiente descripción de ejercicio y extrae información estructurada:

DESCRIPCIÓN: ""{description}""

Extrae:
1. Grupos musculares mencionados o implícitos
2. Tipo de movimiento
3. Equipamiento mencionado
4. Nivel de dificultad implícito
5. Palabras clave importantes

Responde en formato JSON:
{{
  ""muscleGroups"": [""lista de grupos musculares""],
  ""movementType"": ""tipo de movimiento"",
  ""equipment"": [""equipamiento mencionado""],
  ""difficulty"": ""principiante|intermedio|avanzado"",
  ""keywords"": [""palabras clave""],
  ""interpretation"": ""interpretación en lenguaje natural""
}}";

                var aiResponse = await _ollamaService.GenerateResponseAsync(interpretationPrompt);
                var interpretation = ParseNaturalLanguageInterpretation(aiResponse);

                // Create search query from interpretation
                var searchQuery = new SearchQuery
                {
                    TextQuery = description,
                    MuscleGroups = interpretation.MuscleGroups,
                    EquipmentTypes = interpretation.Equipment,
                    MaxResults = 10
                };

                // Add movement type filter if identified
                if (!string.IsNullOrEmpty(interpretation.MovementType))
                {
                    var movementType = ParseMovementType(interpretation.MovementType);
                    if (movementType.HasValue)
                    {
                        searchQuery.MovementTypes.Add(movementType.Value);
                    }
                }

                // Add difficulty filter if identified
                if (!string.IsNullOrEmpty(interpretation.Difficulty))
                {
                    var difficultyLevel = ParseDifficultyLevel(interpretation.Difficulty);
                    if (difficultyLevel.HasValue)
                    {
                        searchQuery.MinDifficulty = difficultyLevel;
                        searchQuery.MaxDifficulty = difficultyLevel;
                    }
                }

                // Perform the search
                var searchResult = await SearchExercisesAsync(searchQuery);

                // If no results with strict matching, try fuzzy search
                if (!searchResult.Exercises.Any())
                {
                    searchResult = await PerformFuzzySearch(description);
                }

                _logger.LogInformation($"Natural language search found {searchResult.Exercises.Count} exercises");
                return searchResult.Exercises.Select(e => e.Exercise).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in natural language search for: {description}");
                return new List<Exercise>();
            }
        }

        public async Task<Exercise?> IdentifyExerciseFromImageAsync(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return null;

                _logger.LogInformation("Starting exercise identification from image");

                // Note: This would require integration with a vision model like LLaVA
                // For now, we'll implement a basic approach using Ollama with vision capabilities
                var prompt = @"
Analiza esta imagen de ejercicio y identifica:
1. Qué ejercicio se está realizando
2. Qué grupos musculares trabaja
3. Qué equipamiento se está usando
4. Posición y técnica observada

Responde con el nombre específico del ejercicio si puedes identificarlo claramente.
Si no estás seguro, describe lo que ves y sugiere posibles ejercicios.";

                // This would need to be implemented with a vision-capable model
                // For now, return null as placeholder
                _logger.LogWarning("Image recognition not fully implemented - requires vision model integration");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error identifying exercise from image");
                return null;
            }
        }

        public async Task<List<Exercise>> GetSimilarExercisesAsync(int exerciseId)
        {
            try
            {
                var targetExercise = await _exerciseRepository.GetByIdWithImagesAsync(exerciseId);
                if (targetExercise == null)
                    return new List<Exercise>();

                var allExercises = await _exerciseRepository.GetAllAsync();
                var similarExercises = new List<(Exercise exercise, float similarity)>();

                foreach (var exercise in allExercises.Where(e => e.Id != exerciseId))
                {
                    var similarity = CalculateSimilarity(targetExercise.ToModel(), exercise.ToModel());
                    if (similarity > 0.3f) // Threshold for similarity
                    {
                        similarExercises.Add((exercise.ToModel(), similarity));
                    }
                }

                var result = similarExercises
                    .OrderByDescending(pair => pair.similarity)
                    .Take(8)
                    .Select(pair => pair.exercise)
                    .ToList();

                _logger.LogInformation($"Found {result.Count} similar exercises for exercise {exerciseId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding similar exercises for {exerciseId}");
                return new List<Exercise>();
            }
        }

        public async Task<SearchResult> SearchInDocsDirectoryAsync(string pattern)
        {
            try
            {
                _logger.LogInformation($"Searching in docs directory for pattern: {pattern}");

                // Search in docs/ejercicios directory
                var docsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "ejercicios");
                if (!Directory.Exists(docsPath))
                {
                    _logger.LogWarning($"Docs directory not found: {docsPath}");
                    return new SearchResult();
                }

                var results = new List<ExerciseSearchResult>();
                var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(file);
                        if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            var exercise = await ParseExerciseFromDocumentAsync(file, content);
                            if (exercise != null)
                            {
                                results.Add(new ExerciseSearchResult
                                {
                                    Exercise = exercise,
                                    RelevanceScore = CalculateDocumentRelevance(content, pattern),
                                    MatchReasons = new List<string> { $"Encontrado en documento: {Path.GetFileName(file)}" },
                                    Snippet = ExtractSnippet(content, pattern),
                                    Source = DataSource.DocsDirectory
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error processing file {file}");
                    }
                }

                _logger.LogInformation($"Found {results.Count} matches in docs directory");
                return new SearchResult
                {
                    Exercises = results.OrderByDescending(r => r.RelevanceScore).ToList(),
                    TotalCount = results.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching in docs directory");
                return new SearchResult();
            }
        }

        public async Task<List<Exercise>> GetTrendingExercisesAsync()
        {
            try
            {
                // Get exercises that have been used frequently in recent routines
                // This would require tracking usage statistics
                var allExercises = await _exerciseRepository.GetAllAsync();

                // For now, return popular compound movements
                var trendingExercises = allExercises
                    .Where(e => IsCompoundMovement(e.ToModel()))
                    .Select(e => e.ToModel())
                    .OrderBy(e => e.DifficultyLevel)
                    .Take(10)
                    .ToList();

                _logger.LogInformation($"Retrieved {trendingExercises?.Count ?? 0} trending exercises");
                return trendingExercises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending exercises");
                return new List<Exercise>();
            }
        }

        public async Task<List<Exercise>> GetRecommendedExercisesAsync(UserProfile profile)
        {
            try
            {
                var allExercises = await _exerciseRepository.GetAllAsync();
                var recommendations = new List<(Exercise exercise, float score)>();

                foreach (var exercise in allExercises)
                {
                    var score = CalculateRecommendationScore(exercise.ToModel(), profile);
                    if (score > 0.5f)
                    {
                        recommendations.Add((exercise.ToModel(), score));
                    }
                }

                var result = recommendations
                    .OrderByDescending(pair => pair.score)
                    .Take(12)
                    .Select(pair => pair.exercise)
                    .ToList();

                _logger.LogInformation($"Generated {result.Count} recommendations for user {profile.Name}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating recommendations for user {profile.Id}");
                return new List<Exercise>();
            }
        }

        public async Task<SearchResult> AdvancedSearchAsync(AdvancedSearchQuery query)
        {
            try
            {
                // Start with basic search
                var baseResult = await SearchExercisesAsync(query);

                // Apply advanced filters
                var filteredResults = baseResult.Exercises.Where(result =>
                {
                    var exercise = result.Exercise;

                    // Tag filter
                    if (query.Tags.Any() && !query.Tags.Any(tag =>
                        exercise.Name.Contains(tag, StringComparison.OrdinalIgnoreCase) ||
                        exercise.Description?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return false;
                    }

                    // Image requirement - Exercise model doesn't have Images property
                    if (query.RequireImages)
                    {
                        // For now, assume no images available in Core.Models.Exercise
                        return false;
                    }

                    // Instructions requirement
                    if (query.RequireInstructions && string.IsNullOrWhiteSpace(exercise.Instructions))
                    {
                        return false;
                    }

                    // Date range filter
                    if (query.CreatedDateRange != null)
                    {
                        if (exercise.CreatedAt < query.CreatedDateRange.From || exercise.CreatedAt > query.CreatedDateRange.To)
                        {
                            return false;
                        }
                    }

                    return true;
                }).ToList();

                baseResult.Exercises = filteredResults;
                baseResult.TotalCount = filteredResults?.Count() ?? 0;

                _logger.LogInformation($"Advanced search returned {filteredResults?.Count() ?? 0} results");
                return baseResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
                return new SearchResult();
            }
        }

        #region Private Helper Methods

        private List<Exercise> ApplyFilters(List<Exercise> exercises, SearchQuery query)
        {
            var filtered = exercises.AsEnumerable();

            // Muscle group filter
            if (query.MuscleGroups.Any())
            {
                filtered = filtered.Where(e =>
                    query.MuscleGroups.Any(mg =>
                        e.PrimaryMuscleGroup.Contains(mg, StringComparison.OrdinalIgnoreCase) ||
                        e.MuscleGroups.Any(emg => emg.Contains(mg, StringComparison.OrdinalIgnoreCase))));
            }

            // Equipment filter
            if (query.EquipmentTypes.Any())
            {
                filtered = filtered.Where(e =>
                    query.EquipmentTypes.Any(eq =>
                        e.RequiredEquipment.Any(re => string.Equals(re, eq, StringComparison.OrdinalIgnoreCase))));
            }

            // Difficulty filter
            if (query.MinDifficulty.HasValue)
            {
                var minDifficultyValue = (int)query.MinDifficulty.Value;
                filtered = filtered.Where(e => GetDifficultyValue(e.DifficultyLevel) >= minDifficultyValue);
            }

            if (query.MaxDifficulty.HasValue)
            {
                var maxDifficultyValue = (int)query.MaxDifficulty.Value;
                filtered = filtered.Where(e => GetDifficultyValue(e.DifficultyLevel) <= maxDifficultyValue);
            }

            // Exclude exercises
            if (query.ExcludeExercises.Any())
            {
                filtered = filtered.Where(e =>
                    !query.ExcludeExercises.Any(ex =>
                        e.Name.Contains(ex, StringComparison.OrdinalIgnoreCase)));
            }

            return filtered.ToList();
        }

        private float CalculateRelevanceScore(Exercise exercise, SearchQuery query)
        {
            float score = 0;

            if (string.IsNullOrWhiteSpace(query.TextQuery))
                return 1.0f; // If no text query, all exercises are equally relevant

            var textQuery = query.TextQuery.ToLowerInvariant();
            var exerciseName = exercise.Name.ToLowerInvariant();
            var description = exercise.Description?.ToLowerInvariant() ?? "";

            // Exact name match
            if (exerciseName.Contains(textQuery))
            {
                score += 1.0f;
            }

            // Description match
            if (description.Contains(textQuery))
            {
                score += 0.5f;
            }

            // Muscle group match
            if (exercise.PrimaryMuscleGroup.ToLowerInvariant().Contains(textQuery))
            {
                score += 0.7f;
            }

            // Equipment match
            if (exercise.Equipment.ToLowerInvariant().Contains(textQuery))
            {
                score += 0.3f;
            }

            // Synonym matching
            foreach (var synonym in _synonyms)
            {
                if (textQuery.Contains(synonym.Key))
                {
                    foreach (var synonymValue in synonym.Value)
                    {
                        if (exerciseName.Contains(synonymValue) || description.Contains(synonymValue))
                        {
                            score += 0.4f;
                            break;
                        }
                    }
                }
            }

            return score;
        }

        private List<string> GetMatchReasons(Exercise exercise, SearchQuery query)
        {
            var reasons = new List<string>();

            if (string.IsNullOrWhiteSpace(query.TextQuery))
                return reasons;

            var textQuery = query.TextQuery.ToLowerInvariant();

            if (exercise.Name.ToLowerInvariant().Contains(textQuery))
            {
                reasons.Add("Coincidencia en el nombre");
            }

            if (exercise.Description?.ToLowerInvariant().Contains(textQuery) == true)
            {
                reasons.Add("Coincidencia en la descripción");
            }

            if (exercise.PrimaryMuscleGroup.ToLowerInvariant().Contains(textQuery))
            {
                reasons.Add("Trabaja el grupo muscular buscado");
            }

            if (exercise.Equipment.ToLowerInvariant().Contains(textQuery))
            {
                reasons.Add("Usa el equipamiento especificado");
            }

            return reasons;
        }

        private string GenerateSnippet(Exercise exercise, SearchQuery query)
        {
            if (string.IsNullOrWhiteSpace(exercise.Description))
                return $"{exercise.Name} - {exercise.PrimaryMuscleGroup}";

            var description = exercise.Description;
            if (description.Length > 150)
            {
                description = description.Substring(0, 150) + "...";
            }

            return description;
        }

        private List<string> GetHighlightedTerms(Exercise exercise, SearchQuery query)
        {
            var terms = new List<string>();

            if (!string.IsNullOrWhiteSpace(query.TextQuery))
            {
                var queryWords = query.TextQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                terms.AddRange(queryWords);
            }

            return terms;
        }

        private SearchMetadata GenerateSearchMetadata(List<ExerciseSearchResult> results)
        {
            var metadata = new SearchMetadata();

            foreach (var result in results)
            {
                var exercise = result.Exercise;

                // Muscle group breakdown
                var muscleGroup = exercise.PrimaryMuscleGroup;
                metadata.MuscleGroupBreakdown[muscleGroup] = metadata.MuscleGroupBreakdown.GetValueOrDefault(muscleGroup, 0) + 1;

                // Difficulty breakdown
                var difficulty = exercise.DifficultyLevel.ToString();
                metadata.DifficultyBreakdown[difficulty] = metadata.DifficultyBreakdown.GetValueOrDefault(difficulty, 0) + 1;

                // Equipment breakdown
                var equipment = exercise.Equipment;
                metadata.EquipmentBreakdown[equipment] = metadata.EquipmentBreakdown.GetValueOrDefault(equipment, 0) + 1;

                // Source breakdown
                metadata.SourceBreakdown[result.Source] = metadata.SourceBreakdown.GetValueOrDefault(result.Source, 0) + 1;
            }

            return metadata;
        }

        private async Task<List<string>> GenerateSearchSuggestions(SearchQuery query, List<ExerciseSearchResult> results)
        {
            var suggestions = new List<string>();

            if (!results.Any())
            {
                suggestions.Add("Intenta usar términos más generales");
                suggestions.Add("Verifica la ortografía de tu búsqueda");
                suggestions.Add("Busca por grupo muscular (ej: 'pecho', 'espalda')");
            }
            else if (results.Count < 3)
            {
                suggestions.Add("Amplía tu búsqueda con términos relacionados");
                suggestions.Add("Prueba sinónimos de los términos usados");
            }

            return suggestions;
        }

        private async Task<string> InterpretQuery(SearchQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.TextQuery))
                return "Búsqueda sin filtros específicos";

            // Simple interpretation for now
            var interpretation = $"Buscando ejercicios que contengan: '{query.TextQuery}'";

            if (query.MuscleGroups.Any())
            {
                interpretation += $" para {string.Join(", ", query.MuscleGroups)}";
            }

            if (query.EquipmentTypes.Any())
            {
                interpretation += $" usando {string.Join(", ", query.EquipmentTypes)}";
            }

            return interpretation;
        }

        private NaturalLanguageInterpretation ParseNaturalLanguageInterpretation(string aiResponse)
        {
            // Parse AI response JSON
            try
            {
                // Simple regex-based parsing for JSON-like response
                var interpretation = new NaturalLanguageInterpretation();

                var muscleGroupsMatch = Regex.Match(aiResponse, @"""muscleGroups"":\s*\[(.*?)\]");
                if (muscleGroupsMatch.Success)
                {
                    var muscleGroups = muscleGroupsMatch.Groups[1].Value
                        .Split(',')
                        .Select(mg => mg.Trim(' ', '"'))
                        .Where(mg => !string.IsNullOrEmpty(mg))
                        .ToList();
                    interpretation.MuscleGroups = muscleGroups;
                }

                var movementMatch = Regex.Match(aiResponse, @"""movementType"":\s*""(.*?)""");
                if (movementMatch.Success)
                {
                    interpretation.MovementType = movementMatch.Groups[1].Value;
                }

                var equipmentMatch = Regex.Match(aiResponse, @"""equipment"":\s*\[(.*?)\]");
                if (equipmentMatch.Success)
                {
                    var equipment = equipmentMatch.Groups[1].Value
                        .Split(',')
                        .Select(eq => eq.Trim(' ', '"'))
                        .Where(eq => !string.IsNullOrEmpty(eq))
                        .ToList();
                    interpretation.Equipment = equipment;
                }

                var difficultyMatch = Regex.Match(aiResponse, @"""difficulty"":\s*""(.*?)""");
                if (difficultyMatch.Success)
                {
                    interpretation.Difficulty = difficultyMatch.Groups[1].Value;
                }

                return interpretation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI interpretation");
                return new NaturalLanguageInterpretation();
            }
        }

        private MovementType? ParseMovementType(string movementTypeStr)
        {
            foreach (var pattern in _movementPatterns)
            {
                if (movementTypeStr.Contains(pattern.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return pattern.Value;
                }
            }

            return null;
        }

        private DifficultyLevel? ParseDifficultyLevel(string difficultyStr)
        {
            return difficultyStr.ToLowerInvariant() switch
            {
                "principiante" or "beginner" or "fácil" => DifficultyLevel.Beginner,
                "intermedio" or "intermediate" or "medio" => DifficultyLevel.Intermediate,
                "avanzado" or "advanced" or "difícil" => DifficultyLevel.Advanced,
                _ => null
            };
        }

        private async Task<SearchResult> PerformFuzzySearch(string description)
        {
            // Implement fuzzy search with relaxed matching
            var allExercises = await _exerciseRepository.GetAllAsync();
            var results = new List<ExerciseSearchResult>();

            var words = description.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var exercise in allExercises)
            {
                var score = 0f;
                var matchReasons = new List<string>();

                foreach (var word in words)
                {
                    if (exercise.Name.ToLowerInvariant().Contains(word))
                    {
                        score += 0.5f;
                        matchReasons.Add($"Coincidencia parcial: '{word}'");
                    }

                    if (exercise.Description?.ToLowerInvariant().Contains(word) == true)
                    {
                        score += 0.2f;
                    }
                }

                if (score > 0)
                {
                    results.Add(new ExerciseSearchResult
                    {
                        Exercise = MapToExerciseModel(exercise),
                        RelevanceScore = score,
                        MatchReasons = matchReasons,
                        Source = DataSource.Database
                    });
                }
            }

            return new SearchResult
            {
                Exercises = results.OrderByDescending(r => r.RelevanceScore).Take(10).ToList(),
                TotalCount = results.Count
            };
        }

        private float CalculateSimilarity(Exercise exercise1, Exercise exercise2)
        {
            float similarity = 0;

            // Same primary muscle group
            if (exercise1.PrimaryMuscleGroup == exercise2.PrimaryMuscleGroup)
                similarity += 0.4f;

            // Similar difficulty (assuming difficulty levels: Principiante=1, Intermedio=2, Avanzado=3)
            var diff1 = GetDifficultyValue(exercise1.DifficultyLevel);
            var diff2 = GetDifficultyValue(exercise2.DifficultyLevel);
            var difficultyDiff = Math.Abs(diff1 - diff2);
            similarity += (3 - difficultyDiff) * 0.1f;

            // Same equipment
            if (exercise1.Equipment == exercise2.Equipment)
                similarity += 0.2f;

            // Same exercise type
            if (exercise1.ExerciseType == exercise2.ExerciseType)
                similarity += 0.3f;

            return Math.Max(0, similarity);
        }

        private async Task<Exercise?> ParseExerciseFromDocumentAsync(string filePath, string content)
        {
            // Parse exercise information from markdown document
            try
            {
                var lines = content.Split('\n');
                var exercise = new Exercise();

                foreach (var line in lines)
                {
                    if (line.StartsWith("# "))
                    {
                        exercise.Name = line.Substring(2).Trim();
                    }
                    else if (line.StartsWith("**Músculo principal:**"))
                    {
                        exercise.PrimaryMuscleGroup = line.Replace("**Músculo principal:**", "").Trim();
                    }
                    else if (line.StartsWith("**Equipamiento:**"))
                    {
                        exercise.Equipment = line.Replace("**Equipamiento:**", "").Trim();
                    }
                }

                exercise.Description = content;
                exercise.CreatedAt = File.GetCreationTime(filePath);

                return exercise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing exercise from document {filePath}");
                return null;
            }
        }

        private int GetDifficultyValue(string difficultyLevel)
        {
            return difficultyLevel?.ToLower() switch
            {
                "principiante" or "facil" or "fácil" => 1,
                "intermedio" or "medio" => 2,
                "avanzado" or "dificil" or "difícil" => 3,
                _ => 2 // Default to intermediate
            };
        }

        private float CalculateDocumentRelevance(string content, string pattern)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase).Count;
            return Math.Min(matches * 0.1f, 1.0f);
        }

        private string ExtractSnippet(string content, string pattern)
        {
            var match = Regex.Match(content, $".{{0,50}}{pattern}.{{0,50}}", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : content.Substring(0, Math.Min(100, content.Length));
        }

        private bool IsCompoundMovement(Exercise exercise)
        {
            var compoundIndicators = new[] { "sentadilla", "peso muerto", "press", "remo", "dominada" };
            return compoundIndicators.Any(indicator =>
                exercise.Name.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }

        private float CalculateRecommendationScore(Exercise exercise, UserProfile profile)
        {
            float score = 0.5f; // Base score

            // Adjust based on user's fitness level
            var exerciseDifficultyLevel = GetUserDifficultyLevel(exercise.DifficultyLevel);
            var levelDifference = Math.Abs(exerciseDifficultyLevel - GetUserDifficultyLevel(profile.FitnessLevel));
            score -= levelDifference * 0.2f;

            // Prefer exercises appropriate for user's age
            if (profile.Age > 50 && exerciseDifficultyLevel > 1) // 1 = Intermediate
            {
                score -= 0.3f;
            }

            // Prefer compound movements for beginners
            if (profile.FitnessLevel == "Principiante" && IsCompoundMovement(exercise))
            {
                score += 0.2f;
            }

            return Math.Max(0, score);
        }

        private int GetUserDifficultyLevel(string fitnessLevel)
        {
            return fitnessLevel.ToLowerInvariant() switch
            {
                "principiante" => 0,
                "intermedio" => 1,
                "avanzado" => 2,
                _ => 0
            };
        }


        #endregion

        private class NaturalLanguageInterpretation
        {
            public List<string> MuscleGroups { get; set; } = new();
            public string MovementType { get; set; } = string.Empty;
            public List<string> Equipment { get; set; } = new();
            public string Difficulty { get; set; } = string.Empty;
            public List<string> Keywords { get; set; } = new();
            public string Interpretation { get; set; } = string.Empty;
        }

        private GymRoutineGenerator.Core.Models.Exercise MapToExerciseModel(GymRoutineGenerator.Data.Entities.Exercise dataExercise)
        {
            return new GymRoutineGenerator.Core.Models.Exercise
            {
                Id = dataExercise.Id,
                Name = dataExercise.Name,
                SpanishName = dataExercise.SpanishName,
                Description = dataExercise.Description,
                Instructions = dataExercise.Instructions,
                MuscleGroups = new List<string> { dataExercise.PrimaryMuscleGroup?.Name ?? "General" },
                PrimaryMuscleGroup = dataExercise.PrimaryMuscleGroup?.Name ?? "General",
                Equipment = dataExercise.EquipmentType?.Name ?? "Sin equipamiento",
                RequiredEquipment = new List<string> { dataExercise.EquipmentType?.Name ?? "Sin equipamiento" },
                DifficultyLevel = dataExercise.DifficultyLevel.ToString(),
                ExerciseType = MapExerciseType(dataExercise.ExerciseType),
                Type = dataExercise.ExerciseType.ToString(),
                Tags = new List<string>(),
                CreatedAt = dataExercise.CreatedAt,
                RecommendedSets = 3,
                RecommendedReps = "8-12",
                RestPeriod = "60-90 seconds",
                Modifications = new List<string>(),
                SafetyNotes = new List<string>()
            };
        }

        private GymRoutineGenerator.Core.Models.ExerciseType MapExerciseType(GymRoutineGenerator.Core.Enums.ExerciseType sourceType)
        {
            return sourceType switch
            {
                GymRoutineGenerator.Core.Enums.ExerciseType.Strength => GymRoutineGenerator.Core.Models.ExerciseType.Compound,
                GymRoutineGenerator.Core.Enums.ExerciseType.Cardio => GymRoutineGenerator.Core.Models.ExerciseType.Cardio,
                GymRoutineGenerator.Core.Enums.ExerciseType.Flexibility => GymRoutineGenerator.Core.Models.ExerciseType.Flexibility,
                GymRoutineGenerator.Core.Enums.ExerciseType.Balance => GymRoutineGenerator.Core.Models.ExerciseType.Balance,
                GymRoutineGenerator.Core.Enums.ExerciseType.Functional => GymRoutineGenerator.Core.Models.ExerciseType.Compound,
                _ => GymRoutineGenerator.Core.Models.ExerciseType.Compound // Default
            };
        }
    }
}