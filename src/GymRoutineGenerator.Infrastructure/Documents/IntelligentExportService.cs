using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;
using System.Diagnostics;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UserProfile = GymRoutineGenerator.Core.Models.UserProfile;
using UserRoutine = GymRoutineGenerator.Core.Models.UserRoutine;
using Exercise = GymRoutineGenerator.Core.Models.Exercise;
using WorkoutSession = GymRoutineGenerator.Core.Models.WorkoutSession;

namespace GymRoutineGenerator.Infrastructure.Documents
{
    public class IntelligentExportService : IIntelligentExportService
    {
        private readonly IOllamaService _ollamaService;
        private readonly ISmartPromptService _promptService;
        private readonly IProgressionService _progressionService;
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<IntelligentExportService> _logger;

        public IntelligentExportService(
            IOllamaService ollamaService,
            ISmartPromptService promptService,
            IProgressionService progressionService,
            IUserRepository userRepository,
            IExerciseRepository exerciseRepository,
            ILogger<IntelligentExportService> logger)
        {
            _ollamaService = ollamaService;
            _promptService = promptService;
            _progressionService = progressionService;
            _userRepository = userRepository;
            _exerciseRepository = exerciseRepository;
            _logger = logger;
        }

        public async Task<ExportResult> ExportWithAIEnhancementsAsync(UserRoutine routine, ExportOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting AI-enhanced export for routine {routine.Id}");

                // Generate AI-enhanced content
                var aiContent = await GenerateAIContentAsync(routine, options);

                // Create export based on format
                var result = await CreateExportAsync(routine, options, aiContent);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed AI-enhanced export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AI-enhanced export for routine {routine.Id}");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<ExportResult> ExportProgressReportAsync(int userId, ProgressReportOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting progress report export for user {userId}");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException($"User {userId} not found");

                var progressionAnalysis = await _progressionService.AnalyzeUserProgressionAsync(userId);
                var progressMetrics = await _progressionService.GetProgressMetricsAsync(userId, options.TimeRange);

                var coreUser = MapToCore(user);
                var result = await CreateProgressReportAsync(coreUser, progressionAnalysis, progressMetrics, options);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed progress report export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in progress report export for user {userId}");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<ExportResult> ExportCustomizedRoutineBookAsync(List<UserRoutine> routines, BookOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting routine book export for {routines.Count} routines");

                var result = await CreateRoutineBookAsync(routines, options);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed routine book export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in routine book export");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<ExportResult> ExportWorkoutLogAsync(List<WorkoutSession> sessions, WorkoutLogOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting workout log export for {sessions.Count} sessions");

                var result = await CreateWorkoutLogAsync(sessions, options);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed workout log export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in workout log export");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<byte[]> GenerateInstructionalPDFAsync(List<Exercise> exercises, InstructionalOptions options)
        {
            try
            {
                _logger.LogInformation($"Generating instructional PDF for {exercises.Count} exercises");

                // This would generate a comprehensive PDF with step-by-step instructions
                // For now, return empty byte array as placeholder
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating instructional PDF");
                throw;
            }
        }

        public async Task<ExportResult> ExportNutritionGuideAsync(UserProfile profile, NutritionOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting nutrition guide export for user {profile.Id}");

                var result = await CreateNutritionGuideAsync(profile, options);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed nutrition guide export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in nutrition guide export for user {profile.Id}");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        public async Task<ExportResult> ExportComprehensiveReportAsync(int userId, ComprehensiveReportOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Starting comprehensive report export for user {userId}");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException($"User {userId} not found");

                var coreUser = new GymRoutineGenerator.Core.Models.UserProfile
                {
                    Id = user.Id,
                    UserId = user.Id,
                    Name = user.Name,
                    Age = user.Age,
                    Gender = user.Gender,
                    FitnessLevel = user.FitnessLevel,
                    TrainingDays = user.TrainingDays,
                    TrainingDaysPerWeek = user.TrainingDays
                };
                var result = await CreateComprehensiveReportAsync(coreUser, options);

                stopwatch.Stop();
                result.GenerationTime = stopwatch.Elapsed;

                _logger.LogInformation($"Completed comprehensive report export in {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in comprehensive report export for user {userId}");
                stopwatch.Stop();

                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message },
                    GenerationTime = stopwatch.Elapsed
                };
            }
        }

        #region Private Helper Methods

        private async Task<AIEnhancedContent> GenerateAIContentAsync(UserRoutine routine, ExportOptions options)
        {
            try
            {
                var content = new AIEnhancedContent();

                if (options.IncludeAIExplanations)
                {
                    content.ExecutiveSummary = await GenerateExecutiveSummaryAsync(routine);
                    content.ExerciseExplanations = await GenerateExerciseExplanationsAsync(routine);
                    content.ProgressionRationale = await GenerateProgressionRationaleAsync(routine);
                    content.SafetyAssessment = await GenerateSafetyAssessmentAsync(routine);
                    content.MotivationalMessage = await GenerateMotivationalMessageAsync(routine);
                }

                if (options.IncludeScientificReferences)
                {
                    content.ScientificInsights = await GenerateScientificInsightsAsync(routine);
                }

                if (options.IncludePersonalization)
                {
                    content.PersonalizedTips = await GeneratePersonalizedTipsAsync(routine);
                    content.Recommendations = await GenerateAIRecommendationsAsync(routine);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI content");
                return new AIEnhancedContent();
            }
        }

        private async Task<string> GenerateExecutiveSummaryAsync(UserRoutine routine)
        {
            var prompt = $@"
Genera un resumen ejecutivo profesional para la rutina de ejercicios ""{routine.Name}"".

Información de la rutina:
- Nombre: {routine.Name}
- Creada: {routine.CreatedAt:dd/MM/yyyy}
- Estado: {routine.Status}
- Notas: {routine.Notes}

El resumen debe incluir:
1. Objetivo principal de la rutina
2. Beneficios esperados
3. Público objetivo
4. Duración recomendada
5. Puntos clave de seguridad

Mantén un tono profesional y conciso. Máximo 200 palabras.";

            return await _ollamaService.GenerateResponseAsync(prompt);
        }

        private async Task<List<AIExplanation>> GenerateExerciseExplanationsAsync(UserRoutine routine)
        {
            var explanations = new List<AIExplanation>();

            // Parse routine data to get exercises (this would depend on your routine data structure)
            var exercises = await ExtractExercisesFromRoutineAsync(routine);

            foreach (var exercise in exercises)
            {
                var explanation = await GenerateExerciseExplanationAsync(exercise);
                explanations.Add(explanation);
            }

            return explanations;
        }

        private async Task<AIExplanation> GenerateExerciseExplanationAsync(Exercise exercise)
        {
            var prompt = $@"
Genera una explicación científica completa para el ejercicio ""{exercise.Name}"".

Información del ejercicio:
- Músculo principal: {exercise.PrimaryMuscleGroup}
- Equipamiento: {exercise.RequiredEquipment}
- Dificultad: {exercise.DifficultyLevel}
- Tipo: {exercise.ExerciseType}

La explicación debe incluir:
1. Por qué este ejercicio es beneficioso
2. Fundamento científico (biomecánica, fisiología)
3. Técnica correcta paso a paso
4. Errores comunes y cómo evitarlos
5. Consideraciones de seguridad
6. Progresiones posibles

Responde en formato JSON con esta estructura:
{{
  ""whyIncluded"": ""razón de inclusión"",
  ""scientificRationale"": ""fundamento científico"",
  ""benefits"": [""lista de beneficios""],
  ""techniqueKeys"": [""puntos clave de técnica""],
  ""commonMistakes"": [""errores comunes""],
  ""safetyNotes"": ""notas de seguridad""
}}";

            var response = await _ollamaService.GenerateResponseAsync(prompt);
            return ParseExerciseExplanation(response, exercise);
        }

        private async Task<string> GenerateProgressionRationaleAsync(UserRoutine routine)
        {
            var prompt = $@"
Explica la lógica de progresión para la rutina ""{routine.Name}"".

Incluye:
1. Principios de progresión aplicados
2. Por qué esta secuencia de ejercicios
3. Cómo debe evolucionar la rutina en el tiempo
4. Indicadores de progreso a observar
5. Cuándo y cómo hacer ajustes

Mantén un enfoque educativo y científico. Máximo 300 palabras.";

            return await _ollamaService.GenerateResponseAsync(prompt);
        }

        private async Task<string> GenerateSafetyAssessmentAsync(UserRoutine routine)
        {
            var prompt = $@"
Realiza una evaluación de seguridad para la rutina ""{routine.Name}"".

Analiza:
1. Riesgos potenciales de los ejercicios
2. Precauciones necesarias
3. Contraindicaciones
4. Recomendaciones de supervisión
5. Equipamiento de seguridad necesario

Proporciona un análisis profesional orientado a la prevención de lesiones.";

            return await _ollamaService.GenerateResponseAsync(prompt);
        }

        private async Task<string> GenerateMotivationalMessageAsync(UserRoutine routine)
        {
            var prompt = $@"
Crea un mensaje motivacional personalizado para alguien que seguirá la rutina ""{routine.Name}"".

El mensaje debe:
1. Ser inspirador y positivo
2. Reconocer el esfuerzo requerido
3. Enfocarse en los beneficios a obtener
4. Incluir consejos para mantener la consistencia
5. Ser genuino y empático

Tono: Motivador pero realista. Máximo 150 palabras.";

            return await _ollamaService.GenerateResponseAsync(prompt);
        }

        private async Task<List<AIInsight>> GenerateScientificInsightsAsync(UserRoutine routine)
        {
            var insights = new List<AIInsight>();

            var prompt = $@"
Proporciona insights científicos relevantes para la rutina ""{routine.Name}"".

Incluye estudios, principios fisiológicos y evidencia científica sobre:
1. Adaptaciones musculares esperadas
2. Efectos cardiovasculares
3. Beneficios metabólicos
4. Aspectos neurológicos del entrenamiento
5. Consideraciones hormonales

Para cada insight, incluye nivel de evidencia y aplicación práctica.";

            var response = await _ollamaService.GenerateResponseAsync(prompt);
            // Parse response and create insights (simplified implementation)
            insights.Add(new AIInsight
            {
                Topic = "Adaptaciones Musculares",
                Insight = response,
                Reliability = ReliabilityLevel.Medium,
                Source = "Análisis IA basado en literatura científica"
            });

            return insights;
        }

        private async Task<List<PersonalizedTip>> GeneratePersonalizedTipsAsync(UserRoutine routine)
        {
            var tips = new List<PersonalizedTip>();

            var user = await _userRepository.GetByIdAsync(routine.UserId);
            if (user == null) return tips;

            var prompt = $@"
Genera consejos personalizados para {user.Name} basándote en su perfil:
- Edad: {user.Age}
- Género: {user.Gender}
- Nivel de fitness: {user.FitnessLevel}

Los consejos deben ser específicos, prácticos y aplicables a la rutina ""{routine.Name}"".

Incluye consejos sobre:
1. Técnica y forma
2. Progresión
3. Recuperación
4. Nutrición básica
5. Mentalidad y motivación";

            var response = await _ollamaService.GenerateResponseAsync(prompt);
            // Parse and create tips (simplified implementation)
            tips.Add(new PersonalizedTip
            {
                Category = "Técnica",
                Tip = response,
                Priority = Priority.High,
                Reasoning = "Basado en perfil de usuario y análisis de rutina"
            });

            return tips;
        }

        private async Task<List<AIRecommendation>> GenerateAIRecommendationsAsync(UserRoutine routine)
        {
            var recommendations = new List<AIRecommendation>();

            var progressionAnalysis = await _progressionService.AnalyzeUserProgressionAsync(routine.UserId);

            foreach (var opportunity in progressionAnalysis.Opportunities)
            {
                recommendations.Add(new AIRecommendation
                {
                    Type = opportunity.Category,
                    Recommendation = opportunity.Description,
                    Justification = "Basado en análisis de progresión personalizado",
                    Timeline = opportunity.EstimatedTimeframe.ToString(),
                    ConfidenceLevel = opportunity.PotentialImpact,
                    ActionSteps = opportunity.ActionItems
                });
            }

            return recommendations;
        }

        private async Task<ExportResult> CreateExportAsync(UserRoutine routine, ExportOptions options, AIEnhancedContent aiContent)
        {
            return options.Format switch
            {
                ExportFormat.Word => await CreateWordDocumentAsync(routine, options, aiContent),
                ExportFormat.PDF => await CreatePDFDocumentAsync(routine, options, aiContent),
                ExportFormat.HTML => await CreateHTMLDocumentAsync(routine, options, aiContent),
                ExportFormat.JSON => await CreateJSONExportAsync(routine, options, aiContent),
                _ => throw new NotSupportedException($"Export format {options.Format} not supported")
            };
        }

        private async Task<ExportResult> CreateWordDocumentAsync(UserRoutine routine, ExportOptions options, AIEnhancedContent aiContent)
        {
            try
            {
                var fileName = $"{routine.Name}_AI_Enhanced_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                var filePath = Path.Combine(options.OutputPath, fileName);

                using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Add AI-enhanced content sections
                await AddDocumentHeaderAsync(body, routine, options);
                await AddExecutiveSummaryAsync(body, aiContent.ExecutiveSummary);
                await AddExerciseExplanationsAsync(body, aiContent.ExerciseExplanations);
                await AddProgressionSectionAsync(body, aiContent.ProgressionRationale);
                await AddSafetySectionAsync(body, aiContent.SafetyAssessment);
                await AddPersonalizedTipsAsync(body, aiContent.PersonalizedTips);
                await AddMotivationalSectionAsync(body, aiContent.MotivationalMessage);

                if (options.IncludeScientificReferences)
                {
                    await AddScientificInsightsAsync(body, aiContent.ScientificInsights);
                }

                document.Save();

                var fileInfo = new FileInfo(filePath);
                return new ExportResult
                {
                    Success = true,
                    FilePath = filePath,
                    FileName = fileName,
                    Format = ExportFormat.Word,
                    FileSizeBytes = fileInfo.Length,
                    Metadata = new ExportMetadata
                    {
                        CreatedAt = DateTime.UtcNow,
                        ExerciseCount = aiContent.ExerciseExplanations.Count,
                        TemplateUsed = options.TemplateStyle.ToString(),
                        AIGeneratedSections = new List<string>
                        {
                            "Executive Summary", "Exercise Explanations", "Progression Rationale",
                            "Safety Assessment", "Personalized Tips", "Motivational Message"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Word document");
                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ExportResult> CreatePDFDocumentAsync(UserRoutine routine, ExportOptions options, AIEnhancedContent aiContent)
        {
            // PDF generation would be implemented here
            return new ExportResult
            {
                Success = false,
                Errors = new List<string> { "PDF export not yet implemented" }
            };
        }

        private async Task<ExportResult> CreateHTMLDocumentAsync(UserRoutine routine, ExportOptions options, AIEnhancedContent aiContent)
        {
            try
            {
                var fileName = $"{routine.Name}_AI_Enhanced_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                var filePath = Path.Combine(options.OutputPath, fileName);

                var html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang='es'>");
                html.AppendLine("<head>");
                html.AppendLine($"<title>{routine.Name} - Rutina con IA</title>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                html.AppendLine(GetHTMLStyles(options.TemplateStyle));
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                // Add content sections
                html.AppendLine($"<header><h1>{routine.Name}</h1><p>Rutina mejorada con IA</p></header>");
                html.AppendLine("<main>");

                if (!string.IsNullOrEmpty(aiContent.ExecutiveSummary))
                {
                    html.AppendLine("<section class='executive-summary'>");
                    html.AppendLine("<h2>📋 Resumen Ejecutivo</h2>");
                    html.AppendLine($"<p>{aiContent.ExecutiveSummary}</p>");
                    html.AppendLine("</section>");
                }

                // Add other sections...

                html.AppendLine("</main>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");

                await File.WriteAllTextAsync(filePath, html.ToString());

                var fileInfo = new FileInfo(filePath);
                return new ExportResult
                {
                    Success = true,
                    FilePath = filePath,
                    FileName = fileName,
                    Format = ExportFormat.HTML,
                    FileSizeBytes = fileInfo.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating HTML document");
                return new ExportResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ExportResult> CreateJSONExportAsync(UserRoutine routine, ExportOptions options, AIEnhancedContent aiContent)
        {
            // JSON export implementation
            return new ExportResult
            {
                Success = false,
                Errors = new List<string> { "JSON export not yet implemented" }
            };
        }

        // Additional export type implementations
        private async Task<ExportResult> CreateProgressReportAsync(UserProfile user, ProgressionAnalysis analysis, List<ProgressMetric> metrics, ProgressReportOptions options)
        {
            try
            {
                _logger.LogInformation($"Creating progress report for user {user.Id}");

                var fileName = $"Progress_Report_{user.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var filePath = Path.Combine(options.OutputPath, fileName);

                return options.Format switch
                {
                    ExportFormat.Word => await CreateProgressReportWordAsync(user, analysis, metrics, options, filePath),
                    ExportFormat.HTML => await CreateProgressReportHTMLAsync(user, analysis, metrics, options, filePath),
                    ExportFormat.PDF => await CreateProgressReportPDFAsync(user, analysis, metrics, options, filePath),
                    _ => new ExportResult { Success = false, Errors = new List<string> { $"Format {options.Format} not supported for progress reports" } }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating progress report");
                return new ExportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        private async Task<ExportResult> CreateRoutineBookAsync(List<UserRoutine> routines, BookOptions options)
        {
            try
            {
                _logger.LogInformation($"Creating routine book with {routines.Count} routines");

                var fileName = $"{options.BookTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var filePath = Path.Combine(options.OutputPath, fileName);

                using var document = WordprocessingDocument.Create($"{filePath}.docx", WordprocessingDocumentType.Document);
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Book cover
                await AddBookCoverAsync(body, options);

                // Table of contents
                if (options.IncludeTableOfContents)
                {
                    await AddTableOfContentsAsync(body, routines);
                }

                // Introduction
                if (options.IncludeIntroduction)
                {
                    await AddBookIntroductionAsync(body, options);
                }

                // Routines
                foreach (var routine in routines)
                {
                    await AddRoutineToBookAsync(body, routine, options);
                }

                // Appendix
                if (options.IncludeAppendix)
                {
                    await AddBookAppendixAsync(body, routines, options);
                }

                document.Save();

                var fileInfo = new FileInfo($"{filePath}.docx");
                return new ExportResult
                {
                    Success = true,
                    FilePath = $"{filePath}.docx",
                    FileName = $"{fileName}.docx",
                    Format = ExportFormat.Word,
                    FileSizeBytes = fileInfo.Length,
                    Metadata = new ExportMetadata
                    {
                        CreatedAt = DateTime.UtcNow,
                        ExerciseCount = routines.SelectMany(r => ExtractExercisesFromRoutineAsync(r).Result).Count(),
                        TemplateUsed = options.TemplateStyle.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating routine book");
                return new ExportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        private async Task<ExportResult> CreateWorkoutLogAsync(List<WorkoutSession> sessions, WorkoutLogOptions options)
        {
            try
            {
                _logger.LogInformation($"Creating workout log for {sessions.Count} sessions");

                var fileName = $"Workout_Log_{DateTime.Now:yyyyMMdd_HHmmss}";
                var filePath = Path.Combine(options.OutputPath, fileName);

                var html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang='es'>");
                html.AppendLine("<head>");
                html.AppendLine($"<title>Registro de Entrenamientos</title>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine(GetWorkoutLogStyles());
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                html.AppendLine("<header>");
                html.AppendLine("<h1>📋 Registro de Entrenamientos</h1>");
                html.AppendLine($"<p>Período: {sessions.Min(s => s.StartTime):dd/MM/yyyy} - {sessions.Max(s => s.StartTime):dd/MM/yyyy}</p>");
                html.AppendLine($"<p>Total de sesiones: {sessions.Count}</p>");
                html.AppendLine("</header>");

                html.AppendLine("<main>");

                if (options.IncludeSummaryStats)
                {
                    html.AppendLine("<section class='summary-stats'>");
                    html.AppendLine("<h2>📊 Estadísticas Resumidas</h2>");
                    await AddWorkoutLogSummaryAsync(html, sessions);
                    html.AppendLine("</section>");
                }

                if (options.GroupByWeek)
                {
                    var weeklyGroups = sessions.GroupBy(s => GetWeekOfYear(s.StartTime)).OrderBy(g => g.Key);
                    foreach (var week in weeklyGroups)
                    {
                        html.AppendLine($"<section class='weekly-log'>");
                        html.AppendLine($"<h3>🗓️ Semana {week.Key}</h3>");
                        await AddWeeklySessionsAsync(html, week.ToList(), options);
                        html.AppendLine("</section>");
                    }
                }
                else
                {
                    foreach (var session in sessions.OrderByDescending(s => s.StartTime))
                    {
                        await AddSessionToLogAsync(html, session, options);
                    }
                }

                html.AppendLine("</main>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");

                await File.WriteAllTextAsync($"{filePath}.html", html.ToString());

                var fileInfo = new FileInfo($"{filePath}.html");
                return new ExportResult
                {
                    Success = true,
                    FilePath = $"{filePath}.html",
                    FileName = $"{fileName}.html",
                    Format = ExportFormat.HTML,
                    FileSizeBytes = fileInfo.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workout log");
                return new ExportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        private async Task<ExportResult> CreateNutritionGuideAsync(UserProfile profile, NutritionOptions options)
        {
            try
            {
                _logger.LogInformation($"Creating nutrition guide for user {profile.Id}");

                var aiContent = await GenerateNutritionContentAsync(profile, options);

                var fileName = $"Nutrition_Guide_{profile.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var filePath = Path.Combine(options.OutputPath, fileName);

                var html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang='es'>");
                html.AppendLine("<head>");
                html.AppendLine($"<title>Guía Nutricional - {profile.Name}</title>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine(GetNutritionGuideStyles());
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                html.AppendLine($"<header>");
                html.AppendLine($"<h1>🥗 Guía Nutricional Personalizada</h1>");
                html.AppendLine($"<h2>{profile.Name}</h2>");
                html.AppendLine($"<p>Objetivo: {options.Goal}</p>");
                html.AppendLine("</header>");

                html.AppendLine("<main>");
                html.AppendLine($"<div class='ai-content'>{aiContent}</div>");
                html.AppendLine("</main>");

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                await File.WriteAllTextAsync($"{filePath}.html", html.ToString());

                var fileInfo = new FileInfo($"{filePath}.html");
                return new ExportResult
                {
                    Success = true,
                    FilePath = $"{filePath}.html",
                    FileName = $"{fileName}.html",
                    Format = ExportFormat.HTML,
                    FileSizeBytes = fileInfo.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating nutrition guide");
                return new ExportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        private async Task<ExportResult> CreateComprehensiveReportAsync(UserProfile user, ComprehensiveReportOptions options)
        {
            try
            {
                _logger.LogInformation($"Creating comprehensive report for user {user.Id}");

                var fileName = $"Comprehensive_Report_{user.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                var filePath = Path.Combine(options.OutputPath, fileName);

                using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Executive Summary
                if (options.IncludeExecutiveSummary)
                {
                    await AddComprehensiveReportSummaryAsync(body, user, options);
                }

                // Sections based on options
                if (options.IncludeRoutineHistory)
                {
                    await AddRoutineHistoryReportAsync(body, user.Id);
                }

                if (options.IncludeProgressAnalysis)
                {
                    var progressAnalysis = await _progressionService.AnalyzeUserProgressionAsync(user.Id);
                    await AddProgressAnalysisReportAsync(body, progressAnalysis);
                }

                if (options.IncludeRecommendations)
                {
                    await AddRecommendationsReportAsync(body, user);
                }

                document.Save();

                var fileInfo = new FileInfo(filePath);
                return new ExportResult
                {
                    Success = true,
                    FilePath = filePath,
                    FileName = fileName,
                    Format = ExportFormat.Word,
                    FileSizeBytes = fileInfo.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comprehensive report");
                return new ExportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        // Word document helper methods
        private async Task AddDocumentHeaderAsync(Body body, UserRoutine routine, ExportOptions options)
        {
            var title = new Paragraph(new Run(new Text($"{routine.Name} - Rutina Mejorada con IA")));
            body.Append(title);
        }

        private async Task AddExecutiveSummaryAsync(Body body, string summary)
        {
            if (string.IsNullOrEmpty(summary)) return;

            var heading = new Paragraph(new Run(new Text("📋 Resumen Ejecutivo")));
            var content = new Paragraph(new Run(new Text(summary)));
            body.Append(heading);
            body.Append(content);
        }

        private async Task AddExerciseExplanationsAsync(Body body, List<AIExplanation> explanations)
        {
            if (!explanations.Any()) return;

            var heading = new Paragraph(new Run(new Text("💡 Explicaciones de Ejercicios")));
            body.Append(heading);

            foreach (var explanation in explanations)
            {
                var exerciseHeading = new Paragraph(new Run(new Text($"🏋️ {explanation.Exercise.Name}")));
                var whyIncluded = new Paragraph(new Run(new Text($"Por qué incluido: {explanation.WhyIncluded}")));
                var rationale = new Paragraph(new Run(new Text($"Fundamento científico: {explanation.ScientificRationale}")));

                body.Append(exerciseHeading);
                body.Append(whyIncluded);
                body.Append(rationale);
            }
        }

        private async Task AddProgressionSectionAsync(Body body, string progressionRationale)
        {
            if (string.IsNullOrEmpty(progressionRationale)) return;

            var heading = new Paragraph(new Run(new Text("📈 Lógica de Progresión")));
            var content = new Paragraph(new Run(new Text(progressionRationale)));
            body.Append(heading);
            body.Append(content);
        }

        private async Task AddSafetySectionAsync(Body body, string safetyAssessment)
        {
            if (string.IsNullOrEmpty(safetyAssessment)) return;

            var heading = new Paragraph(new Run(new Text("🛡️ Evaluación de Seguridad")));
            var content = new Paragraph(new Run(new Text(safetyAssessment)));
            body.Append(heading);
            body.Append(content);
        }

        private async Task AddPersonalizedTipsAsync(Body body, List<PersonalizedTip> tips)
        {
            if (!tips.Any()) return;

            var heading = new Paragraph(new Run(new Text("💎 Consejos Personalizados")));
            body.Append(heading);

            foreach (var tip in tips)
            {
                var tipParagraph = new Paragraph(new Run(new Text($"• {tip.Tip}")));
                body.Append(tipParagraph);
            }
        }

        private async Task AddMotivationalSectionAsync(Body body, string motivationalMessage)
        {
            if (string.IsNullOrEmpty(motivationalMessage)) return;

            var heading = new Paragraph(new Run(new Text("🚀 Mensaje Motivacional")));
            var content = new Paragraph(new Run(new Text(motivationalMessage)));
            body.Append(heading);
            body.Append(content);
        }

        private async Task AddScientificInsightsAsync(Body body, List<AIInsight> insights)
        {
            if (!insights.Any()) return;

            var heading = new Paragraph(new Run(new Text("🧬 Insights Científicos")));
            body.Append(heading);

            foreach (var insight in insights)
            {
                var insightParagraph = new Paragraph(new Run(new Text($"{insight.Topic}: {insight.Insight}")));
                body.Append(insightParagraph);
            }
        }

        private string GetHTMLStyles(TemplateStyle style)
        {
            return @"
<style>
    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; margin: 0; padding: 20px; }
    header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; margin-bottom: 30px; }
    h1 { margin: 0; font-size: 2.5em; }
    h2 { color: #333; border-bottom: 2px solid #667eea; padding-bottom: 10px; }
    .executive-summary { background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }
    section { margin: 30px 0; }
    .ai-generated { border-left: 4px solid #667eea; padding-left: 20px; background: #f0f4ff; margin: 15px 0; padding: 15px; border-radius: 5px; }
</style>";
        }

        // Helper methods
        private async Task<List<Exercise>> ExtractExercisesFromRoutineAsync(UserRoutine routine)
        {
            // This would parse the routine data to extract exercises
            // For now, return empty list as placeholder
            return new List<Exercise>();
        }

        private AIExplanation ParseExerciseExplanation(string response, Exercise exercise)
        {
            // Parse JSON response from AI - simplified implementation
            return new AIExplanation
            {
                Exercise = exercise,
                WhyIncluded = "Análisis generado por IA",
                ScientificRationale = response,
                Benefits = new List<string> { "Beneficio extraído del análisis IA" },
                TechniqueKeys = new List<string> { "Técnica clave extraída del análisis IA" },
                SafetyNotes = "Notas de seguridad generadas por IA"
            };
        }

        #endregion

        #region Progress Report Helper Methods

        private async Task<ExportResult> CreateProgressReportWordAsync(UserProfile user, ProgressionAnalysis analysis, List<ProgressMetric> metrics, ProgressReportOptions options, string filePath)
        {
            using var document = WordprocessingDocument.Create($"{filePath}.docx", WordprocessingDocumentType.Document);
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            var title = new Paragraph(new Run(new Text($"📊 Reporte de Progreso - {user.Name}")));
            body.Append(title);

            // Analysis period
            var endDate = analysis.AnalysisDate;
            var startDate = analysis.AnalysisDate.Subtract(analysis.AnalysisPeriod);
            var period = new Paragraph(new Run(new Text($"Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}")));
            body.Append(period);

            // Overall progress
            if (options.IncludeAchievements)
            {
                await AddAchievementsSectionAsync(body, analysis.AchievementsList);
            }

            // Progress charts would go here
            if (options.IncludeCharts)
            {
                await AddProgressChartsAsync(body, metrics);
            }

            document.Save();
            var fileInfo = new FileInfo($"{filePath}.docx");
            return new ExportResult
            {
                Success = true,
                FilePath = $"{filePath}.docx",
                FileName = $"{Path.GetFileName(filePath)}.docx",
                Format = ExportFormat.Word,
                FileSizeBytes = fileInfo.Length
            };
        }

        private async Task<ExportResult> CreateProgressReportHTMLAsync(UserProfile user, ProgressionAnalysis analysis, List<ProgressMetric> metrics, ProgressReportOptions options, string filePath)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine($"<title>Reporte de Progreso - {user.Name}</title>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine(GetProgressReportStyles());
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine($"<header>");
            html.AppendLine($"<h1>📊 Reporte de Progreso</h1>");
            html.AppendLine($"<h2>{user.Name}</h2>");
            var endDate = analysis.AnalysisDate;
            var startDate = analysis.AnalysisDate.Subtract(analysis.AnalysisPeriod);
            html.AppendLine($"<p>Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}</p>");
            html.AppendLine("</header>");

            html.AppendLine("<main>");

            // Overall progress summary
            html.AppendLine("<section class='overall-progress'>");
            html.AppendLine("<h3>📈 Progreso General</h3>");
            html.AppendLine($"<p><strong>Puntuación General:</strong> {analysis.OverallProgress.OverallScore:F1}/100</p>");
            html.AppendLine($"<p>{analysis.OverallProgress.Summary}</p>");
            html.AppendLine("</section>");

            // Achievements
            if (options.IncludeAchievements && analysis.AchievementsList.Any())
            {
                html.AppendLine("<section class='achievements'>");
                html.AppendLine("<h3>🏆 Logros Destacados</h3>");
                foreach (var achievement in analysis.AchievementsList)
                {
                    html.AppendLine($"<div class='achievement'>");
                    html.AppendLine($"<h4>{achievement.Title}</h4>");
                    html.AppendLine($"<p>{achievement.Description}</p>");
                    html.AppendLine($"<small>Fecha: {achievement.DateAchieved:dd/MM/yyyy}</small>");
                    html.AppendLine("</div>");
                }
                html.AppendLine("</section>");
            }

            html.AppendLine("</main>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            await File.WriteAllTextAsync($"{filePath}.html", html.ToString());
            var fileInfo = new FileInfo($"{filePath}.html");
            return new ExportResult
            {
                Success = true,
                FilePath = $"{filePath}.html",
                FileName = $"{Path.GetFileName(filePath)}.html",
                Format = ExportFormat.HTML,
                FileSizeBytes = fileInfo.Length
            };
        }

        private async Task<ExportResult> CreateProgressReportPDFAsync(UserProfile user, ProgressionAnalysis analysis, List<ProgressMetric> metrics, ProgressReportOptions options, string filePath)
        {
            // For now, create HTML and suggest PDF conversion
            var htmlResult = await CreateProgressReportHTMLAsync(user, analysis, metrics, options, filePath);
            if (htmlResult.Success)
            {
                htmlResult.Warnings.Add("PDF export not yet implemented. HTML file generated for manual conversion.");
            }
            return htmlResult;
        }

        #endregion

        #region Book Creation Helper Methods

        private async Task AddBookCoverAsync(Body body, BookOptions options)
        {
            var coverPage = new Paragraph();
            var titleRun = new Run(new Text(options.BookTitle));
            coverPage.Append(titleRun);
            body.Append(coverPage);

            if (!string.IsNullOrEmpty(options.AuthorName))
            {
                var authorParagraph = new Paragraph(new Run(new Text($"Por: {options.AuthorName}")));
                body.Append(authorParagraph);
            }
        }

        private async Task AddTableOfContentsAsync(Body body, List<UserRoutine> routines)
        {
            var tocTitle = new Paragraph(new Run(new Text("📋 Tabla de Contenidos")));
            body.Append(tocTitle);

            for (int i = 0; i < routines.Count; i++)
            {
                var tocEntry = new Paragraph(new Run(new Text($"{i + 1}. {routines[i].Name}")));
                body.Append(tocEntry);
            }
        }

        private async Task AddBookIntroductionAsync(Body body, BookOptions options)
        {
            var introTitle = new Paragraph(new Run(new Text("📖 Introducción")));
            body.Append(introTitle);

            var prompt = $@"
Genera una introducción atractiva para un libro de rutinas de gimnasio titulado ""{options.BookTitle}"".

La introducción debe:
1. Motivar al lector
2. Explicar el propósito del libro
3. Dar una visión general de lo que encontrará
4. Incluir consejos generales de seguridad
5. Ser inspiradora y educativa

Mantén un tono profesional pero accesible. Máximo 400 palabras.";

            var introduction = await _ollamaService.GenerateResponseAsync(prompt);
            var introParagraph = new Paragraph(new Run(new Text(introduction)));
            body.Append(introParagraph);
        }

        private async Task AddRoutineToBookAsync(Body body, UserRoutine routine, BookOptions options)
        {
            var routineTitle = new Paragraph(new Run(new Text($"🏋️ {routine.Name}")));
            body.Append(routineTitle);

            var routineDescription = new Paragraph(new Run(new Text(routine.Notes ?? "Rutina de entrenamiento personalizada")));
            body.Append(routineDescription);

            // Add AI-enhanced explanation for this routine
            if (options.IncludeAIExplanations)
            {
                var explanation = await GenerateExecutiveSummaryAsync(routine);
                var explanationParagraph = new Paragraph(new Run(new Text(explanation)));
                body.Append(explanationParagraph);
            }
        }

        private async Task AddBookAppendixAsync(Body body, List<UserRoutine> routines, BookOptions options)
        {
            var appendixTitle = new Paragraph(new Run(new Text("📚 Apéndice")));
            body.Append(appendixTitle);

            if (options.IncludeGlossary)
            {
                await AddGlossaryAsync(body);
            }

            if (options.IncludeProgressTrackers)
            {
                await AddProgressTrackersAsync(body);
            }
        }

        private async Task AddGlossaryAsync(Body body)
        {
            var glossaryTitle = new Paragraph(new Run(new Text("📖 Glosario de Términos")));
            body.Append(glossaryTitle);

            var prompt = @"
Genera un glosario completo de términos de fitness y entrenamiento con peso.

Incluye al menos 20 términos importantes como:
- RPE, series, repeticiones, peso muerto, etc.
- Términos anatómicos básicos
- Conceptos de entrenamiento

Formato: **Término**: Definición clara y concisa.";

            var glossary = await _ollamaService.GenerateResponseAsync(prompt);
            var glossaryParagraph = new Paragraph(new Run(new Text(glossary)));
            body.Append(glossaryParagraph);
        }

        private async Task AddProgressTrackersAsync(Body body)
        {
            var trackersTitle = new Paragraph(new Run(new Text("📊 Plantillas de Seguimiento")));
            body.Append(trackersTitle);

            var trackerInfo = new Paragraph(new Run(new Text("Utiliza estas plantillas para registrar tu progreso:")));
            body.Append(trackerInfo);

            // Add simple tracking templates
            var weeklyTemplate = new Paragraph(new Run(new Text("Registro Semanal: Fecha | Ejercicio | Series x Reps | Peso | RPE | Notas")));
            body.Append(weeklyTemplate);
        }

        #endregion

        #region Workout Log Helper Methods

        private string GetWorkoutLogStyles()
        {
            return @"
<style>
    body { font-family: 'Segoe UI', sans-serif; margin: 20px; line-height: 1.6; }
    header { background: #2196F3; color: white; padding: 20px; border-radius: 8px; text-align: center; }
    .summary-stats { background: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0; }
    .weekly-log { margin: 20px 0; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
    .session { margin: 10px 0; padding: 10px; border-left: 4px solid #4CAF50; background: #f9f9f9; }
    h1, h2, h3 { color: #333; }
    .exercise-list { margin-left: 20px; }
    .stat { display: inline-block; margin: 5px 10px; padding: 5px 10px; background: #e3f2fd; border-radius: 3px; }
</style>";
        }

        private async Task AddWorkoutLogSummaryAsync(StringBuilder html, List<WorkoutSession> sessions)
        {
            var totalSessions = sessions.Count;
            var totalDuration = TimeSpan.FromMilliseconds(sessions.Sum(s => s.Duration.TotalMilliseconds));
            var avgDuration = totalSessions > 0 ? TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds / totalSessions) : TimeSpan.Zero;
            var completedSessions = sessions.Count(s => s.Completed);

            html.AppendLine($"<div class='stat'>Total de sesiones: {totalSessions}</div>");
            html.AppendLine($"<div class='stat'>Sesiones completadas: {completedSessions}</div>");
            html.AppendLine($"<div class='stat'>Duración total: {totalDuration.TotalHours:F1}h</div>");
            html.AppendLine($"<div class='stat'>Duración promedio: {avgDuration.TotalMinutes:F0} min</div>");
            if (sessions.Any(s => s.CaloriesBurned > 0))
            {
                var totalCalories = sessions.Sum(s => s.CaloriesBurned);
                html.AppendLine($"<div class='stat'>Calorías totales: {totalCalories:F0}</div>");
            }
        }

        private async Task AddWeeklySessionsAsync(StringBuilder html, List<WorkoutSession> sessions, WorkoutLogOptions options)
        {
            foreach (var session in sessions.OrderBy(s => s.StartTime))
            {
                await AddSessionToLogAsync(html, session, options);
            }
        }

        private async Task AddSessionToLogAsync(StringBuilder html, WorkoutSession session, WorkoutLogOptions options)
        {
            html.AppendLine("<div class='session'>");
            html.AppendLine($"<h4>🏋️ {session.StartTime:dd/MM/yyyy HH:mm}</h4>");
            html.AppendLine($"<p><strong>Duración:</strong> {session.Duration.TotalMinutes:F0} minutos</p>");
            html.AppendLine($"<p><strong>Ubicación:</strong> {session.Location}</p>");

            if (options.IncludeRPEAnalysis)
            {
                html.AppendLine($"<p><strong>RPE General:</strong> {session.OverallRPE}/10</p>");
            }

            if (session.ExercisePerformances.Any())
            {
                html.AppendLine("<div class='exercise-list'>");
                html.AppendLine("<h5>Ejercicios:</h5>");
                foreach (var exercise in session.ExercisePerformances)
                {
                    html.AppendLine($"<p>• {exercise.ExerciseName} - {exercise.Sets} series</p>");
                }
                html.AppendLine("</div>");
            }

            if (options.IncludeNotes && !string.IsNullOrEmpty(session.Notes))
            {
                html.AppendLine($"<p><strong>Notas:</strong> {session.Notes}</p>");
            }

            html.AppendLine("</div>");
        }

        private int GetWeekOfYear(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion

        #region Nutrition Guide Helper Methods

        private async Task<string> GenerateNutritionContentAsync(UserProfile profile, NutritionOptions options)
        {
            var prompt = $@"
Genera una guía nutricional personalizada completa para {profile.Name}.

Perfil del usuario:
- Edad: {profile.Age}
- Género: {profile.Gender}
- Nivel de fitness: {profile.FitnessLevel}
- Objetivo nutricional: {options.Goal}

La guía debe incluir:
1. Recomendaciones calóricas diarias
2. Distribución de macronutrientes
3. Sugerencias de alimentos saludables
4. Timing de comidas alrededor del entrenamiento
5. Hidratación
6. Suplementos básicos (si es apropiado)

Considera las restricciones dietéticas si las hay.
Mantén un tono profesional pero accesible. Incluye fundamentos científicos simples.
Formato en HTML con secciones claras.";

            return await _ollamaService.GenerateResponseAsync(prompt);
        }

        private string GetNutritionGuideStyles()
        {
            return @"
<style>
    body { font-family: 'Segoe UI', sans-serif; margin: 20px; line-height: 1.6; }
    header { background: linear-gradient(135deg, #81C784 0%, #4CAF50 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; }
    .ai-content { background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }
    h1, h2, h3 { color: #2E7D32; }
    section { margin: 20px 0; padding: 15px; border-left: 4px solid #4CAF50; }
    .recommendation { background: #E8F5E8; padding: 10px; margin: 10px 0; border-radius: 5px; }
    .warning { background: #FFF3E0; padding: 10px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #FF9800; }
</style>";
        }

        #endregion

        #region Comprehensive Report Helper Methods

        private async Task AddComprehensiveReportSummaryAsync(Body body, UserProfile user, ComprehensiveReportOptions options)
        {
            var summaryTitle = new Paragraph(new Run(new Text($"📋 Resumen Ejecutivo - {user.Name}")));
            body.Append(summaryTitle);

            var prompt = $@"
Genera un resumen ejecutivo completo para el reporte integral de {user.Name}.

Información del usuario:
- Edad: {user.Age}
- Género: {user.Gender}
- Nivel de fitness: {user.FitnessLevel}

El resumen debe incluir:
1. Estado actual del fitness del usuario
2. Progreso general observado
3. Fortalezas principales
4. Áreas de oportunidad
5. Recomendaciones principales
6. Próximos pasos sugeridos

Mantén un tono profesional, motivador y constructivo. Máximo 500 palabras.";

            var summary = await _ollamaService.GenerateResponseAsync(prompt);
            var summaryParagraph = new Paragraph(new Run(new Text(summary)));
            body.Append(summaryParagraph);
        }

        private async Task AddRoutineHistoryReportAsync(Body body, int userId)
        {
            var historyTitle = new Paragraph(new Run(new Text("📚 Historial de Rutinas")));
            body.Append(historyTitle);

            // This would fetch actual routine history from the database
            var routineHistory = "Análisis del historial de rutinas del usuario...";
            var historyParagraph = new Paragraph(new Run(new Text(routineHistory)));
            body.Append(historyParagraph);
        }

        private async Task AddProgressAnalysisReportAsync(Body body, ProgressionAnalysis analysis)
        {
            var analysisTitle = new Paragraph(new Run(new Text("📊 Análisis de Progreso")));
            body.Append(analysisTitle);

            var analysisSummary = new Paragraph(new Run(new Text(string.Join(Environment.NewLine, analysis.AIInsights))));
            body.Append(analysisSummary);
        }

        private async Task AddRecommendationsReportAsync(Body body, UserProfile user)
        {
            var recsTitle = new Paragraph(new Run(new Text("💡 Recomendaciones Personalizadas")));
            body.Append(recsTitle);

            var prompt = $@"
Genera recomendaciones personalizadas integrales para {user.Name}.

Perfil del usuario:
- Edad: {user.Age}
- Género: {user.Gender}
- Nivel de fitness: {user.FitnessLevel}

Incluye recomendaciones para:
1. Entrenamiento (ajustes, nuevos objetivos)
2. Nutrición (mejoras dietéticas)
3. Recuperación (descanso, manejo del estrés)
4. Estilo de vida (hábitos saludables)
5. Seguimiento (métricas a monitorear)

Sé específico y práctico. Mantén un tono motivador.";

            var recommendations = await _ollamaService.GenerateResponseAsync(prompt);
            var recsParagraph = new Paragraph(new Run(new Text(recommendations)));
            body.Append(recsParagraph);
        }

        private async Task AddAchievementsSectionAsync(Body body, List<Achievement> achievements)
        {
            var achievementsTitle = new Paragraph(new Run(new Text("🏆 Logros Destacados")));
            body.Append(achievementsTitle);

            foreach (var achievement in achievements.Take(10)) // Limit to top 10
            {
                var achievementParagraph = new Paragraph(new Run(new Text($"• {achievement.Title}: {achievement.Description}")));
                body.Append(achievementParagraph);
            }
        }

        private async Task AddProgressChartsAsync(Body body, List<ProgressMetric> metrics)
        {
            var chartsTitle = new Paragraph(new Run(new Text("📈 Gráficos de Progreso")));
            body.Append(chartsTitle);

            // For now, add textual representation of progress
            var chartsDescription = new Paragraph(new Run(new Text("Los gráficos detallados de progreso estarían disponibles en la versión web de este reporte.")));
            body.Append(chartsDescription);
        }

        private string GetProgressReportStyles()
        {
            return @"
<style>
    body { font-family: 'Segoe UI', sans-serif; margin: 20px; line-height: 1.6; }
    header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; }
    .overall-progress { background: #f0f4ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea; }
    .achievements { background: #fff8e1; padding: 20px; border-radius: 8px; margin: 20px 0; }
    .achievement { background: white; padding: 15px; margin: 10px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    h1, h2, h3 { color: #333; }
    .stat { display: inline-block; margin: 5px; padding: 8px 12px; background: #e3f2fd; border-radius: 4px; }
</style>";
        }

        #endregion

        // Mapping methods for type conversion
        private GymRoutineGenerator.Core.Models.UserProfile MapToCore(GymRoutineGenerator.Data.Entities.UserProfile dataUser)
        {
            return new GymRoutineGenerator.Core.Models.UserProfile
            {
                Id = dataUser.Id,
                Name = dataUser.Name,
                Age = dataUser.Age,
                Gender = dataUser.Gender,
                FitnessLevel = dataUser.FitnessLevel,
                TrainingDays = dataUser.TrainingDaysPerWeek,
                TrainingDaysPerWeek = dataUser.TrainingDaysPerWeek,
                Goals = new List<string>(), // Default empty list
                CreatedDate = dataUser.CreatedAt,
                ModifiedDate = dataUser.LastUpdated,
                UpdatedAt = dataUser.UpdatedAt,
                IsActive = true
            };
        }

    }
}