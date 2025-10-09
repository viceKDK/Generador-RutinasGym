using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Data.Repositories;
using GymRoutineGenerator.Infrastructure.Mapping;
using System.Text.Json;
using System.Text.RegularExpressions;
using DataEntities = GymRoutineGenerator.Data.Entities;
using CoreModels = GymRoutineGenerator.Core.Models;
using ServiceImageRecognitionResult = GymRoutineGenerator.Core.Services.ImageRecognitionResult;

namespace GymRoutineGenerator.Infrastructure.AI
{
    public class ImageRecognitionService : IImageRecognitionService
    {
        private readonly IOllamaService _ollamaService;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<ImageRecognitionService> _logger;

        // Exercise patterns for image recognition
        private static readonly Dictionary<string, List<string>> _exercisePatterns = new()
        {
            ["push_up"] = new() { "flexion", "planca", "push", "empuje", "brazos extendidos" },
            ["squat"] = new() { "sentadilla", "agachado", "piernas flexionadas", "cuclillas" },
            ["deadlift"] = new() { "peso muerto", "barra", "levantamiento", "espalda recta" },
            ["bench_press"] = new() { "press banca", "acostado", "barra", "pecho" },
            ["pull_up"] = new() { "dominada", "colgado", "barra", "tirón" },
            ["bicep_curl"] = new() { "curl", "biceps", "mancuerna", "flexion brazo" },
            ["shoulder_press"] = new() { "press hombro", "militar", "mancuerna arriba" },
            ["lunge"] = new() { "zancada", "paso adelante", "pierna adelante" },
            ["plank"] = new() { "plancha", "tabla", "codos", "abdomen" },
            ["row"] = new() { "remo", "tirón", "espalda", "codos atrás" }
        };

        private static readonly Dictionary<string, string> _equipmentPatterns = new()
        {
            ["barbell"] = "barra",
            ["dumbbell"] = "mancuerna",
            ["kettlebell"] = "pesa rusa",
            ["resistance_band"] = "banda elástica",
            ["cable_machine"] = "máquina de poleas",
            ["bench"] = "banco",
            ["pull_up_bar"] = "barra dominadas",
            ["treadmill"] = "cinta correr",
            ["exercise_bike"] = "bicicleta estática"
        };

        public ImageRecognitionService(
            IOllamaService ollamaService,
            IExerciseRepository exerciseRepository,
            ILogger<ImageRecognitionService> logger)
        {
            _ollamaService = ollamaService;
            _exerciseRepository = exerciseRepository;
            _logger = logger;
        }

        public async Task<ServiceImageRecognitionResult> AnalyzeExerciseImageAsync(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                {
                    return new ServiceImageRecognitionResult
                    {
                        ExplanationMessage = "No se proporcionó imagen válida",
                        Confidence = 0
                    };
                }

                _logger.LogInformation($"Starting image analysis for {imageData.Length} bytes");

                // Basic image validation
                var isValid = await IsValidExerciseImageAsync(imageData);
                if (!isValid)
                {
                    return new ServiceImageRecognitionResult
                    {
                        ExplanationMessage = "La imagen no parece contener un ejercicio válido",
                        Confidence = 0
                    };
                }

                // Use AI vision to analyze the image
                var analysisPrompt = @"
Analiza esta imagen de ejercicio y proporciona un análisis detallado.

Identifica:
1. ¿Qué ejercicio específico se está realizando?
2. ¿Qué grupos musculares están siendo trabajados?
3. ¿Qué equipamiento se está utilizando?
4. ¿Cuál es la fase del movimiento mostrada?
5. ¿La técnica parece correcta?
6. ¿Qué nivel de dificultad representa?

Responde en formato JSON:
{
  ""exerciseName"": ""nombre específico del ejercicio"",
  ""confidence"": 0.85,
  ""muscleGroups"": [""lista de grupos musculares""],
  ""equipment"": [""equipamiento visible""],
  ""movementType"": ""tipo de movimiento"",
  ""technique"": {
    ""rating"": ""excellent|good|needs_improvement|poor"",
    ""notes"": [""observaciones sobre la técnica""]
  },
  ""difficulty"": ""beginner|intermediate|advanced"",
  ""bodyPosition"": ""descripción de la posición corporal"",
  ""environment"": ""gym|home|outdoor"",
  ""alternatives"": [""ejercicios similares sugeridos""],
  ""explanation"": ""explicación detallada de lo observado""
}

Sé específico y detallado en tu análisis.";

                // Note: This would require a vision-capable model like LLaVA
                // For now, we'll simulate the analysis using text-based patterns
                var result = await SimulateImageAnalysis(imageData, analysisPrompt);

                _logger.LogInformation($"Image analysis completed with confidence: {result.Confidence}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing exercise image");
                return new ServiceImageRecognitionResult
                {
                    ExplanationMessage = $"Error al analizar la imagen: {ex.Message}",
                    Confidence = 0
                };
            }
        }

        public async Task<List<Exercise>> FindExercisesByImageSimilarityAsync(byte[] imageData)
        {
            try
            {
                // First analyze the image to get exercise characteristics
                var analysis = await AnalyzeExerciseImageAsync(imageData);

                if (analysis.Confidence < 0.3f)
                {
                    _logger.LogWarning("Low confidence in image analysis, returning empty results");
                    return new List<Exercise>();
                }

                // Search for exercises matching the identified characteristics
                var allDataExercises = await _exerciseRepository.GetAllAsync();
                var allExercises = allDataExercises.Select(MapToCore).ToList();
                var similarExercises = new List<(Exercise exercise, float similarity)>();

                foreach (var exercise in allExercises)
                {
                    var similarity = CalculateExerciseSimilarity(exercise, analysis);
                    if (similarity > 0.4f)
                    {
                        similarExercises.Add((exercise, similarity));
                    }
                }

                var result = similarExercises
                    .OrderByDescending(pair => pair.similarity)
                    .Take(8)
                    .Select(pair => pair.exercise)
                    .ToList();

                _logger.LogInformation($"Found {result.Count} similar exercises based on image analysis");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding exercises by image similarity");
                return new List<Exercise>();
            }
        }

        public async Task<ExerciseClassificationResult> ClassifyExerciseTypeAsync(byte[] imageData)
        {
            try
            {
                var analysis = await AnalyzeExerciseImageAsync(imageData);

                var classification = new ExerciseClassificationResult
                {
                    Category = DetermineExerciseCategory(analysis).ToString(),
                    CategoryConfidence = analysis.Confidence,
                    EstimatedDifficulty = ParseDifficultyFromAnalysis(analysis),
                    RequiredSkills = ExtractRequiredSkills(analysis),
                    SafetyConsiderations = AssessSafetyConsiderations(analysis).Select(sc => sc.ToString()).ToList(),
                    RequiresSpotter = DetermineSpotterRequirement(analysis),
                    RecommendedEnvironment = DetermineRecommendedEnvironment(analysis).ToString()
                };

                _logger.LogInformation($"Classified exercise as {classification.Category} with {classification.CategoryConfidence:P0} confidence");
                return classification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying exercise type");
                return new ExerciseClassificationResult
                {
                    Category = GymRoutineGenerator.Core.Models.ExerciseCategory.Strength.ToString(),
                    CategoryConfidence = 0
                };
            }
        }

        public async Task<bool> IsValidExerciseImageAsync(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return false;

                // Basic file format validation
                if (!IsValidImageFormat(imageData))
                    return false;

                // Check minimum file size (too small images are likely invalid)
                if (imageData.Length < 1024) // 1KB minimum
                    return false;

                // Check maximum file size (too large files might be problematic)
                if (imageData.Length > 10 * 1024 * 1024) // 10MB maximum
                    return false;

                // Additional validation could include:
                // - Image dimensions check
                // - Content validation using AI
                // - Format-specific validation

                _logger.LogInformation($"Image validation passed for {imageData.Length} bytes");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image");
                return false;
            }
        }

        public async Task<ImageAnalysisResult> GetDetailedImageAnalysisAsync(byte[] imageData)
        {
            try
            {
                var result = new ImageAnalysisResult();

                // Analyze image quality
                result.Quality = await AnalyzeImageQuality(imageData);

                // Detect person in image
                result.PersonDetection = await AnalyzePersonDetection(imageData);

                // Detect equipment
                result.EquipmentDetection = await AnalyzeEquipmentDetection(imageData);

                // Analyze movement
                result.MovementAnalysis = await AnalyzeMovement(imageData);

                // Analyze environment
                result.EnvironmentAnalysis = await AnalyzeEnvironment(imageData);

                result.ProcessingNotes.Add($"Análisis completado en {DateTime.UtcNow:HH:mm:ss}");

                _logger.LogInformation("Detailed image analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in detailed image analysis");
                return new ImageAnalysisResult
                {
                    ProcessingNotes = new List<string> { $"Error en análisis: {ex.Message}" }
                };
            }
        }

        public async Task<List<string>> ExtractEquipmentFromImageAsync(byte[] imageData)
        {
            try
            {
                var equipmentPrompt = @"
Identifica todo el equipamiento de gimnasio visible en esta imagen.

Lista específicamente:
1. Pesas (mancuernas, barras, discos)
2. Máquinas de ejercicio
3. Accesorios (bandas, cables, etc.)
4. Mobiliario (bancos, racks, etc.)

Responde solo con una lista de equipamiento en español, uno por línea.
Si no ves equipamiento, responde 'Sin equipamiento visible'.";

                // This would use vision model - for now simulate
                var detectedEquipment = await SimulateEquipmentDetection(imageData);

                _logger.LogInformation($"Extracted {detectedEquipment.Count} equipment items from image");
                return detectedEquipment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting equipment from image");
                return new List<string> { "Error al detectar equipamiento" };
            }
        }

        public async Task<BodyPositionAnalysis> AnalyzeBodyPositionAsync(byte[] imageData)
        {
            try
            {
                var positionPrompt = @"
Analiza la posición corporal en esta imagen de ejercicio.

Evalúa:
1. Posición principal del cuerpo (de pie, sentado, acostado, etc.)
2. Alineación de la columna vertebral
3. Posición de las articulaciones principales
4. Distribución del peso
5. Calidad de la forma/técnica

Responde en formato JSON con evaluación detallada de la postura.";

                var jointsList = await AnalyzeJointPositions(imageData);
                var analysis = new BodyPositionAnalysis
                {
                    PrimaryPosition = await DeterminePrimaryPosition(imageData),
                    JointPositions = jointsList.ToDictionary(j => j.JointName, j => j.Angle),
                    SpinalAlignment = await AnalyzeSpinalAlignment(imageData),
                    WeightDistribution = await AnalyzeWeightDistribution(imageData),
                    FormAssessment = string.Join("; ", await AssessExerciseForm(imageData))
                };

                _logger.LogInformation("Body position analysis completed");
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing body position");
                return new BodyPositionAnalysis
                {
                    PrimaryPosition = "Indeterminado",
                    FormAssessment = $"Error en análisis: {ex.Message}"
                };
            }
        }

        #region Private Helper Methods

        private async Task<ServiceImageRecognitionResult> SimulateImageAnalysis(byte[] imageData, string prompt)
        {
            // This is a simulation since we don't have actual vision model integration
            // In a real implementation, this would send the image to a vision model

            var result = new ServiceImageRecognitionResult
            {
                ExerciseName = "Análisis de imagen (simulado)",
                Confidence = 0.7f,
                ExplanationMessage = "Esta es una simulación del análisis de imagen. Para análisis real se requiere integración con modelo de visión como LLaVA."
            };

            // Simulate some basic analysis based on image size and format
            if (imageData.Length > 100000) // Larger images might be higher quality
            {
                result.Confidence += 0.1f;
                result.IdentifiedMuscleGroups.Add("Músculos principales");
                result.IdentifiedEquipment.Add("Equipamiento detectado");
            }

            // Add some generic alternatives
            result.AlternativeInterpretations.AddRange(new[]
            {
                "Ejercicio de fuerza",
                "Ejercicio cardiovascular",
                "Ejercicio de flexibilidad"
            });

            // Quality assessment
            result.Quality = new ImageQuality
            {
                Clarity = 0.8f,
                Lighting = 0.7f,
                Composition = 0.6f,
                IsPersonVisible = true,
                IsEquipmentVisible = imageData.Length > 50000,
                OverallScore = 0.7f
            };

            return result;
        }

        private bool IsValidImageFormat(byte[] imageData)
        {
            if (imageData.Length < 4)
                return false;

            // Check for common image file signatures
            var header = imageData.Take(4).ToArray();

            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8)
                return true;

            // PNG
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return true;

            // GIF
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46)
                return true;

            // BMP
            if (header[0] == 0x42 && header[1] == 0x4D)
                return true;

            return false;
        }

        private float CalculateExerciseSimilarity(Exercise exercise, ServiceImageRecognitionResult analysis)
        {
            float similarity = 0;

            // Check if exercise name matches
            if (!string.IsNullOrEmpty(analysis.ExerciseName))
            {
                if (exercise.Name.Contains(analysis.ExerciseName, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 0.5f;
                }
            }

            // Check muscle groups
            foreach (var muscleGroup in analysis.IdentifiedMuscleGroups)
            {
                if (exercise.PrimaryMuscleGroup.Contains(muscleGroup, StringComparison.OrdinalIgnoreCase))
                {
                    similarity += 0.3f;
                }
            }

            // Check equipment
            foreach (var equipment in analysis.IdentifiedEquipment)
            {
                if (exercise.RequiredEquipment.Any(e => e.Equals(equipment, StringComparison.OrdinalIgnoreCase)))
                {
                    similarity += 0.2f;
                }
            }

            return Math.Min(similarity, 1.0f);
        }

        private CoreModels.ExerciseCategory DetermineExerciseCategory(ServiceImageRecognitionResult analysis)
        {
            // Simple heuristics based on analysis
            if (analysis.IdentifiedEquipment.Any(e => e.Contains("barra") || e.Contains("mancuerna")))
            {
                return CoreModels.ExerciseCategory.Compound;
            }

            if (analysis.EstimatedMovementType == MovementType.Explosive)
            {
                return CoreModels.ExerciseCategory.Cardio;
            }

            if (analysis.EstimatedMovementType == MovementType.Isometric)
            {
                return CoreModels.ExerciseCategory.Core;
            }

            return CoreModels.ExerciseCategory.Accessory;
        }

        private DifficultyLevel ParseDifficultyFromAnalysis(ServiceImageRecognitionResult analysis)
        {
            // Determine difficulty based on equipment and movement complexity
            if (analysis.IdentifiedEquipment.Count > 2)
                return DifficultyLevel.Advanced;

            if (analysis.IdentifiedEquipment.Any(e => e.Contains("máquina")))
                return DifficultyLevel.Intermediate;

            return DifficultyLevel.Beginner;
        }

        private List<string> ExtractRequiredSkills(ServiceImageRecognitionResult analysis)
        {
            var skills = new List<string>();

            if (analysis.EstimatedMovementType == MovementType.Pull)
                skills.Add("Fuerza de tracción");

            if (analysis.EstimatedMovementType == MovementType.Push)
                skills.Add("Fuerza de empuje");

            if (analysis.IdentifiedEquipment.Any(e => e.Contains("barra")))
                skills.Add("Estabilización central");

            return skills;
        }

        private List<CoreModels.SafetyConsideration> AssessSafetyConsiderations(ServiceImageRecognitionResult analysis)
        {
            var considerations = new List<CoreModels.SafetyConsideration>();

            if (analysis.IdentifiedEquipment.Any(e => e.Contains("barra") && e.Contains("peso")))
            {
                considerations.Add(new CoreModels.SafetyConsideration
                {
                    Consideration = "Uso de peso libre",
                    Severity = CoreModels.SafetyLevel.Warning,
                    Precautions = new List<string> { "Usar observador", "Comenzar con peso ligero" },
                    WarningSignsToStop = new List<string> { "Dolor agudo", "Pérdida de control" }
                });
            }

            return considerations;
        }

        private bool DetermineSpotterRequirement(ServiceImageRecognitionResult analysis)
        {
            return analysis.IdentifiedEquipment.Any(e =>
                e.Contains("barra") || e.Contains("press") || e.Contains("sentadilla"));
        }

        private CoreModels.EnvironmentType DetermineRecommendedEnvironment(ServiceImageRecognitionResult analysis)
        {
            if (analysis.IdentifiedEquipment.Count > 3)
                return CoreModels.EnvironmentType.Gym;

            if (analysis.IdentifiedEquipment.Any(e => e.Contains("máquina")))
                return CoreModels.EnvironmentType.Gym;

            return CoreModels.EnvironmentType.Home;
        }

        private async Task<ImageQuality> AnalyzeImageQuality(byte[] imageData)
        {
            return new ImageQuality
            {
                Clarity = CalculateImageClarity(imageData),
                Lighting = 0.7f, // Simulated
                Composition = 0.6f, // Simulated
                IsPersonVisible = imageData.Length > 50000, // Heuristic
                IsEquipmentVisible = imageData.Length > 30000, // Heuristic
                OverallScore = 0.7f,
                QualityIssues = imageData.Length < 30000 ? new List<string> { "Imagen pequeña" } : new List<string>()
            };
        }

        private async Task<PersonDetection> AnalyzePersonDetection(byte[] imageData)
        {
            return new PersonDetection
            {
                IsPersonDetected = imageData.Length > 20000, // Heuristic
                NumberOfPeople = 1,
                DetectedBodyParts = new List<string> { "Torso", "Brazos", "Piernas" },
                EstimatedGender = "Indeterminado",
                EstimatedFitnessLevel = "Intermedio"
            };
        }

        private async Task<EquipmentDetection> AnalyzeEquipmentDetection(byte[] imageData)
        {
            var equipment = await SimulateEquipmentDetection(imageData);

            return new EquipmentDetection
            {
                Equipment = equipment,
                EnvironmentType = equipment.Count > 2 ? GymEnvironmentType.Commercial.ToString() : GymEnvironmentType.Home.ToString(),
                IsHomeGym = equipment.Count <= 2,
                IsCommercialGym = equipment.Count > 2
            };
        }

        private async Task<MovementAnalysis> AnalyzeMovement(byte[] imageData)
        {
            return new MovementAnalysis
            {
                Phase = "Peak",
                PrimaryPlane = MovementPlane.Sagittal.ToString(),
                SecondaryPlanes = new List<string> { MovementPlane.Frontal.ToString() },
                IsStaticHold = false,
                IsDynamicMovement = true,
                EstimatedIntensity = 0.6f
            };
        }

        private async Task<EnvironmentAnalysis> AnalyzeEnvironment(byte[] imageData)
        {
            return new EnvironmentAnalysis
            {
                Location = LocationType.IndoorGym.ToString(),
                VisibleEquipment = await SimulateEquipmentDetection(imageData),
                SafetyFeatures = new List<string> { "Espejo", "Superficie antideslizante" },
                SpaceAvailability = "Amplio",
                Lighting = LightingConditions.Good.ToString()
            };
        }

        private async Task<List<string>> SimulateEquipmentDetection(byte[] imageData)
        {
            // Simulate equipment detection based on image characteristics
            var equipment = new List<string>();

            if (imageData.Length > 100000)
            {
                equipment.Add("Mancuernas");
                equipment.Add("Banco de ejercicio");
            }

            if (imageData.Length > 200000)
            {
                equipment.Add("Barra olímpica");
                equipment.Add("Rack de potencia");
            }

            if (imageData.Length > 50000)
            {
                equipment.Add("Colchoneta");
            }

            return equipment;
        }

        private float CalculateImageClarity(byte[] imageData)
        {
            // Simple heuristic based on file size
            if (imageData.Length > 500000) return 0.9f;
            if (imageData.Length > 200000) return 0.8f;
            if (imageData.Length > 100000) return 0.7f;
            if (imageData.Length > 50000) return 0.6f;
            return 0.5f;
        }

        private async Task<string> DeterminePrimaryPosition(byte[] imageData)
        {
            // Simulate position detection
            var positions = new[] { "De pie", "Acostado", "Sentado", "En cuclillas", "Plancha" };
            var random = new Random();
            return positions[random.Next(positions.Length)];
        }

        private async Task<List<JointPosition>> AnalyzeJointPositions(byte[] imageData)
        {
            return new List<JointPosition>
            {
                new JointPosition
                {
                    JointName = "Rodilla",
                    Angle = 90,
                    Position = "Flexionada",
                    IsOptimal = true,
                    Notes = "Posición correcta para ejercicio"
                },
                new JointPosition
                {
                    JointName = "Cadera",
                    Angle = 45,
                    Position = "Flexionada",
                    IsOptimal = true,
                    Notes = "Alineación adecuada"
                }
            };
        }

        private async Task<SpinalAlignment> AnalyzeSpinalAlignment(byte[] imageData)
        {
            return new SpinalAlignment
            {
                IsNeutral = true,
                CurvatureAssessment = "Curvatura natural conservada",
                Issues = new List<string>(),
                AlignmentScore = 0.85f
            };
        }

        private async Task<WeightDistribution> AnalyzeWeightDistribution(byte[] imageData)
        {
            return new WeightDistribution
            {
                Distribution = "Equilibrada",
                BalanceScore = 0.8f,
                LeftPercentage = 50f,
                RightPercentage = 50f,
                IsBalanced = true,
                DistributionScore = 0.8f,
                Recommendations = new List<string> { "Distribución uniforme del peso corporal" }
            };
        }

        private async Task<List<string>> AssessExerciseForm(byte[] imageData)
        {
            return new List<string>
            {
                "Postura general: Buena",
                "Alineación espinal: Correcta",
                "Posición de extremidades: Adecuada",
                "Recomendación: Mantener la técnica"
            };
        }

        // Interface implementations
        public async Task<List<Exercise>> FindSimilarExercisesAsync(byte[] imageData)
        {
            try
            {
                var classification = await ClassifyExerciseFromImageAsync(imageData);
                if (classification.IsValidExercise)
                {
                    var entities = await _exerciseRepository.GetByMuscleGroupAsync(classification.MuscleGroup);
                    return entities.ToModelList();
                }
                return new List<Exercise>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar exercises from image");
                return new List<Exercise>();
            }
        }

        public async Task<Exercise?> IdentifyExerciseAsync(string imagePath)
        {
            try
            {
                var imageData = await File.ReadAllBytesAsync(imagePath);
                var classification = await ClassifyExerciseFromImageAsync(imageData);

                if (classification.IsValidExercise && !string.IsNullOrEmpty(classification.ExerciseName))
                {
                    // Try to find the exercise in database
                    var exercises = await _exerciseRepository.GetByMuscleGroupAsync(classification.MuscleGroup);
                    return exercises.ToModelList().FirstOrDefault(e => e.Name.Contains(classification.ExerciseName, StringComparison.OrdinalIgnoreCase));
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error identifying exercise from image path: {imagePath}");
                return null;
            }
        }

        public async Task<bool> ValidateExerciseImageAsync(byte[] imageData)
        {
            try
            {
                var analysis = await AnalyzeImageQualityAsync(imageData);
                return analysis.QualityRating != "Poor" && !analysis.IsBlurry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating exercise image");
                return false;
            }
        }

        public async Task<List<string>> ExtractExerciseFeaturesAsync(byte[] imageData)
        {
            try
            {
                var classification = await ClassifyExerciseFromImageAsync(imageData);
                var analysis = await AnalyzeImageQualityAsync(imageData);

                var features = new List<string>
                {
                    $"Exercise: {classification.ExerciseName}",
                    $"Muscle Group: {classification.MuscleGroup}",
                    $"Confidence: {classification.Confidence:F2}",
                    $"Difficulty: {classification.EstimatedDifficulty}",
                    $"Image Quality: {analysis.QualityRating}",
                    $"Valid: {classification.IsValidExercise}"
                };

                features.AddRange(classification.DetectedEquipment.Select(eq => $"Equipment: {eq}"));
                return features;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting exercise features");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }

        public async Task<ExerciseClassification> ClassifyExerciseFromImageAsync(byte[] imageData)
        {
            try
            {
                var analysis = await AnalyzeExerciseImageAsync(imageData);

                return new ExerciseClassification
                {
                    ExerciseName = analysis.ExerciseName ?? "Ejercicio no identificado",
                    MuscleGroup = analysis.IdentifiedMuscleGroups.FirstOrDefault() ?? "Músculos principales",
                    Confidence = analysis.Confidence,
                    IsValidExercise = analysis.Confidence > 0.3f,
                    EstimatedDifficulty = ParseDifficultyFromAnalysis(analysis).ToString(),
                    DetectedEquipment = analysis.IdentifiedEquipment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying exercise from image");
                return new ExerciseClassification
                {
                    ExerciseName = "Error",
                    MuscleGroup = "Desconocido",
                    Confidence = 0,
                    IsValidExercise = false,
                    EstimatedDifficulty = "Beginner",
                    DetectedEquipment = new List<string>()
                };
            }
        }

        public async Task<ImageQualityAssessment> AnalyzeImageQualityAsync(byte[] imageData)
        {
            try
            {
                var quality = await AnalyzeImageQuality(imageData);

                return new ImageQualityAssessment
                {
                    QualityRating = quality.OverallScore > 0.8f ? "Excellent" :
                                   quality.OverallScore > 0.6f ? "Good" :
                                   quality.OverallScore > 0.4f ? "Fair" : "Poor",
                    IsBlurry = quality.Clarity < 0.5f,
                    HasProperLighting = quality.Lighting > 0.6f,
                    IsPersonVisible = quality.IsPersonVisible,
                    OverallScore = quality.OverallScore,
                    Issues = new List<string>() // ImageQuality doesn't have QualityIssues
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image quality");
                return new ImageQualityAssessment
                {
                    QualityRating = "Poor",
                    IsBlurry = true,
                    HasProperLighting = false,
                    IsPersonVisible = false,
                    OverallScore = 0,
                    Issues = new List<string> { $"Error: {ex.Message}" }
                };
            }
        }

        #endregion

        // Mapping methods for type conversion
        private Exercise MapToCore(GymRoutineGenerator.Data.Entities.Exercise dataExercise)
        {
            return new Exercise
            {
                Id = dataExercise.Id,
                Name = dataExercise.Name,
                SpanishName = dataExercise.SpanishName,
                Description = dataExercise.Description,
                Instructions = dataExercise.Instructions,
                MuscleGroups = new List<string> { dataExercise.PrimaryMuscleGroup?.Name ?? "Unknown" },
                Equipment = dataExercise.EquipmentType?.Name ?? "Unknown",
                DifficultyLevel = dataExercise.DifficultyLevel.ToString(),
                ExerciseType = MapExerciseType(dataExercise.ExerciseType),
                Type = dataExercise.ExerciseType.ToString(),
                Tags = new List<string>(),
                Metadata = new Dictionary<string, object>(),
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