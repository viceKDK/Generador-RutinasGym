using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Data.Repositories;
using System.Text.Json;
using DataEntities = GymRoutineGenerator.Data.Entities;
using CoreSafetyLevel = GymRoutineGenerator.Core.Models.SafetyLevel;

namespace GymRoutineGenerator.Infrastructure.AI
{
    public class SafetyValidationService : ISafetyValidationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IOllamaService _ollamaService;
        private readonly ISmartPromptService _promptService;
        private readonly ILogger<SafetyValidationService> _logger;

        // Safety rules database - in a real implementation, this would be in a database
        private static readonly Dictionary<string, List<string>> _contraindicationRules = new()
        {
            ["Knee_Problems"] = new() { "squat", "lunge", "jump", "deep knee bend" },
            ["Back_Problems"] = new() { "deadlift", "bent over row", "heavy overhead press" },
            ["Shoulder_Injury"] = new() { "overhead press", "lateral raise", "pull-up" },
            ["Heart_Condition"] = new() { "high intensity", "explosive movements", "heavy lifting" },
            ["Pregnancy"] = new() { "supine exercises", "heavy weights", "high impact" },
            ["Osteoporosis"] = new() { "high impact", "spinal flexion", "rotational movements" }
        };

        private static readonly Dictionary<DifficultyLevel, float> _difficultyRiskMultipliers = new()
        {
            [DifficultyLevel.Beginner] = 1.0f,
            [DifficultyLevel.Intermediate] = 1.5f,
            [DifficultyLevel.Advanced] = 2.5f
        };

        public SafetyValidationService(
            IUserRepository userRepository,
            IExerciseRepository exerciseRepository,
            IOllamaService ollamaService,
            ISmartPromptService promptService,
            ILogger<SafetyValidationService> logger)
        {
            _userRepository = userRepository;
            _exerciseRepository = exerciseRepository;
            _ollamaService = ollamaService;
            _promptService = promptService;
            _logger = logger;
        }

        public async Task<SafetyValidationResult> ValidateExerciseForUserAsync(Exercise exercise, UserProfile user)
        {
            // Fetch Data.Entities types from repository
            var dataExercise = await _exerciseRepository.GetByIdWithImagesAsync(exercise.Id);
            var dataUser = await _userRepository.GetByIdAsync(user.Id);

            if (dataExercise == null || dataUser == null)
                return new SafetyValidationResult { IsSafe = false, SafetyLevel = CoreSafetyLevel.High_Risk };

            return await ValidateExerciseForUserInternalAsync(dataExercise, dataUser);
        }

        public async Task<SafetyValidationResult> ValidateRoutineModificationAsync(ExerciseModification modification, UserProfile user)
        {
            var dataUser = await _userRepository.GetByIdAsync(user.Id);
            if (dataUser == null)
                return new SafetyValidationResult { IsSafe = false, SafetyLevel = CoreSafetyLevel.High_Risk };

            return await ValidateRoutineModificationInternalAsync(modification, dataUser);
        }

        public async Task<List<SafetyWarning>> GetSafetyWarningsAsync(Exercise exercise, UserProfile user)
        {
            var dataExercise = await _exerciseRepository.GetByIdWithImagesAsync(exercise.Id);
            var dataUser = await _userRepository.GetByIdAsync(user.Id);

            if (dataExercise == null || dataUser == null)
                return new List<SafetyWarning>();

            return await GetSafetyWarningsInternalAsync(dataExercise, dataUser);
        }

        // Internal implementations using Data.Entities types
        private async Task<SafetyValidationResult> ValidateExerciseForUserInternalAsync(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            try
            {
                _logger.LogInformation($"Validating exercise {exercise.Name} for user {user.Name}");

                // Convert Data.Entities to Core.Models for method compatibility
                var coreExercise = new Exercise
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    DifficultyLevel = exercise.DifficultyLevel.ToString()
                };

                var coreUser = new UserProfile
                {
                    Id = user.Id,
                    Name = user.Name,
                    Age = user.Age,
                    FitnessLevel = user.FitnessLevel
                };

                var result = new SafetyValidationResult();
                var safetyProfile = await BuildSafetyProfileAsync(user);

                // Check contraindications
                var contraindicationResult = await CheckContraindicationsAsync(coreExercise, safetyProfile.PhysicalLimitations);
                if (contraindicationResult.HasContraindications)
                {
                    result.IsSafe = false;
                    result.SafetyLevel = CoreSafetyLevel.Contraindicated;
                    result.Contraindications = contraindicationResult.IdentifiedContraindications;
                }
                else
                {
                    // Perform detailed safety analysis
                    result = await PerformDetailedSafetyAnalysisAsync(coreExercise, safetyProfile);
                }

                // Get AI-powered safety assessment (using original data entities)
                var aiAssessment = await GetAISafetyAssessmentAsync(exercise, user);
                result.SafetyNotes.AddRange(aiAssessment.SafetyNotes);

                // Calculate final risk score (using original data entities)
                result.RiskScore = CalculateOverallRiskScore(result, exercise, safetyProfile);

                // Generate summary
                result.Summary = GenerateSafetySummary(result);

                _logger.LogInformation($"Safety validation completed for {exercise.Name}: {result.SafetyLevel}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating exercise safety for {exercise.Name}");
                return new SafetyValidationResult
                {
                    IsSafe = false,
                    SafetyLevel = CoreSafetyLevel.High_Risk,
                    Warnings = new List<SafetyWarning>
                    {
                        new SafetyWarning
                        {
                            Category = "System_Error",
                            Severity = CoreSafetyLevel.High_Risk,
                            Message = "No se pudo completar la validación de seguridad",
                            Reason = ex.Message
                        }
                    }
                };
            }
        }

        private async Task<SafetyValidationResult> ValidateRoutineModificationInternalAsync(ExerciseModification modification, DataEntities.UserProfile user)
        {
            try
            {
                _logger.LogInformation($"Validating routine modification for user {user.Name}");

                var result = new SafetyValidationResult();

                // Get the exercise being modified
                DataEntities.Exercise? exercise = null;
                if (modification.ExerciseId.HasValue)
                {
                    exercise = await _exerciseRepository.GetByIdWithImagesAsync(modification.ExerciseId.Value);
                }

                if (exercise == null)
                {
                    result.IsSafe = false;
                    result.SafetyLevel = CoreSafetyLevel.High_Risk;
                    result.Warnings.Add(new SafetyWarning
                    {
                        Category = "Invalid_Exercise",
                        Severity = CoreSafetyLevel.High_Risk,
                        Message = "No se pudo identificar el ejercicio a modificar"
                    });
                    return result;
                }

                // Validate modification type
                result = await ValidateModificationTypeAsync(modification, exercise, user);

                // Check if modification creates new risks
                var newRisks = await AssessModificationRisksAsync(modification, exercise, user);
                result.Warnings.AddRange(newRisks);

                // Use AI to assess modification safety
                var aiAssessment = await GetAIModificationAssessmentAsync(modification, exercise, user);
                result.SafetyNotes.AddRange(aiAssessment);

                result.IsSafe = result.SafetyLevel <= CoreSafetyLevel.Moderate_Risk && !result.Contraindications.Any();

                _logger.LogInformation($"Modification validation completed: {result.SafetyLevel}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating routine modification");
                return new SafetyValidationResult
                {
                    IsSafe = false,
                    SafetyLevel = CoreSafetyLevel.High_Risk,
                    Summary = $"Error en validación: {ex.Message}"
                };
            }
        }

        public async Task<SafetyValidationResult> ValidateCompleteRoutineAsync(UserRoutine routine, UserProfile user)
        {
            try
            {
                _logger.LogInformation($"Validating complete routine {routine.Name} for user {user.Name}");

                // Get DataEntities.UserProfile for internal methods
                var dataUser = await _userRepository.GetByIdAsync(user.Id);
                if (dataUser == null)
                {
                    return new SafetyValidationResult
                    {
                        IsSafe = false,
                        SafetyLevel = CoreSafetyLevel.High_Risk,
                        Summary = "Usuario no encontrado"
                    };
                }

                var result = new SafetyValidationResult();
                var exercises = await ExtractExercisesFromRoutineAsync(routine);
                var safetyProfile = await BuildSafetyProfileAsync(dataUser);

                // Validate each exercise individually
                var exerciseValidations = new List<SafetyValidationResult>();
                foreach (var exercise in exercises)
                {
                    var exerciseResult = await ValidateExerciseForUserInternalAsync(exercise, dataUser);
                    exerciseValidations.Add(exerciseResult);
                }

                // Aggregate results
                result = AggregateValidationResults(exerciseValidations);

                // Check routine-level safety considerations
                var routineWarnings = await ValidateRoutineStructureAsync(exercises, safetyProfile);
                result.Warnings.AddRange(routineWarnings);

                // Check for muscle imbalances and overuse
                var balanceWarnings = await CheckMuscleBalanceAndOveruseAsync(exercises, dataUser);
                result.Warnings.AddRange(balanceWarnings);

                // Generate AI-powered routine assessment
                var aiRoutineAssessment = await GetAIRoutineAssessmentAsync(routine, dataUser);
                result.SafetyNotes.AddRange(aiRoutineAssessment);

                result.Summary = $"Rutina validada: {exercises.Count} ejercicios analizados";

                _logger.LogInformation($"Complete routine validation finished: {result.SafetyLevel}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating complete routine {routine.Id}");
                return new SafetyValidationResult
                {
                    IsSafe = false,
                    SafetyLevel = CoreSafetyLevel.High_Risk,
                    Summary = $"Error validando rutina: {ex.Message}"
                };
            }
        }

        public async Task<List<SafetyWarning>> GetExerciseWarningsAsync(Exercise exercise, UserProfile user)
        {
            try
            {
                // Fetch Data.Entities types
                var dataExercise = await _exerciseRepository.GetByIdWithImagesAsync(exercise.Id);
                var dataUser = await _userRepository.GetByIdAsync(user.Id);

                if (dataExercise == null || dataUser == null)
                    return new List<SafetyWarning>();

                var warnings = new List<SafetyWarning>();
                var safetyProfile = await BuildSafetyProfileAsync(dataUser);

                // Check age-related warnings
                warnings.AddRange(GetAgeRelatedWarnings(dataExercise, dataUser));

                // Check fitness level warnings
                warnings.AddRange(GetFitnessLevelWarnings(dataExercise, dataUser));

                // Check equipment safety
                warnings.AddRange(AssessEquipmentSafety(dataExercise, safetyProfile));

                // Check technique complexity warnings
                warnings.AddRange(GetTechniqueWarnings(dataExercise, dataUser));

                // Get AI-generated warnings
                var aiWarnings = await GetAIGeneratedWarningsAsync(dataExercise, dataUser);
                warnings.AddRange(aiWarnings);

                return warnings.Where(w => w.Severity >= CoreSafetyLevel.Low_Risk).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting exercise warnings for {exercise.Name}");
                return new List<SafetyWarning>
                {
                    new SafetyWarning
                    {
                        Category = "System_Error",
                        Severity = GymRoutineGenerator.Core.Models.SafetyLevel.Moderate_Risk,
                        Message = "No se pudieron generar advertencias de seguridad"
                    }
                };
            }
        }


        public async Task<bool> IsExerciseSafeForUserAsync(int exerciseId, int userId)
        {
            try
            {
                var exercise = await _exerciseRepository.GetByIdWithImagesAsync(exerciseId);
                var user = await _userRepository.GetByIdAsync(userId);

                if (exercise == null || user == null)
                    return false;

                var validation = await ValidateExerciseForUserInternalAsync(exercise, user);
                return validation.IsSafe && validation.SafetyLevel <= CoreSafetyLevel.Moderate_Risk;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking exercise safety for user {userId}, exercise {exerciseId}");
                return false; // Err on the side of caution
            }
        }

        public async Task<ContraindicationResult> CheckContraindicationsAsync(Exercise exercise, List<string> limitations)
        {
            try
            {
                var result = new ContraindicationResult();

                foreach (var limitation in limitations)
                {
                    var contraindicationKey = DetermineLimitationType(limitation);

                    if (_contraindicationRules.ContainsKey(contraindicationKey))
                    {
                        var contraindicatedExercises = _contraindicationRules[contraindicationKey];

                        if (contraindicatedExercises.Any(ce =>
                            exercise.Name.Contains(ce, StringComparison.OrdinalIgnoreCase) ||
                            exercise.Description?.Contains(ce, StringComparison.OrdinalIgnoreCase) == true))
                        {
                            var contraindicationMessage = $"Exercise {exercise.Name} is contraindicated for {limitation}";
                            result.Contraindications.Add(contraindicationMessage);
                        }
                    }
                }

                result.HasContraindications = result.Contraindications.Any();

                if (result.HasContraindications)
                {
                    result.RiskLevel = SafetyLevel.High_Risk;
                    result.Recommendations.Add($"Se identificaron {result.Contraindications.Count} contraindicaciones para este ejercicio");
                    result.Recommendations.Add("Consulte con un profesional médico antes de realizar este ejercicio");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking contraindications");
                return new ContraindicationResult
                {
                    HasContraindications = true,
                    Recommendations = new List<string> { "Error en verificación de contraindicaciones - se recomienda precaución" }
                };
            }
        }

        public async Task<SafetyValidationResult> ValidateProgressionSafetyAsync(Exercise currentExercise, Exercise nextExercise, UserProfile user)
        {
            try
            {
                var result = new SafetyValidationResult();

                // Convert difficulty levels to comparable values
                var currentLevel = ParseDifficultyLevel(currentExercise.DifficultyLevel);
                var nextLevel = ParseDifficultyLevel(nextExercise.DifficultyLevel);
                var difficultyGap = nextLevel - currentLevel;

                if (difficultyGap > 1)
                {
                    result.IsSafe = false;
                    result.SafetyLevel = CoreSafetyLevel.High_Risk;
                    result.Summary = "Salto de dificultad demasiado grande";
                    result.RiskScore = difficultyGap * 25; // 25% increase per difficulty level
                }
                else
                {
                    result.IsSafe = true;
                    result.SafetyLevel = CoreSafetyLevel.Safe;
                    result.Summary = "Progresión apropiada dentro del rango seguro";
                    result.RiskScore = difficultyGap * 10;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating progression safety");
                return new SafetyValidationResult
                {
                    IsSafe = false,
                    SafetyLevel = CoreSafetyLevel.High_Risk,
                    Summary = "Error en validación - se recomienda precaución"
                };
            }
        }

        private int ParseDifficultyLevel(string difficultyLevel)
        {
            return difficultyLevel?.ToLower() switch
            {
                "principiante" or "beginner" => 1,
                "intermedio" or "intermediate" => 2,
                "avanzado" or "advanced" => 3,
                _ => 1
            };
        }

        #region Private Helper Methods

        private async Task<SafetyProfile> BuildSafetyProfileAsync(UserProfile user)
        {
            var limitations = await _userRepository.GetUserPhysicalLimitationsAsync(user.Id);
            var equipment = await _userRepository.GetUserEquipmentPreferencesAsync(user.Id);

            var profile = new SafetyProfile
            {
                UserId = user.Id,
                PhysicalLimitations = limitations.Select(l => l.LimitationType).ToList(),
                ExperienceLevel = user.FitnessLevel,
                Age = user.Age,
                PreferredEquipment = equipment.Where(e => e.IsAvailable).Select(e => e.EquipmentType).ToList()
            };

            // Determine if medical clearance is required
            profile.RequiresMedicalClearance = DetermineMedicalClearanceRequirement(profile);

            return profile;
        }

        private async Task<SafetyProfile> BuildSafetyProfileAsync(DataEntities.UserProfile user)
        {
            var limitations = await _userRepository.GetUserPhysicalLimitationsAsync(user.Id);
            var equipment = await _userRepository.GetUserEquipmentPreferencesAsync(user.Id);

            var profile = new SafetyProfile
            {
                UserId = user.Id,
                PhysicalLimitations = limitations.Select(l => l.LimitationType).ToList(),
                ExperienceLevel = user.FitnessLevel,
                Age = user.Age,
                PreferredEquipment = equipment.Where(e => e.IsAvailable).Select(e => e.EquipmentType).ToList()
            };

            // Determine if medical clearance is required
            profile.RequiresMedicalClearance = DetermineMedicalClearanceRequirement(profile);

            return profile;
        }

        private async Task<SafetyValidationResult> PerformDetailedSafetyAnalysisAsync(Exercise exercise, SafetyProfile safetyProfile)
        {
            var result = new SafetyValidationResult();

            // Get Data.Entities exercise for internal methods
            var dataExercise = await _exerciseRepository.GetByIdWithImagesAsync(exercise.Id);
            if (dataExercise == null)
            {
                result.IsSafe = false;
                result.SafetyLevel = CoreSafetyLevel.High_Risk;
                return result;
            }

            // Assess exercise difficulty vs user experience
            var difficultyMatch = AssessExerciseDifficultyMatch(dataExercise, safetyProfile);
            if (difficultyMatch.RiskLevel > CoreSafetyLevel.Moderate_Risk)
            {
                result.Warnings.Add(new SafetyWarning
                {
                    Category = "Difficulty_Mismatch",
                    Severity = difficultyMatch.RiskLevel,
                    Message = difficultyMatch.Message,
                    MitigationStrategies = difficultyMatch.MitigationStrategies
                });
            }

            // Check equipment safety
            var equipmentSafety = AssessEquipmentSafety(dataExercise, safetyProfile);
            result.Warnings.AddRange(equipmentSafety);

            // Assess supervision requirements
            result.RequiresSupervision = DetermineSupervisionRequirement(dataExercise, safetyProfile);

            // Set overall safety level
            result.SafetyLevel = DetermineOverallSafetyLevel(result.Warnings);
            result.IsSafe = result.SafetyLevel <= CoreSafetyLevel.Moderate_Risk;

            return result;
        }

        private async Task<SafetyValidationResult> GetAISafetyAssessmentAsync(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            try
            {
                // Convert to Core.Models.UserProfile for prompt service
                var coreUser = new UserProfile
                {
                    Id = user.Id,
                    Name = user.Name,
                    Age = user.Age,
                    FitnessLevel = user.FitnessLevel
                };

                var prompt = await _promptService.BuildSafetyValidationPromptAsync(
                    new ExerciseModification { ExerciseId = exercise.Id }, coreUser);

                var aiResponse = await _ollamaService.GenerateResponseAsync(prompt);
                return ParseAISafetyResponse(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI safety assessment");
                return new SafetyValidationResult
                {
                    SafetyNotes = new List<string> { "No se pudo obtener evaluación de IA" }
                };
            }
        }

        private SafetyValidationResult ParseAISafetyResponse(string aiResponse)
        {
            // Parse AI response - in a real implementation, this would be more sophisticated
            var result = new SafetyValidationResult();

            if (aiResponse.Contains("seguro", StringComparison.OrdinalIgnoreCase))
            {
                result.SafetyLevel = CoreSafetyLevel.Safe;
                result.IsSafe = true;
            }
            else if (aiResponse.Contains("precaucion", StringComparison.OrdinalIgnoreCase))
            {
                result.SafetyLevel = CoreSafetyLevel.Moderate_Risk;
                result.IsSafe = true;
            }
            else
            {
                result.SafetyLevel = CoreSafetyLevel.High_Risk;
                result.IsSafe = false;
            }

            result.SafetyNotes.Add(aiResponse);
            return result;
        }

        private float CalculateOverallRiskScore(SafetyValidationResult result, DataEntities.Exercise exercise, SafetyProfile safetyProfile)
        {
            float baseRisk = 10; // Base risk score

            // Add risk based on difficulty
            baseRisk += GetDifficultyValue(exercise.DifficultyLevel.ToString()) * 10;

            // Add risk based on warnings
            foreach (var warning in result.Warnings)
            {
                baseRisk += (int)warning.Severity * 5;
            }

            // Add risk based on contraindications
            baseRisk += result.Contraindications.Count * 20;

            // Apply user-specific risk modifiers
            if (safetyProfile.ExperienceLevel == "Principiante")
                baseRisk *= 1.5f;

            if (safetyProfile.PhysicalLimitations.Any())
                baseRisk *= 1.3f;

            return Math.Min(baseRisk, 100); // Cap at 100
        }

        private string GenerateSafetySummary(SafetyValidationResult result)
        {
            if (result.IsSafe)
            {
                return result.Warnings.Any()
                    ? "Ejercicio seguro con precauciones menores"
                    : "Ejercicio seguro para el usuario";
            }
            else
            {
                return result.Contraindications.Any()
                    ? "Ejercicio contraindicado para el usuario"
                    : "Ejercicio presenta riesgos significativos";
            }
        }

        // Additional helper methods
        private string DetermineLimitationType(string limitation)
        {
            return limitation.ToLowerInvariant() switch
            {
                "rodilla" or "knee" => "Knee_Problems",
                "espalda" or "back" => "Back_Problems",
                "hombro" or "shoulder" => "Shoulder_Injury",
                "corazón" or "heart" or "cardíaco" => "Heart_Condition",
                "embarazo" or "pregnancy" => "Pregnancy",
                "osteoporosis" => "Osteoporosis",
                _ => "General_Limitation"
            };
        }

        private string DetermineExperienceLevel(string fitnessLevel)
        {
            return fitnessLevel;
        }

        private bool DetermineMedicalClearanceRequirement(SafetyProfile profile)
        {
            return profile.PhysicalLimitations.Any(l => l.Contains("severe", StringComparison.OrdinalIgnoreCase)) ||
                   profile.Age >= 65 ||
                   profile.AllergiesAndConditions.Any();
        }

        // Placeholder implementations for complex methods
        private SafetyWarning AssessExerciseDifficultyMatch(DataEntities.Exercise exercise, SafetyProfile safetyProfile)
        {
            return new SafetyWarning
            {
                Category = "Difficulty_Assessment",
                Severity = GymRoutineGenerator.Core.Models.SafetyLevel.Low_Risk,
                Message = "Dificultad apropiada para el usuario",
                Mitigation = new List<string> { "Comenzar con peso ligero", "Enfocar en técnica" }
            };
        }

        private List<SafetyWarning> AssessEquipmentSafety(DataEntities.Exercise exercise, SafetyProfile safetyProfile)
        {
            return new List<SafetyWarning>();
        }

        private bool DetermineSupervisionRequirement(DataEntities.Exercise exercise, SafetyProfile safetyProfile)
        {
            var exerciseDifficultyValue = GetDifficultyValue(exercise.DifficultyLevel.ToString());
            return exerciseDifficultyValue >= 3 || // 3 = Advanced
                   safetyProfile.ExperienceLevel == "Principiante";
        }

        private CoreSafetyLevel DetermineOverallSafetyLevel(List<SafetyWarning> warnings)
        {
            if (!warnings.Any()) return GymRoutineGenerator.Core.Models.SafetyLevel.Safe;
            return warnings.Max(w => w.Severity);
        }

        // Additional placeholder methods
        private async Task<List<DataEntities.Exercise>> ExtractExercisesFromRoutineAsync(UserRoutine routine) { return new List<DataEntities.Exercise>(); }
        private async Task<SafetyValidationResult> ValidateModificationTypeAsync(ExerciseModification modification, DataEntities.Exercise exercise, DataEntities.UserProfile user) { return new SafetyValidationResult(); }
        private async Task<List<SafetyWarning>> AssessModificationRisksAsync(ExerciseModification modification, DataEntities.Exercise exercise, DataEntities.UserProfile user) { return new List<SafetyWarning>(); }
        private async Task<List<string>> GetAIModificationAssessmentAsync(ExerciseModification modification, DataEntities.Exercise exercise, DataEntities.UserProfile user) { return new List<string>(); }
        private SafetyValidationResult AggregateValidationResults(List<SafetyValidationResult> results) { return new SafetyValidationResult(); }
        private async Task<List<SafetyWarning>> ValidateRoutineStructureAsync(List<DataEntities.Exercise> exercises, SafetyProfile safetyProfile) { return new List<SafetyWarning>(); }
        private async Task<List<SafetyWarning>> CheckMuscleBalanceAndOveruseAsync(List<DataEntities.Exercise> exercises, DataEntities.UserProfile user) { return new List<SafetyWarning>(); }
        private async Task<List<string>> GetAIRoutineAssessmentAsync(UserRoutine routine, DataEntities.UserProfile user) { return new List<string>(); }

        private async Task<List<SafetyWarning>> GetSafetyWarningsInternalAsync(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            try
            {
                var validationResult = await ValidateExerciseForUserInternalAsync(exercise, user);
                return validationResult.Warnings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting safety warnings for exercise {exercise.Name}");
                return new List<SafetyWarning>
                {
                    new SafetyWarning
                    {
                        Severity = GymRoutineGenerator.Core.Models.SafetyLevel.Moderate_Risk,
                        Message = "No se pudieron obtener las advertencias de seguridad",
                        Category = "System Error"
                    }
                };
            }
        }

        public async Task<List<string>> GetSafetyRecommendationsAsync(UserProfile user)
        {
            try
            {
                var safetyProfile = await BuildSafetyProfileAsync(user);
                var recommendations = new List<string>();

                // General recommendations based on profile
                if (safetyProfile.PhysicalLimitations.Any())
                {
                    recommendations.Add("Considere ejercicios de bajo impacto debido a limitaciones físicas");
                    recommendations.Add("Consulte con un profesional de la salud antes de comenzar");
                }

                if (safetyProfile.ExperienceLevel == "Principiante")
                {
                    recommendations.Add("Comience con ejercicios básicos y progrese gradualmente");
                    recommendations.Add("Considere trabajar con un entrenador personal");
                }

                if (safetyProfile.Age > 50)
                {
                    recommendations.Add("Incluya ejercicios de flexibilidad y equilibrio");
                    recommendations.Add("Priorice el calentamiento y enfriamiento");
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting safety recommendations for user {user.Id}");
                return new List<string> { "No se pudieron obtener recomendaciones de seguridad" };
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

        private List<SafetyWarning> GetAgeRelatedWarnings(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            var warnings = new List<SafetyWarning>();
            if (user.Age > 60 && exercise.DifficultyLevel.ToString().ToLower() == "avanzado")
            {
                warnings.Add(new SafetyWarning
                {
                    Message = "Ejercicio avanzado - considere modificaciones para edad",
                    Category = "Age_Related",
                    Severity = CoreSafetyLevel.Caution
                });
            }
            return warnings;
        }

        private List<SafetyWarning> GetFitnessLevelWarnings(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            var warnings = new List<SafetyWarning>();
            if (user.FitnessLevel == "Principiante" && exercise.DifficultyLevel.ToString().ToLower() == "avanzado")
            {
                warnings.Add(new SafetyWarning
                {
                    Message = "Ejercicio muy avanzado para nivel principiante",
                    Category = "Fitness_Level",
                    Severity = CoreSafetyLevel.Moderate_Risk
                });
            }
            return warnings;
        }

        private List<SafetyWarning> GetTechniqueWarnings(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            var warnings = new List<SafetyWarning>();
            if (exercise.Name.Contains("Sentadilla", StringComparison.OrdinalIgnoreCase) && user.FitnessLevel == "Principiante")
            {
                warnings.Add(new SafetyWarning
                {
                    Message = "Asegúrese de mantener técnica correcta - considere supervisión",
                    Category = "Technique",
                    Severity = CoreSafetyLevel.Caution
                });
            }
            return warnings;
        }

        private async Task<List<SafetyWarning>> GetAIGeneratedWarningsAsync(DataEntities.Exercise exercise, DataEntities.UserProfile user)
        {
            // Placeholder - in real implementation, would use AI to generate warnings
            await Task.CompletedTask;
            return new List<SafetyWarning>();
        }

        #endregion
    }
}