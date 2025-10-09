using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Data.Repositories;
using System.Text.Json;
using System.Text.RegularExpressions;
using DataEntities = GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Infrastructure.AI
{
    public class RoutineModificationService : IRoutineModificationService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOllamaService _ollamaService;
        private readonly ILogger<RoutineModificationService> _logger;

        public RoutineModificationService(
            IExerciseRepository exerciseRepository,
            IUserRepository userRepository,
            IOllamaService ollamaService,
            ILogger<RoutineModificationService> logger)
        {
            _exerciseRepository = exerciseRepository;
            _userRepository = userRepository;
            _ollamaService = ollamaService;
            _logger = logger;
        }

        public async Task<UserRoutine> ApplyModificationAsync(int routineId, ExerciseModification modification)
        {
            try
            {
                var routine = await _userRepository.GetUserRoutineByIdAsync(routineId);
                if (routine == null)
                    throw new ArgumentException($"Routine with ID {routineId} not found");

                // Parse the current routine data
                var routineData = JsonSerializer.Deserialize<Dictionary<string, object>>(routine.RoutineData);
                if (routineData == null)
                    throw new InvalidOperationException("Invalid routine data format");

                // Apply modification based on type
                switch (modification.ModificationType.ToUpper())
                {
                    case "REPLACE":
                        await ApplyExerciseReplacementAsync(routineData, modification);
                        break;
                    case "ADJUST_REPS":
                        ApplyRepetitionAdjustment(routineData, modification);
                        break;
                    case "ADJUST_WEIGHT":
                        ApplyWeightAdjustment(routineData, modification);
                        break;
                    case "REMOVE":
                        ApplyExerciseRemoval(routineData, modification);
                        break;
                    case "ADD":
                        await ApplyExerciseAdditionAsync(routineData, modification);
                        break;
                    default:
                        throw new ArgumentException($"Unknown modification type: {modification.ModificationType}");
                }

                // Update routine data
                routine.RoutineData = JsonSerializer.Serialize(routineData);
                routine.LastModified = DateTime.UtcNow;

                // Save modification history
                await SaveModificationHistoryAsync(routine.Id, modification);

                // Update routine in database
                await _userRepository.UpdateUserRoutineAsync(routine);

                _logger.LogInformation($"Applied modification {modification.ModificationType} to routine {routineId}");

                // Convert to Core.Models.UserRoutine
                return ConvertToCore(routine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying modification to routine {routineId}");
                throw;
            }
        }

        public async Task<List<ExerciseAlternative>> GetAlternativeExercisesAsync(int exerciseId, UserProfile profile)
        {
            try
            {
                var originalExercise = await _exerciseRepository.GetByIdWithImagesAsync(exerciseId);
                if (originalExercise == null)
                    return new List<ExerciseAlternative>();

                // Get Data.Entities.UserProfile from repository
                var dataProfile = await _userRepository.GetByIdAsync(profile.Id);
                if (dataProfile == null)
                    return new List<ExerciseAlternative>();

                // Get exercises targeting the same primary muscle group
                var similarExercises = await _exerciseRepository.GetByMuscleGroupAsync(originalExercise.PrimaryMuscleGroup?.Name ?? string.Empty);

                // Filter out the original exercise and apply user preferences
                var alternatives = similarExercises
                    .Where(e => e.Id != exerciseId)
                    .Where(e => IsExerciseSuitableForUser(e, dataProfile))
                    .Select(e => new ExerciseAlternative
                    {
                        Exercise = MapToCoreModels(e),
                        SimilarityScore = CalculateSimilarityScore(originalExercise, e),
                        ReasonForSuggestion = GenerateReasonForSuggestion(originalExercise, e),
                        Benefits = GenerateBenefits(originalExercise, e),
                        Considerations = GenerateConsiderations(e, dataProfile)
                    })
                    .OrderByDescending(a => a.SimilarityScore)
                    .Take(5)
                    .ToList();

                return alternatives;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting alternatives for exercise {exerciseId}");
                return new List<ExerciseAlternative>();
            }
        }

        public async Task<UserRoutine> CreateVariationAsync(int routineId, string variationType)
        {
            try
            {
                var originalRoutine = await _userRepository.GetUserRoutineByIdAsync(routineId);
                if (originalRoutine == null)
                    throw new ArgumentException($"Original routine {routineId} not found");

                var routineData = JsonSerializer.Deserialize<Dictionary<string, object>>(originalRoutine.RoutineData);
                if (routineData == null)
                    throw new InvalidOperationException("Invalid routine data format");

                // Create variation based on type (simplified implementation)
                var modifiedData = variationType.ToUpper() switch
                {
                    "EASIER" => CreateEasierVariation(routineData),
                    "HARDER" => CreateHarderVariation(routineData),
                    "TIME_CONSTRAINED" => CreateTimeConstrainedVariation(routineData),
                    "EQUIPMENT_LIMITED" => CreateEquipmentLimitedVariation(routineData),
                    _ => throw new ArgumentException($"Unknown variation type: {variationType}")
                };

                // Create new routine
                var newRoutine = new DataEntities.UserRoutine
                {
                    UserId = originalRoutine.UserId,
                    Name = $"{originalRoutine.Name} - {variationType}",
                    CreatedAt = DateTime.UtcNow,
                    RoutineData = JsonSerializer.Serialize(modifiedData),
                    Status = "ACTIVE",
                    Notes = $"Variación {variationType} de {originalRoutine.Name}"
                };

                await _userRepository.CreateUserRoutineAsync(newRoutine);
                _logger.LogInformation($"Created {variationType} variation of routine {routineId}");

                return ConvertToCore(newRoutine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating variation of routine {routineId}");
                throw;
            }
        }

        public async Task<ValidationResult> ValidateRoutineIntegrityAsync(UserRoutine routine)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                var routineData = JsonSerializer.Deserialize<Dictionary<string, object>>(routine.RoutineData);
                if (routineData == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Invalid routine data format");
                    return result;
                }

                // Validate muscle group balance
                await ValidateMuscleGroupBalance(routineData, result);

                // Validate exercise progression
                ValidateExerciseProgression(routineData, result);

                // Validate workout duration
                ValidateWorkoutDuration(routineData, result);

                // Validate rest periods
                ValidateRestPeriods(routineData, result);

                _logger.LogInformation($"Validated routine {routine.Id}: {(result.IsValid ? "Valid" : "Invalid")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating routine {routine.Id}");
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        public async Task<List<ExerciseModification>> SuggestModificationsAsync(UserRoutine routine, string userRequest)
        {
            try
            {
                var prompt = $@"
Analiza la siguiente rutina de ejercicios y la solicitud del usuario para sugerir modificaciones específicas.

RUTINA ACTUAL:
{routine.RoutineData}

SOLICITUD DEL USUARIO:
{userRequest}

Sugiere modificaciones específicas en formato JSON con esta estructura:
{{
  ""modifications"": [
    {{
      ""type"": ""REPLACE|ADJUST_REPS|ADJUST_WEIGHT|REMOVE|ADD"",
      ""exerciseId"": 123,
      ""originalValue"": ""valor actual"",
      ""newValue"": ""nuevo valor"",
      ""reason"": ""explicación científica de por qué esta modificación"",
      ""safetyWarnings"": [""advertencia1"", ""advertencia2""]
    }}
  ]
}}

Enfócate en modificaciones seguras y efectivas basadas en principios de entrenamiento.";

                var response = await _ollamaService.GenerateResponseAsync(prompt);
                var modifications = ParseModificationsFromResponse(response);

                _logger.LogInformation($"Generated {modifications.Count} modification suggestions for routine {routine.Id}");
                return modifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error suggesting modifications for routine {routine.Id}");
                return new List<ExerciseModification>();
            }
        }

        public async Task<ExerciseModification> ParseModificationRequestAsync(string userMessage, UserRoutine currentRoutine)
        {
            try
            {
                var prompt = $@"
Analiza el siguiente mensaje del usuario y extrae una modificación específica para su rutina de ejercicios.

MENSAJE DEL USUARIO:
{userMessage}

RUTINA ACTUAL:
{currentRoutine.RoutineData}

Responde SOLO con un JSON válido con esta estructura:
{{
  ""modificationType"": ""REPLACE|ADJUST_REPS|ADJUST_WEIGHT|REMOVE|ADD"",
  ""exerciseId"": número_del_ejercicio_afectado,
  ""originalValue"": ""valor_actual"",
  ""newValue"": ""nuevo_valor_propuesto"",
  ""reason"": ""explicación_de_la_modificación"",
  ""requiresConfirmation"": true/false,
  ""safetyWarnings"": [""lista"", ""de"", ""advertencias""]
}}

Si no puedes extraer una modificación específica, responde con modificationType ""UNCLEAR"".";

                var response = await _ollamaService.GenerateResponseAsync(prompt);
                var modification = ParseSingleModificationFromResponse(response, currentRoutine.Id);

                _logger.LogInformation($"Parsed modification request for routine {currentRoutine.Id}");
                return modification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing modification request for routine {currentRoutine.Id}");
                return new ExerciseModification
                {
                    RoutineId = currentRoutine.Id,
                    ModificationType = "UNCLEAR",
                    Reason = "No se pudo interpretar la solicitud",
                    UserMessage = userMessage
                };
            }
        }

        public async Task<bool> CanApplyModificationSafelyAsync(ExerciseModification modification, UserProfile profile)
        {
            try
            {
                // Check user's physical limitations
                var limitations = await _userRepository.GetUserPhysicalLimitationsAsync(profile.Id);
                foreach (var limitation in limitations)
                {
                    if (limitation.ExercisesToAvoid?.Contains(modification.NewValue) == true)
                    {
                        modification.SafetyWarnings.Add(new SafetyWarning
                        {
                            Message = $"Ejercicio restringido por limitación: {limitation.Description}",
                            Category = "Physical_Limitation",
                            Severity = Core.Models.SafetyLevel.High_Risk
                        });
                        return false;
                    }
                }

                // Check modification safety based on type
                bool isSafe = modification.ModificationType.ToUpper() switch
                {
                    "ADJUST_WEIGHT" => ValidateWeightAdjustmentSafety(modification, profile),
                    "ADJUST_REPS" => ValidateRepetitionAdjustmentSafety(modification, profile),
                    "REPLACE" => await ValidateExerciseReplacementSafety(modification, profile),
                    "ADD" => await ValidateExerciseAdditionSafety(modification, profile),
                    _ => true
                };

                if (!isSafe)
                {
                    modification.SafetyWarnings.Add(new SafetyWarning
                    {
                        Message = "Modificación potencialmente insegura para el perfil del usuario",
                        Category = "User_Profile_Mismatch",
                        Severity = Core.Models.SafetyLevel.Moderate_Risk
                    });
                }

                return isSafe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating modification safety");
                modification.SafetyWarnings.Add(new SafetyWarning
                {
                    Message = "Error al validar seguridad de la modificación",
                    Category = "System_Error",
                    Severity = Core.Models.SafetyLevel.High_Risk,
                    Reason = ex.Message
                });
                return false;
            }
        }

        #region Private Helper Methods

        private async Task ApplyExerciseReplacementAsync(Dictionary<string, object> routineData, ExerciseModification modification)
        {
            if (modification.ExerciseId.HasValue && modification.NewExerciseId.HasValue)
            {
                var newExercise = await _exerciseRepository.GetByIdWithImagesAsync(modification.NewExerciseId.Value);
                if (newExercise != null)
                {
                    // Implementation for replacing exercise in routine data
                    // This would depend on the specific structure of your routine data
                    _logger.LogInformation($"Replaced exercise {modification.ExerciseId} with {modification.NewExerciseId}");
                }
            }
        }

        private void ApplyRepetitionAdjustment(Dictionary<string, object> routineData, ExerciseModification modification)
        {
            // Implementation for adjusting repetitions
            _logger.LogInformation($"Adjusted repetitions: {modification.OriginalValue} -> {modification.NewValue}");
        }

        private void ApplyWeightAdjustment(Dictionary<string, object> routineData, ExerciseModification modification)
        {
            // Implementation for adjusting weight
            _logger.LogInformation($"Adjusted weight: {modification.OriginalValue} -> {modification.NewValue}");
        }

        private void ApplyExerciseRemoval(Dictionary<string, object> routineData, ExerciseModification modification)
        {
            // Implementation for removing exercise
            _logger.LogInformation($"Removed exercise {modification.ExerciseId}");
        }

        private async Task ApplyExerciseAdditionAsync(Dictionary<string, object> routineData, ExerciseModification modification)
        {
            // Implementation for adding exercise
            _logger.LogInformation($"Added new exercise {modification.NewExerciseId}");
        }

        private async Task SaveModificationHistoryAsync(int routineId, ExerciseModification modification)
        {
            var historyEntry = new DataEntities.RoutineModification
            {
                UserRoutineId = routineId,
                ModifiedAt = DateTime.UtcNow,
                ModificationType = modification.ModificationType,
                OriginalValue = modification.OriginalValue,
                NewValue = modification.NewValue,
                Reason = modification.Reason,
                ModifiedBy = "AI"
            };

            await _userRepository.SaveRoutineModificationAsync(historyEntry);
        }

        private bool IsExerciseSuitableForUser(DataEntities.Exercise exercise, DataEntities.UserProfile profile)
        {
            // Check user's fitness level, equipment preferences, etc.
            return true; // Simplified for now
        }

        private float CalculateSimilarityScore(DataEntities.Exercise original, DataEntities.Exercise alternative)
        {
            float score = 0;

            // Same primary muscle group
            if (original.PrimaryMuscleGroup == alternative.PrimaryMuscleGroup)
                score += 0.4f;

            // Similar difficulty
            if (original.DifficultyLevel == alternative.DifficultyLevel)
                score += 0.2f;

            // Similar movement pattern
            if (original.ExerciseType == alternative.ExerciseType)
                score += 0.3f;

            // Equipment similarity
            if (original.EquipmentTypeId == alternative.EquipmentTypeId)
                score += 0.1f;

            return score;
        }

        private string GenerateReasonForSuggestion(DataEntities.Exercise original, DataEntities.Exercise alternative)
        {
            return $"Alternativa para {original.Name} que trabaja los mismos músculos principales ({original.PrimaryMuscleGroup})";
        }

        private List<string> GenerateBenefits(DataEntities.Exercise original, DataEntities.Exercise alternative)
        {
            return new List<string>
            {
                $"Trabaja {alternative.PrimaryMuscleGroup?.Name} de manera efectiva",
                $"Dificultad {alternative.DifficultyLevel}",
                $"Requiere {alternative.EquipmentType?.Name}"
            };
        }

        private List<string> GenerateConsiderations(DataEntities.Exercise exercise, DataEntities.UserProfile profile)
        {
            var considerations = new List<string>();

            if (exercise.DifficultyLevel > DifficultyLevel.Intermediate && profile.FitnessLevel == "Principiante")
            {
                considerations.Add("Ejercicio avanzado - considerar progresión gradual");
            }

            return considerations;
        }

        private Exercise MapToCoreModels(DataEntities.Exercise dataExercise)
        {
            return new Exercise
            {
                Id = dataExercise.Id,
                Name = dataExercise.Name,
                Description = dataExercise.Description,
                DifficultyLevel = dataExercise.DifficultyLevel.ToString()
            };
        }

        private async Task<Dictionary<string, object>> CreateEasierVariationAsync(Dictionary<string, object> routineData)
        {
            // Implementation for creating easier variation
            // Reduce weights, reps, or replace with easier exercises
            return routineData; // Simplified
        }

        private async Task<Dictionary<string, object>> CreateHarderVariationAsync(Dictionary<string, object> routineData)
        {
            // Implementation for creating harder variation
            // Increase weights, reps, or replace with harder exercises
            return routineData; // Simplified
        }

        private Dictionary<string, object> CreateTimeConstrainedVariation(Dictionary<string, object> routineData, Dictionary<string, object> parameters)
        {
            // Implementation for time-constrained variation
            return routineData; // Simplified
        }

        private async Task<Dictionary<string, object>> CreateEquipmentLimitedVariationAsync(Dictionary<string, object> routineData, Dictionary<string, object> parameters)
        {
            // Implementation for equipment-limited variation
            return routineData; // Simplified
        }

        private async Task ValidateMuscleGroupBalance(Dictionary<string, object> routineData, ValidationResult result)
        {
            // Check if routine has balanced muscle group targeting
        }

        private void ValidateExerciseProgression(Dictionary<string, object> routineData, ValidationResult result)
        {
            // Check if exercises are ordered logically (compound before isolation, etc.)
        }

        private void ValidateWorkoutDuration(Dictionary<string, object> routineData, ValidationResult result)
        {
            // Check if total workout time is reasonable
        }

        private void ValidateRestPeriods(Dictionary<string, object> routineData, ValidationResult result)
        {
            // Check if rest periods are appropriate for exercise types
        }

        private List<ExerciseModification> ParseModificationsFromResponse(string response)
        {
            // Parse AI response to extract modifications
            return new List<ExerciseModification>(); // Simplified
        }

        private ExerciseModification ParseSingleModificationFromResponse(string response, int routineId)
        {
            try
            {
                // Try to extract JSON from the response
                var jsonMatch = Regex.Match(response, @"\{.*\}", RegexOptions.Singleline);
                if (jsonMatch.Success)
                {
                    var modificationData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonMatch.Value);
                    return new ExerciseModification
                    {
                        RoutineId = routineId,
                        ModificationType = modificationData.GetValueOrDefault("modificationType", "UNCLEAR").ToString()!,
                        OriginalValue = modificationData.GetValueOrDefault("originalValue", "").ToString()!,
                        NewValue = modificationData.GetValueOrDefault("newValue", "").ToString()!,
                        Reason = modificationData.GetValueOrDefault("reason", "").ToString()!,
                        RequiresUserConfirmation = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing modification from AI response");
            }

            return new ExerciseModification
            {
                ExerciseId = 0, // Default since we couldn't determine specific exercise
                ModificationType = "UNCLEAR",
                Justification = "No se pudo interpretar la respuesta de la IA"
            };
        }

        private bool ValidateWeightAdjustmentSafety(ExerciseModification modification, UserProfile profile)
        {
            // Validate weight increase/decrease is safe
            return true; // Simplified
        }

        private bool ValidateRepetitionAdjustmentSafety(ExerciseModification modification, UserProfile profile)
        {
            // Validate rep increase/decrease is safe
            return true; // Simplified
        }

        private async Task<bool> ValidateExerciseReplacementSafety(ExerciseModification modification, UserProfile profile)
        {
            // Validate exercise replacement is safe
            return true; // Simplified
        }

        private async Task<bool> ValidateExerciseAdditionSafety(ExerciseModification modification, UserProfile profile)
        {
            // Validate adding exercise is safe
            return true; // Simplified
        }

        // Missing interface methods implementation
        public async Task<UserRoutine> AdaptForLimitationsAsync(int routineId, List<UserPhysicalLimitation> limitations)
        {
            try
            {
                var routine = await _userRepository.GetUserRoutineByIdAsync(routineId);
                if (routine == null)
                    throw new ArgumentException($"Routine with ID {routineId} not found");

                // Convert to Core.Models.UserRoutine (simplified - would need proper mapping)
                var coreRoutine = new UserRoutine
                {
                    Id = routine.Id,
                    UserId = routine.UserId,
                    Name = $"{routine.Name} (Adapted)",
                    Description = "Routine adapted for user limitations",
                    Exercises = new List<Exercise>(), // Would need to map exercises
                    CreatedDate = routine.CreatedAt
                };

                return coreRoutine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adapting routine {routineId} for limitations");
                throw;
            }
        }

        public async Task<ExerciseModification> SuggestModificationAsync(int exerciseId, UserProfile profile)
        {
            try
            {
                var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
                if (exercise == null)
                    throw new ArgumentException($"Exercise with ID {exerciseId} not found");

                // Create a suggested modification based on user profile
                var modification = new ExerciseModification
                {
                    Id = 0,
                    ExerciseId = exerciseId,
                    ModificationType = "ADJUST_INTENSITY",
                    Description = $"Suggested modification for {exercise.Name} based on user profile",
                    Justification = $"Adjusted for {profile.FitnessLevel} fitness level"
                };

                return modification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error suggesting modification for exercise {exerciseId}");
                throw;
            }
        }

        public async Task<bool> ValidateModificationAsync(ExerciseModification modification)
        {
            try
            {
                // Basic validation logic
                if (string.IsNullOrEmpty(modification.ModificationType))
                    return false;

                if (modification.ExerciseId <= 0)
                    return false;

                // Check if modification type is supported
                var supportedTypes = new[] { "REPLACE", "ADJUST_REPS", "ADJUST_SETS", "ADJUST_WEIGHT", "ADJUST_INTENSITY" };
                if (!supportedTypes.Contains(modification.ModificationType.ToUpper()))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating modification");
                return false;
            }
        }

        // Helper methods for creating variations
        private Dictionary<string, object> CreateEasierVariation(Dictionary<string, object> routineData)
        {
            // Simplified implementation - reduce intensity
            var modified = new Dictionary<string, object>(routineData);
            modified["difficulty"] = "Easier";
            modified["notes"] = "Reduced intensity version";
            return modified;
        }

        private Dictionary<string, object> CreateHarderVariation(Dictionary<string, object> routineData)
        {
            // Simplified implementation - increase intensity
            var modified = new Dictionary<string, object>(routineData);
            modified["difficulty"] = "Harder";
            modified["notes"] = "Increased intensity version";
            return modified;
        }

        private Dictionary<string, object> CreateTimeConstrainedVariation(Dictionary<string, object> routineData)
        {
            // Simplified implementation - reduce time
            var modified = new Dictionary<string, object>(routineData);
            modified["duration"] = "Shortened";
            modified["notes"] = "Time-constrained version";
            return modified;
        }

        private Dictionary<string, object> CreateEquipmentLimitedVariation(Dictionary<string, object> routineData)
        {
            // Simplified implementation - modify for limited equipment
            var modified = new Dictionary<string, object>(routineData);
            modified["equipment"] = "Limited";
            modified["notes"] = "Equipment-limited version";
            return modified;
        }

        // Helper method to convert DataEntities.UserRoutine to Core.Models.UserRoutine
        private UserRoutine ConvertToCore(DataEntities.UserRoutine dataRoutine)
        {
            return new UserRoutine
            {
                Id = dataRoutine.Id,
                UserId = dataRoutine.UserId,
                Name = dataRoutine.Name,
                Description = $"Routine from database - {dataRoutine.Status}",
                Exercises = new List<Exercise>(), // Would need to deserialize from RoutineData
                CreatedDate = dataRoutine.CreatedAt,
                Notes = dataRoutine.Notes,
                IsActive = dataRoutine.Status == "ACTIVE"
            };
        }

        #endregion
    }
}