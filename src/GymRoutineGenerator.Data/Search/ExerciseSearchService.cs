using System.Diagnostics;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Data.Search;

public class ExerciseSearchService : IExerciseSearchService
{
    private readonly GymRoutineContext _context;

    public ExerciseSearchService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<SearchResultsPage> SearchExercisesAsync(ExerciseSearchCriteria criteria)
    {
        var stopwatch = Stopwatch.StartNew();

        var query = BuildSearchQuery(criteria);

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

        // Apply pagination
        var exercises = await query
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync();

        // Convert to search results
        var results = exercises.Select(MapToSearchResult).ToList();

        // Calculate search scores if text search
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            results = CalculateSearchScores(results, criteria.SearchTerm);
        }

        stopwatch.Stop();

        var resultsPage = new SearchResultsPage
        {
            Results = results,
            TotalCount = totalCount,
            PageNumber = (criteria.Skip / criteria.Take) + 1,
            PageSize = criteria.Take,
            Statistics = await CalculateStatisticsAsync(criteria, stopwatch.Elapsed),
            Suggestions = await GenerateSuggestionsAsync(criteria, totalCount)
        };

        return resultsPage;
    }

    public async Task<SearchResultsPage> SearchExercisesByTextAsync(string searchTerm, int skip = 0, int take = 50)
    {
        var criteria = new ExerciseSearchCriteria
        {
            SearchTerm = searchTerm,
            Skip = skip,
            Take = take,
            SortBy = ExerciseSearchSort.Relevance
        };

        return await SearchExercisesAsync(criteria);
    }

    public async Task<List<ExerciseSearchResult>> GetSimilarExercisesAsync(int exerciseId, int maxResults = 10)
    {
        var targetExercise = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (targetExercise == null)
            return new List<ExerciseSearchResult>();

        // Find exercises with same primary muscle group or equipment
        var similarExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Where(e => e.Id != exerciseId && e.IsActive)
            .Where(e => e.PrimaryMuscleGroupId == targetExercise.PrimaryMuscleGroupId ||
                       e.EquipmentTypeId == targetExercise.EquipmentTypeId ||
                       e.DifficultyLevel == targetExercise.DifficultyLevel)
            .Take(maxResults)
            .ToListAsync();

        return similarExercises.Select(MapToSearchResult).ToList();
    }

    public async Task<List<ExerciseSearchResult>> GetRandomExercisesAsync(int count = 10, ExerciseSearchCriteria? filters = null)
    {
        var query = _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Where(e => e.IsActive);

        if (filters != null)
        {
            query = ApplyFilters(query, filters);
        }

        // Get total count first
        var totalCount = await query.CountAsync();
        var random = new Random();
        var skip = totalCount > count ? random.Next(0, totalCount - count) : 0;

        // Use skip to get random results (more efficient than OrderBy Guid)
        var randomExercises = await query
            .Skip(skip)
            .Take(count)
            .ToListAsync();

        return randomExercises.Select(MapToSearchResult).ToList();
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string partialTerm, int maxSuggestions = 10)
    {
        if (string.IsNullOrWhiteSpace(partialTerm) || partialTerm.Length < 2)
            return new List<string>();

        var lowerTerm = partialTerm.ToLower();
        var suggestions = new List<string>();

        // Exercise names (Spanish)
        var exerciseNames = await _context.Exercises
            .Where(e => e.IsActive && e.SpanishName.ToLower().Contains(lowerTerm))
            .Select(e => e.SpanishName)
            .Take(maxSuggestions / 2)
            .ToListAsync();
        suggestions.AddRange(exerciseNames);

        // Muscle group names (Spanish)
        var muscleGroupNames = await _context.MuscleGroups
            .Where(mg => mg.SpanishName.ToLower().Contains(lowerTerm))
            .Select(mg => mg.SpanishName)
            .Take(maxSuggestions / 4)
            .ToListAsync();
        suggestions.AddRange(muscleGroupNames);

        // Equipment type names (Spanish)
        var equipmentNames = await _context.EquipmentTypes
            .Where(et => et.SpanishName.ToLower().Contains(lowerTerm))
            .Select(et => et.SpanishName)
            .Take(maxSuggestions / 4)
            .ToListAsync();
        suggestions.AddRange(equipmentNames);

        return suggestions.Distinct().Take(maxSuggestions).ToList();
    }

    public async Task<SearchStatistics> GetSearchStatisticsAsync(ExerciseSearchCriteria criteria)
    {
        var query = BuildSearchQuery(criteria);

        var resultsByMuscleGroup = await query
            .GroupBy(e => e.PrimaryMuscleGroup.SpanishName)
            .Select(g => new { MuscleGroup = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MuscleGroup, x => x.Count);

        var resultsByEquipment = await query
            .GroupBy(e => e.EquipmentType.SpanishName)
            .Select(g => new { Equipment = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Equipment, x => x.Count);

        var resultsByDifficulty = await query
            .GroupBy(e => e.DifficultyLevel)
            .Select(g => new { Difficulty = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Difficulty, x => x.Count);

        var resultsByType = await query
            .GroupBy(e => e.ExerciseType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return new SearchStatistics
        {
            ResultsByMuscleGroup = resultsByMuscleGroup,
            ResultsByEquipment = resultsByEquipment,
            ResultsByDifficulty = resultsByDifficulty,
            ResultsByType = resultsByType
        };
    }

    public async Task<List<ExerciseSearchResult>> GetExercisesByMuscleGroupAsync(int muscleGroupId, int maxResults = 20)
    {
        var exercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Where(e => e.IsActive && (e.PrimaryMuscleGroupId == muscleGroupId ||
                       e.SecondaryMuscles.Any(sm => sm.MuscleGroupId == muscleGroupId)))
            .OrderBy(e => e.SpanishName)
            .Take(maxResults)
            .ToListAsync();

        return exercises.Select(MapToSearchResult).ToList();
    }

    public async Task<List<ExerciseSearchResult>> GetExercisesByEquipmentAsync(int equipmentTypeId, int maxResults = 20)
    {
        var exercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Where(e => e.IsActive && e.EquipmentTypeId == equipmentTypeId)
            .OrderBy(e => e.SpanishName)
            .Take(maxResults)
            .ToListAsync();

        return exercises.Select(MapToSearchResult).ToList();
    }

    public async Task<List<ExerciseSearchResult>> GetExercisesByDifficultyAsync(DifficultyLevel difficulty, int maxResults = 20)
    {
        var exercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Where(e => e.IsActive && e.DifficultyLevel == difficulty)
            .OrderBy(e => e.SpanishName)
            .Take(maxResults)
            .ToListAsync();

        return exercises.Select(MapToSearchResult).ToList();
    }

    private IQueryable<Exercise> BuildSearchQuery(ExerciseSearchCriteria criteria)
    {
        var query = _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .AsQueryable();

        // Apply filters
        query = ApplyFilters(query, criteria);

        return query;
    }

    private IQueryable<Exercise> ApplyFilters(IQueryable<Exercise> query, ExerciseSearchCriteria criteria)
    {
        // Active filter
        if (criteria.IsActive.HasValue)
        {
            query = query.Where(e => e.IsActive == criteria.IsActive.Value);
        }

        // Text search
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(e =>
                e.SpanishName.ToLower().Contains(searchTerm) ||
                e.Name.ToLower().Contains(searchTerm) ||
                e.Description.ToLower().Contains(searchTerm) ||
                e.PrimaryMuscleGroup.SpanishName.ToLower().Contains(searchTerm) ||
                e.EquipmentType.SpanishName.ToLower().Contains(searchTerm));
        }

        // Muscle group filter
        if (criteria.MuscleGroupIds?.Any() == true)
        {
            if (criteria.IncludeSecondaryMuscles)
            {
                query = query.Where(e =>
                    criteria.MuscleGroupIds.Contains(e.PrimaryMuscleGroupId) ||
                    e.SecondaryMuscles.Any(sm => criteria.MuscleGroupIds.Contains(sm.MuscleGroupId)));
            }
            else
            {
                query = query.Where(e => criteria.MuscleGroupIds.Contains(e.PrimaryMuscleGroupId));
            }
        }

        // Equipment filter
        if (criteria.EquipmentTypeIds?.Any() == true)
        {
            query = query.Where(e => criteria.EquipmentTypeIds.Contains(e.EquipmentTypeId));
        }

        // Difficulty filter
        if (criteria.DifficultyLevels?.Any() == true)
        {
            query = query.Where(e => criteria.DifficultyLevels.Contains(e.DifficultyLevel));
        }

        // Exercise type filter
        if (criteria.ExerciseTypes?.Any() == true)
        {
            query = query.Where(e => criteria.ExerciseTypes.Contains(e.ExerciseType));
        }

        // Duration filter
        if (criteria.DurationMinSeconds.HasValue)
        {
            query = query.Where(e => e.DurationSeconds >= criteria.DurationMinSeconds.Value);
        }

        if (criteria.DurationMaxSeconds.HasValue)
        {
            query = query.Where(e => e.DurationSeconds <= criteria.DurationMaxSeconds.Value || e.DurationSeconds == null);
        }

        return query;
    }

    private IQueryable<Exercise> ApplySorting(IQueryable<Exercise> query, ExerciseSearchSort sortBy, bool descending)
    {
        return sortBy switch
        {
            ExerciseSearchSort.Name => descending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            ExerciseSearchSort.SpanishName => descending ? query.OrderByDescending(e => e.SpanishName) : query.OrderBy(e => e.SpanishName),
            ExerciseSearchSort.Difficulty => descending ? query.OrderByDescending(e => e.DifficultyLevel) : query.OrderBy(e => e.DifficultyLevel),
            ExerciseSearchSort.ExerciseType => descending ? query.OrderByDescending(e => e.ExerciseType) : query.OrderBy(e => e.ExerciseType),
            ExerciseSearchSort.MuscleGroup => descending ? query.OrderByDescending(e => e.PrimaryMuscleGroup.SpanishName) : query.OrderBy(e => e.PrimaryMuscleGroup.SpanishName),
            ExerciseSearchSort.Equipment => descending ? query.OrderByDescending(e => e.EquipmentType.SpanishName) : query.OrderBy(e => e.EquipmentType.SpanishName),
            ExerciseSearchSort.Duration => descending ? query.OrderByDescending(e => e.DurationSeconds) : query.OrderBy(e => e.DurationSeconds),
            ExerciseSearchSort.CreatedDate => descending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
            ExerciseSearchSort.UpdatedDate => descending ? query.OrderByDescending(e => e.UpdatedAt) : query.OrderBy(e => e.UpdatedAt),
            _ => query.OrderBy(e => e.SpanishName)
        };
    }

    private ExerciseSearchResult MapToSearchResult(Exercise exercise)
    {
        return new ExerciseSearchResult
        {
            Id = exercise.Id,
            Name = exercise.Name,
            SpanishName = exercise.SpanishName,
            Description = exercise.Description,
            Instructions = exercise.Instructions,
            DifficultyLevel = exercise.DifficultyLevel,
            ExerciseType = exercise.ExerciseType,
            DurationSeconds = exercise.DurationSeconds,
            IsActive = exercise.IsActive,
            PrimaryMuscleGroup = new MuscleGroupInfo
            {
                Id = exercise.PrimaryMuscleGroup.Id,
                Name = exercise.PrimaryMuscleGroup.Name,
                SpanishName = exercise.PrimaryMuscleGroup.SpanishName
            },
            EquipmentType = new EquipmentTypeInfo
            {
                Id = exercise.EquipmentType.Id,
                Name = exercise.EquipmentType.Name,
                SpanishName = exercise.EquipmentType.SpanishName,
                RequiresGym = exercise.EquipmentType.IsRequired
            },
            SecondaryMuscleGroups = exercise.SecondaryMuscles?.Select(sm => new MuscleGroupInfo
            {
                Id = sm.MuscleGroup.Id,
                Name = sm.MuscleGroup.Name,
                SpanishName = sm.MuscleGroup.SpanishName
            }).ToList() ?? new List<MuscleGroupInfo>(),
            Images = exercise.Images?.Select(img => new ExerciseImageInfo
            {
                Id = img.Id,
                ImagePath = img.ImagePath,
                ImagePosition = img.ImagePosition,
                IsPrimary = img.IsPrimary,
                Description = img.Description
            }).ToList() ?? new List<ExerciseImageInfo>(),
            PrimaryImagePath = exercise.Images?.FirstOrDefault(img => img.IsPrimary)?.ImagePath
        };
    }

    private List<ExerciseSearchResult> CalculateSearchScores(List<ExerciseSearchResult> results, string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        var searchTerms = lowerSearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var result in results)
        {
            var score = 0.0;
            var matchedTerms = new List<string>();

            foreach (var term in searchTerms)
            {
                // Exact match in Spanish name (highest score)
                if (result.SpanishName.ToLower().Contains(term))
                {
                    score += result.SpanishName.ToLower() == term ? 100 : 80;
                    matchedTerms.Add($"Nombre: {term}");
                }

                // Match in English name
                if (result.Name.ToLower().Contains(term))
                {
                    score += result.Name.ToLower() == term ? 90 : 70;
                    matchedTerms.Add($"English: {term}");
                }

                // Match in description
                if (result.Description.ToLower().Contains(term))
                {
                    score += 50;
                    matchedTerms.Add($"Descripción: {term}");
                }

                // Match in muscle group
                if (result.PrimaryMuscleGroup.SpanishName.ToLower().Contains(term))
                {
                    score += 60;
                    matchedTerms.Add($"Músculo: {term}");
                }

                // Match in equipment
                if (result.EquipmentType.SpanishName.ToLower().Contains(term))
                {
                    score += 40;
                    matchedTerms.Add($"Equipo: {term}");
                }

                // Match in secondary muscles
                foreach (var secondaryMuscle in result.SecondaryMuscleGroups)
                {
                    if (secondaryMuscle.SpanishName.ToLower().Contains(term))
                    {
                        score += 30;
                        matchedTerms.Add($"Músculo secundario: {term}");
                    }
                }
            }

            result.SearchScore = score;
            result.MatchedTerms = matchedTerms.Distinct().ToList();
        }

        return results.OrderByDescending(r => r.SearchScore).ToList();
    }

    private async Task<SearchStatistics> CalculateStatisticsAsync(ExerciseSearchCriteria criteria, TimeSpan duration)
    {
        var statistics = await GetSearchStatisticsAsync(criteria);
        statistics.SearchDuration = duration;
        return statistics;
    }

    private async Task<List<SearchSuggestion>> GenerateSuggestionsAsync(ExerciseSearchCriteria criteria, int totalResults)
    {
        var suggestions = new List<SearchSuggestion>();

        // If no results, suggest alternatives
        if (totalResults == 0)
        {
            // Suggest removing filters
            if (criteria.DifficultyLevels?.Any() == true)
            {
                suggestions.Add(new SearchSuggestion
                {
                    Text = "Probar con diferentes niveles de dificultad",
                    Type = SearchSuggestionType.DifficultyAdjustment,
                    Description = "Amplía la búsqueda incluyendo más niveles de dificultad"
                });
            }

            if (criteria.EquipmentTypeIds?.Any() == true)
            {
                suggestions.Add(new SearchSuggestion
                {
                    Text = "Probar con diferente equipo",
                    Type = SearchSuggestionType.AlternativeEquipment,
                    Description = "Busca ejercicios que usen otros tipos de equipo"
                });
            }

            // Suggest popular exercises
            var popularExercises = await GetRandomExercisesAsync(3);
            foreach (var exercise in popularExercises)
            {
                suggestions.Add(new SearchSuggestion
                {
                    Text = exercise.SpanishName,
                    Type = SearchSuggestionType.PopularExercise,
                    Description = $"Ejercicio popular para {exercise.PrimaryMuscleGroup.SpanishName}"
                });
            }
        }

        return suggestions;
    }
}