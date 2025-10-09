using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GymRoutineGenerator.Core.Services;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Enums;
using GymRoutineGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Infrastructure.Services
{
    /// <summary>
    /// Servicio de exportación de documentos a Word, HTML y PDF
    /// Migrado desde WordDocumentExporter.cs en app-ui
    /// </summary>
    public class DocumentExportService : IDocumentExportService
    {
        private readonly GymRoutineContext _context;
        private readonly IIntelligentExportService? _intelligentExportService;

        public DocumentExportService(GymRoutineContext context, IIntelligentExportService? intelligentExportService = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _intelligentExportService = intelligentExportService;
        }

        /// <summary>
        /// Verifica si las mejoras de IA están disponibles
        /// </summary>
        public bool IsAIEnabled => _intelligentExportService != null;

        /// <summary>
        /// Exporta rutina a formato Word (HTML compatible con Word)
        /// </summary>
        public async Task<bool> ExportToWordAsync(string filePath, string routineContent, string clientName)
        {
            try
            {
                // Create Word-compatible HTML document
                var wordHtmlContent = CreateWordCompatibleHTML(routineContent, clientName);

                // Save as .doc file (HTML format that Word can open)
                var wordFilePath = Path.ChangeExtension(filePath, ".doc");
                await File.WriteAllTextAsync(wordFilePath, wordHtmlContent, Encoding.UTF8);

                // Also create a clean text version as backup
                var textContent = CreateCleanTextDocument(routineContent, clientName);
                var textFilePath = Path.ChangeExtension(filePath, ".txt");
                await File.WriteAllTextAsync(textFilePath, textContent, Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Exporta rutina a Word con imágenes de la base de datos
        /// </summary>
        public async Task<bool> ExportToWordWithImagesAsync(string filePath, List<WorkoutDay> workoutDays, UserRoutineParameters profile)
        {
            try
            {
                var htmlContent = CreateWordDocumentWithImages(workoutDays, profile);
                var wordFilePath = Path.ChangeExtension(filePath, ".doc");
                await File.WriteAllTextAsync(wordFilePath, htmlContent, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Exporta rutina a HTML optimizado para PDF
        /// </summary>
        public async Task<bool> ExportToPDFAsync(string filePath, string routineContent, string clientName)
        {
            try
            {
                // Create HTML document optimized for PDF conversion
                var htmlContent = CreatePDFReadyHTML(routineContent, clientName);

                // Save as HTML file
                var htmlFilePath = Path.ChangeExtension(filePath, ".html");
                await File.WriteAllTextAsync(htmlFilePath, htmlContent, Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Exporta con mejoras de IA (explicaciones, progresiones, etc.)
        /// </summary>
        public async Task<bool> ExportToWordWithAIAsync(string filePath, List<WorkoutDay> workoutDays, UserRoutineParameters profile, bool includeAIExplanations = true)
        {
            try
            {
                // Create UserRoutine object for AI processing
                var routine = new UserRoutine
                {
                    Id = 0,
                    UserId = profile.Id,
                    Name = $"Rutina para {profile.Name}",
                    CreatedAt = DateTime.Now,
                    Status = RoutineStatus.Active,
                    Notes = "Rutina generada con explicaciones de IA"
                };

                // Serialize workout days to routine data
                routine.RoutineData = SerializeWorkoutDays(workoutDays);

                if (_intelligentExportService != null && includeAIExplanations)
                {
                    var options = new ExportOptions
                    {
                        OutputPath = Path.GetDirectoryName(filePath) ?? "",
                        Format = ExportFormat.Word,
                        IncludeAIExplanations = true,
                        IncludeImages = true,
                        IncludeProgressionSuggestions = true,
                        IncludeSafetyNotes = true,
                        TemplateStyle = TemplateStyle.Professional
                    };

                    var result = await _intelligentExportService.ExportWithAIEnhancementsAsync(routine, options);
                    return result.Success;
                }
                else
                {
                    // Fallback to original method
                    return await ExportToWordWithImagesAsync(filePath, workoutDays, profile);
                }
            }
            catch (Exception)
            {
                // Fallback to original method on error
                return await ExportToWordWithImagesAsync(filePath, workoutDays, profile);
            }
        }

        #region Private Helper Methods

        private string CreateWordCompatibleHTML(string routineContent, string clientName)
        {
            var html = new StringBuilder();

            // Word-compatible HTML header
            html.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office'");
            html.AppendLine("xmlns:w='urn:schemas-microsoft-com:office:word'");
            html.AppendLine("xmlns='http://www.w3.org/TR/REC-html40'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='ProgId' content='Word.Document'>");
            html.AppendLine("<meta name='Generator' content='Microsoft Word'>");
            html.AppendLine("<meta name='Originator' content='Microsoft Word'>");
            html.AppendLine($"<title>Rutina de Gimnasio - {clientName}</title>");

            // CSS styles optimized for Word
            html.AppendLine("<style>");
            html.AppendLine(@"
                body {
                    font-family: 'Calibri', sans-serif;
                    font-size: 11pt;
                    line-height: 1.4;
                    margin: 1in;
                    color: #000000;
                }
                .header {
                    text-align: center;
                    color: #198754;
                    font-size: 18pt;
                    font-weight: bold;
                    margin-bottom: 20pt;
                    border-bottom: 2pt solid #198754;
                    padding-bottom: 10pt;
                }
                .section-header {
                    color: #0d6efd;
                    font-size: 14pt;
                    font-weight: bold;
                    margin-top: 15pt;
                    margin-bottom: 8pt;
                }
                .subsection-header {
                    color: #198754;
                    font-size: 12pt;
                    font-weight: bold;
                    margin-top: 12pt;
                    margin-bottom: 6pt;
                }
                .info-table {
                    border: 1pt solid #dee2e6;
                    border-collapse: collapse;
                    width: 100%;
                    margin: 10pt 0;
                }
                .info-table td {
                    border: 1pt solid #dee2e6;
                    padding: 6pt;
                    vertical-align: top;
                }
                .exercise-table {
                    border: 1pt solid #0d6efd;
                    border-collapse: collapse;
                    width: 100%;
                    margin: 8pt 0;
                }
                .exercise-table th {
                    background-color: #e7f3ff;
                    border: 1pt solid #0d6efd;
                    padding: 8pt;
                    font-weight: bold;
                    text-align: left;
                }
                .exercise-table td {
                    border: 1pt solid #0d6efd;
                    padding: 6pt;
                    vertical-align: top;
                }
                .warning-box {
                    background-color: #fff3cd;
                    border: 1pt solid #ffc107;
                    padding: 10pt;
                    margin: 10pt 0;
                    border-radius: 3pt;
                }
                .timestamp {
                    text-align: center;
                    font-size: 9pt;
                    color: #666666;
                    margin-bottom: 15pt;
                }
                .footer {
                    text-align: center;
                    font-size: 8pt;
                    color: #666666;
                    margin-top: 20pt;
                    border-top: 1pt solid #dee2e6;
                    padding-top: 10pt;
                }
                ul {
                    margin: 5pt 0;
                    padding-left: 20pt;
                }
                li {
                    margin-bottom: 3pt;
                }
                .exercise-image {
                    max-width: 200px;
                    max-height: 150px;
                    margin: 5pt 0;
                    display: block;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Document header
            html.AppendLine("<div class='header'>GENERADOR DE RUTINAS DE GIMNASIO</div>");
            html.AppendLine($"<div class='timestamp'>Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

            // Process content
            ProcessContentForWord(html, routineContent);

            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Documento generado automáticamente</p>");
            html.AppendLine("<p>Para más información, contacte con su entrenador personal</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void ProcessContentForWord(StringBuilder html, string routineContent)
        {
            var lines = routineContent.Split('\n');
            bool inExerciseSection = false;
            bool inPersonalInfo = false;
            bool inGoals = false;
            bool inWarnings = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                // Skip the main header and separator lines
                if (trimmedLine.Contains("RUTINA PERSONALIZADA") ||
                    trimmedLine.Contains("RUTINA ALTERNATIVA") ||
                    trimmedLine.All(c => c == '=' || c == '-' || char.IsWhiteSpace(c))) continue;

                // Section headers - match text patterns, ignore emojis
                if (trimmedLine.Contains("INFORMACIÓN PERSONAL") || trimmedLine.Contains("INFORMACION PERSONAL"))
                {
                    if (inExerciseSection) { html.AppendLine("</table>"); inExerciseSection = false; }
                    if (inWarnings) { html.AppendLine("</div>"); inWarnings = false; }

                    html.AppendLine("<div class='section-header'>INFORMACIÓN PERSONAL</div>");
                    html.AppendLine("<table class='info-table'>");
                    inPersonalInfo = true;
                    continue;
                }

                if (trimmedLine.Contains("OBJETIVOS"))
                {
                    if (inPersonalInfo) { html.AppendLine("</table>"); inPersonalInfo = false; }

                    html.AppendLine("<div class='section-header'>OBJETIVOS SELECCIONADOS</div>");
                    html.AppendLine("<ul>");
                    inGoals = true;
                    continue;
                }

                if (trimmedLine.Contains("RUTINA DE ENTRENAMIENTO"))
                {
                    if (inGoals) { html.AppendLine("</ul>"); inGoals = false; }

                    html.AppendLine("<div class='section-header'>RUTINA DE ENTRENAMIENTO</div>");
                    continue;
                }

                if (trimmedLine.Contains("IMPORTANTE"))
                {
                    if (inExerciseSection) { html.AppendLine("</table>"); inExerciseSection = false; }

                    html.AppendLine("<div class='warning-box'>");
                    html.AppendLine("<div class='subsection-header'>RECOMENDACIONES IMPORTANTES</div>");
                    html.AppendLine("<ul>");
                    inWarnings = true;
                    continue;
                }

                // Day headers (exercise sections)
                if (trimmedLine.Contains("DÍA") || trimmedLine.Contains("DIA"))
                {
                    if (inExerciseSection) html.AppendLine("</table>");

                    html.AppendLine($"<div class='subsection-header'>{CleanEmojis(trimmedLine)}</div>");
                    html.AppendLine("<table class='exercise-table'>");
                    html.AppendLine("<tr><th>Ejercicio</th><th>Series y Repeticiones</th></tr>");
                    inExerciseSection = true;
                    continue;
                }

                // Content processing based on current section
                if (inPersonalInfo && trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var label = System.Text.RegularExpressions.Regex.Replace(parts[0], @"[\p{So}\p{Cs}]", "").Trim();
                        var value = parts[1].Trim();
                        if (!string.IsNullOrEmpty(label))
                        {
                            html.AppendLine($"<tr><td><strong>{label}</strong></td><td>{CleanEmojis(value)}</td></tr>");
                        }
                    }
                }
                else if (inGoals && trimmedLine.StartsWith("-"))
                {
                    var goalText = trimmedLine.Substring(1).Trim();
                    if (!string.IsNullOrEmpty(goalText))
                    {
                        html.AppendLine($"<li>{CleanEmojis(goalText)}</li>");
                    }
                }
                else if (inExerciseSection && System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^\d+\."))
                {
                    // Exercise line (starts with number and dot)
                    var parts = trimmedLine.Split(" - ", 2);
                    if (parts.Length == 2)
                    {
                        var exercise = parts[0].Trim();
                        var sets = parts[1].Trim();

                        // Row with exercise name and sets
                        html.AppendLine($"<tr><td>{CleanEmojis(exercise)}</td><td>{sets}</td></tr>");

                        // Extract exercise name (remove number prefix like "1. ")
                        var exerciseName = System.Text.RegularExpressions.Regex.Replace(exercise, @"^\d+\.\s*", "").Trim();

                        // Try to find image for this exercise
                        var imageBase64 = GetExerciseImageByName(exerciseName);
                        if (!string.IsNullOrEmpty(imageBase64))
                        {
                            // Add a new row with the image spanning both columns, centered
                            html.AppendLine($"<tr><td colspan='2' style='text-align: center; padding: 10pt;'>");
                            html.AppendLine($"<img src='data:image/jpeg;base64,{imageBase64}' class='exercise-image' alt='{exerciseName}' />");
                            html.AppendLine("</td></tr>");
                        }
                    }
                }
                else if (inWarnings && trimmedLine.StartsWith("-"))
                {
                    var warningText = trimmedLine.Substring(1).Trim();
                    if (!string.IsNullOrEmpty(warningText))
                    {
                        html.AppendLine($"<li>{CleanEmojis(warningText)}</li>");
                    }
                }
            }

            // Close any open tags
            if (inPersonalInfo) html.AppendLine("</table>");
            if (inGoals) html.AppendLine("</ul>");
            if (inExerciseSection) html.AppendLine("</table>");
            if (inWarnings) { html.AppendLine("</ul>"); html.AppendLine("</div>"); }
        }

        private string CreateCleanTextDocument(string routineContent, string clientName)
        {
            var content = new StringBuilder();

            content.AppendLine("=====================================================");
            content.AppendLine("          GENERADOR DE RUTINAS DE GIMNASIO          ");
            content.AppendLine("=====================================================");
            content.AppendLine();
            content.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
            content.AppendLine($"Cliente: {clientName}");
            content.AppendLine();

            // Process and clean content
            var lines = routineContent.Split('\n');
            foreach (var line in lines)
            {
                var cleanLine = CleanEmojis(line);

                if (!string.IsNullOrWhiteSpace(cleanLine))
                {
                    content.AppendLine(cleanLine);
                }
            }

            content.AppendLine();
            content.AppendLine("=====================================================");
            content.AppendLine("       Archivo de respaldo en formato texto         ");
            content.AppendLine("   Abra el archivo .doc para el formato completo    ");
            content.AppendLine("=====================================================");

            return content.ToString();
        }

        private string CreatePDFReadyHTML(string routineContent, string clientName)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"<title>Rutina de Gimnasio - {clientName}</title>");

            // PDF-optimized CSS
            html.AppendLine("<style>");
            html.AppendLine(@"
                @page {
                    margin: 0.7in;
                    size: A4;
                }
                @media print {
                    body { margin: 0; }
                    .no-print { display: none; }
                }
                body {
                    font-family: 'Arial', sans-serif;
                    font-size: 11pt;
                    line-height: 1.4;
                    color: #000000;
                    margin: 0;
                    padding: 0;
                }
                .header {
                    text-align: center;
                    color: #198754;
                    font-size: 16pt;
                    font-weight: bold;
                    margin-bottom: 15pt;
                    border-bottom: 2pt solid #198754;
                    padding-bottom: 8pt;
                }
                .section-header {
                    color: #0d6efd;
                    font-size: 13pt;
                    font-weight: bold;
                    margin-top: 12pt;
                    margin-bottom: 6pt;
                    page-break-after: avoid;
                }
                .exercise-section {
                    background-color: #f8f9fa;
                    padding: 8pt;
                    margin: 6pt 0;
                    border-left: 3pt solid #0d6efd;
                    page-break-inside: avoid;
                }
                .exercise-title {
                    font-weight: bold;
                    color: #198754;
                    margin-bottom: 4pt;
                }
                .exercise-list {
                    margin: 0;
                    padding-left: 15pt;
                }
                .exercise-list li {
                    margin-bottom: 2pt;
                }
                .exercise-image {
                    max-width: 300px;
                    max-height: 200px;
                    margin: 10pt auto;
                    display: block;
                    border: 1pt solid #ddd;
                }
                .warning-section {
                    background-color: #fff3cd;
                    border: 1pt solid #ffc107;
                    padding: 8pt;
                    margin: 8pt 0;
                    page-break-inside: avoid;
                }
                .info-section {
                    background-color: #d1ecf1;
                    border: 1pt solid #17a2b8;
                    padding: 8pt;
                    margin: 8pt 0;
                }
                .timestamp {
                    text-align: center;
                    font-size: 9pt;
                    color: #666666;
                    margin-bottom: 12pt;
                }
                .footer {
                    text-align: center;
                    font-size: 8pt;
                    color: #666666;
                    margin-top: 15pt;
                    border-top: 1pt solid #dee2e6;
                    padding-top: 8pt;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Content processing for PDF
            html.AppendLine("<div class='header'>GENERADOR DE RUTINAS DE GIMNASIO</div>");
            html.AppendLine($"<div class='timestamp'>Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

            ProcessContentForPDF(html, routineContent);

            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Para convertir a PDF: Archivo → Imprimir → Guardar como PDF</p>");
            html.AppendLine("<p>Generador de Rutinas de Gimnasio</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void ProcessContentForPDF(StringBuilder html, string routineContent)
        {
            var lines = routineContent.Split('\n');
            bool inExerciseSection = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (trimmedLine.Contains("RUTINA PERSONALIZADA")) continue;

                if (trimmedLine.Contains("INFORMACIÓN PERSONAL") || trimmedLine.Contains("INFORMACION PERSONAL"))
                {
                    html.AppendLine("<div class='section-header'>INFORMACIÓN PERSONAL</div>");
                    html.AppendLine("<div class='info-section'>");
                    continue;
                }

                if (trimmedLine.Contains("OBJETIVOS"))
                {
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='section-header'>OBJETIVOS</div>");
                    html.AppendLine("<div class='info-section'>");
                    continue;
                }

                if (trimmedLine.Contains("RUTINA DE ENTRENAMIENTO"))
                {
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='section-header'>RUTINA DE ENTRENAMIENTO</div>");
                    continue;
                }

                if (trimmedLine.Contains("DÍA") || trimmedLine.Contains("DIA"))
                {
                    if (inExerciseSection) html.AppendLine("</ol></div>");

                    html.AppendLine("<div class='exercise-section'>");
                    html.AppendLine($"<div class='exercise-title'>{CleanEmojis(trimmedLine)}</div>");
                    html.AppendLine("<ol class='exercise-list'>");
                    inExerciseSection = true;
                    continue;
                }

                if (trimmedLine.Contains("IMPORTANTE"))
                {
                    if (inExerciseSection) { html.AppendLine("</ol></div>"); inExerciseSection = false; }

                    html.AppendLine("<div class='warning-section'>");
                    html.AppendLine("<div class='exercise-title'>RECOMENDACIONES IMPORTANTES</div>");
                    continue;
                }

                if (inExerciseSection && System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^\d+\."))
                {
                    var cleanExercise = CleanEmojis(trimmedLine);
                    html.AppendLine($"<li>{cleanExercise}</li>");

                    // Extract exercise name and try to find image
                    var parts = trimmedLine.Split(" - ", 2);
                    if (parts.Length > 0)
                    {
                        var exerciseName = System.Text.RegularExpressions.Regex.Replace(parts[0].Trim(), @"^\d+\.\s*", "").Trim();
                        var imageBase64 = GetExerciseImageByName(exerciseName);
                        if (!string.IsNullOrEmpty(imageBase64))
                        {
                            html.AppendLine($"<div style='text-align: center; margin: 10pt 0;'>");
                            html.AppendLine($"<img src='data:image/jpeg;base64,{imageBase64}' class='exercise-image' alt='{exerciseName}' />");
                            html.AppendLine("</div>");
                        }
                    }
                }
                else if (trimmedLine.StartsWith("-"))
                {
                    var cleanText = CleanEmojis(trimmedLine.Substring(1).Trim());
                    if (!string.IsNullOrEmpty(cleanText))
                    {
                        html.AppendLine($"<p>{cleanText}</p>");
                    }
                }
            }

            if (inExerciseSection) html.AppendLine("</ol></div>");
            html.AppendLine("</div>");
        }

        private string CreateWordDocumentWithImages(List<WorkoutDay> workoutDays, UserRoutineParameters profile)
        {
            var html = new StringBuilder();

            // Word-compatible HTML header con soporte para imágenes
            html.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office'");
            html.AppendLine("xmlns:w='urn:schemas-microsoft-com:office:word'");
            html.AppendLine("xmlns='http://www.w3.org/TR/REC-html40'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='ProgId' content='Word.Document'>");
            html.AppendLine($"<title>Rutina Personalizada - {profile.Name}</title>");

            // CSS optimizado para Word con imágenes
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { font-family: 'Calibri', sans-serif; font-size: 11pt; margin: 1in; }
                .header { text-align: center; color: #198754; font-size: 18pt; font-weight: bold; margin-bottom: 20pt; }
                .exercise-container { margin: 15pt 0; padding: 10pt; border: 1pt solid #ddd; }
                .exercise-title { color: #0d6efd; font-size: 12pt; font-weight: bold; margin-bottom: 5pt; }
                .exercise-details { margin-left: 10pt; }
                .exercise-image { max-width: 200px; max-height: 150px; margin: 5pt 0; }
                .workout-day { margin: 20pt 0; border-bottom: 2pt solid #198754; }
                .day-title { color: #198754; font-size: 14pt; font-weight: bold; margin-bottom: 10pt; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header del documento
            html.AppendLine($"<div class='header'>RUTINA PERSONALIZADA - {profile.Name.ToUpper()}</div>");
            html.AppendLine($"<p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine($"<p><strong>Edad:</strong> {profile.Age} años | <strong>Nivel:</strong> {profile.ExperienceLevel} | <strong>Días/semana:</strong> {profile.TrainingDaysPerWeek}</p>");

            if (profile.Goals != null && profile.Goals.Any())
            {
                html.AppendLine($"<p><strong>Objetivos:</strong> {string.Join(", ", profile.Goals)}</p>");
            }

            html.AppendLine("<hr>");

            // Días de entrenamiento con ejercicios e imágenes
            foreach (var day in workoutDays)
            {
                html.AppendLine($"<div class='workout-day'>");
                html.AppendLine($"<div class='day-title'>{day.Name.ToUpper()}</div>");

                foreach (var exercise in day.Exercises)
                {
                    html.AppendLine("<div class='exercise-container'>");
                    html.AppendLine($"<div class='exercise-title'>{exercise.Name}</div>");
                    html.AppendLine("<div class='exercise-details'>");

                    // Construir series y reps
                    var setsAndReps = $"{exercise.RecommendedSets} series x {exercise.RecommendedReps} repeticiones";
                    html.AppendLine($"<p><strong>Series/Reps:</strong> {setsAndReps}</p>");

                    if (!string.IsNullOrEmpty(exercise.Instructions))
                    {
                        html.AppendLine($"<p><strong>Instrucciones:</strong> {exercise.Instructions}</p>");
                    }

                    // Agregar imagen - prioridad: BD -> Sin imagen
                    string imageBase64 = string.Empty;

                    // Intentar desde BD si tiene ID
                    if (exercise.Id > 0)
                    {
                        imageBase64 = GetExerciseImageAsBase64(exercise.Id);
                    }

                    // Si aún no hay imagen, buscar por nombre
                    if (string.IsNullOrEmpty(imageBase64))
                    {
                        imageBase64 = GetExerciseImageByName(exercise.Name);
                    }

                    // Mostrar imagen o mensaje
                    if (!string.IsNullOrEmpty(imageBase64))
                    {
                        html.AppendLine($"<img src='data:image/jpeg;base64,{imageBase64}' class='exercise-image' alt='{exercise.Name}' />");
                    }
                    else
                    {
                        html.AppendLine("<p><em>Imagen no disponible - Agregar en Gestor de Imágenes</em></p>");
                    }

                    html.AppendLine("</div>");
                    html.AppendLine("</div>");
                }

                html.AppendLine("</div>");
            }

            // Footer
            html.AppendLine("<hr>");
            html.AppendLine("<p><small>Documento generado automáticamente por el Generador de Rutinas de Gimnasio</small></p>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string GetExerciseImageAsBase64(int exerciseId)
        {
            try
            {
                var image = _context.ExerciseImages
                    .Where(img => img.ExerciseId == exerciseId && img.ImageData != null && img.ImageData.Length > 0)
                    .OrderByDescending(img => img.IsPrimary)
                    .FirstOrDefault();

                if (image?.ImageData != null)
                {
                    return Convert.ToBase64String(image.ImageData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo imagen para ejercicio {exerciseId}: {ex.Message}");
            }

            return string.Empty;
        }

        private string GetExerciseImageByName(string exerciseName)
        {
            try
            {
                // First try exact match in database
                var exercise = _context.Exercises
                    .FirstOrDefault(e => e.Name.ToLower() == exerciseName.ToLower() ||
                                        e.SpanishName.ToLower() == exerciseName.ToLower());

                if (exercise != null)
                {
                    var imageFromDb = GetExerciseImageAsBase64(exercise.Id);
                    if (!string.IsNullOrEmpty(imageFromDb))
                    {
                        return imageFromDb;
                    }
                }

                // Try partial match in database
                exercise = _context.Exercises
                    .FirstOrDefault(e => e.Name.ToLower().Contains(exerciseName.ToLower()) ||
                                        exerciseName.ToLower().Contains(e.Name.ToLower()) ||
                                        e.SpanishName.ToLower().Contains(exerciseName.ToLower()) ||
                                        exerciseName.ToLower().Contains(e.SpanishName.ToLower()));

                if (exercise != null)
                {
                    var imageFromDb = GetExerciseImageAsBase64(exercise.Id);
                    if (!string.IsNullOrEmpty(imageFromDb))
                    {
                        return imageFromDb;
                    }
                }

                // If not found in DB, try to find in docs/ejercicios folder
                var imageFromFolder = GetExerciseImageFromFolder(exerciseName);
                if (!string.IsNullOrEmpty(imageFromFolder))
                {
                    return imageFromFolder;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error buscando imagen para ejercicio '{exerciseName}': {ex.Message}");
            }

            return string.Empty;
        }

        private string GetExerciseImageFromFolder(string exerciseName)
        {
            try
            {
                // Base path for exercise images
                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "docs", "ejercicios");
                basePath = Path.GetFullPath(basePath);

                if (!Directory.Exists(basePath))
                {
                    // Try alternative path
                    basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "docs", "ejercicios");
                    basePath = Path.GetFullPath(basePath);
                }

                if (!Directory.Exists(basePath))
                {
                    Console.WriteLine($"Carpeta no encontrada: {basePath}");
                    return string.Empty;
                }

                // Normalizar nombre de ejercicio para búsqueda
                var normalizedName = NormalizeExerciseName(exerciseName);

                // Search for images in all subdirectories
                var imageExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.webp" };
                foreach (var ext in imageExtensions)
                {
                    var files = Directory.GetFiles(basePath, ext, SearchOption.AllDirectories);

                    // Try exact match by folder name first
                    var exactMatch = files.FirstOrDefault(f =>
                    {
                        var folderName = Path.GetFileName(Path.GetDirectoryName(f));
                        if (folderName == null) return false;

                        var normalizedFolderName = NormalizeExerciseName(folderName);
                        return normalizedFolderName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase);
                    });

                    if (exactMatch != null && File.Exists(exactMatch))
                    {
                        var imageBytes = File.ReadAllBytes(exactMatch);
                        return Convert.ToBase64String(imageBytes);
                    }

                    // Try partial match - folder contains exercise name or vice versa
                    var partialMatch = files.FirstOrDefault(f =>
                    {
                        var folderName = Path.GetFileName(Path.GetDirectoryName(f));
                        if (folderName == null) return false;

                        var normalizedFolderName = NormalizeExerciseName(folderName);

                        // Check if folder name contains any significant word from exercise name
                        var exerciseWords = normalizedName.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        var folderWords = normalizedFolderName.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

                        // Si alguna palabra del ejercicio está en la carpeta (mínimo 4 caracteres)
                        foreach (var word in exerciseWords.Where(w => w.Length >= 4))
                        {
                            if (folderWords.Any(fw => fw.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                                                      word.Contains(fw, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                        }

                        return false;
                    });

                    if (partialMatch != null && File.Exists(partialMatch))
                    {
                        var imageBytes = File.ReadAllBytes(partialMatch);
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error buscando imagen en carpeta para '{exerciseName}': {ex.Message}");
            }

            return string.Empty;
        }

        private string NormalizeExerciseName(string name)
        {
            // Remove accents and special characters for better matching
            return name.ToLower()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n")
                .Trim();
        }

        private string CleanEmojis(string text)
        {
            // Remove common emojis used in routine formatting
            return System.Text.RegularExpressions.Regex.Replace(text, @"[\p{So}\p{Cs}]", "")
                .Replace("1⃣", "1.")
                .Replace("2⃣", "2.")
                .Replace("3⃣", "3.")
                .Replace("4⃣", "4.")
                .Replace("5⃣", "5.")
                .Replace("6⃣", "6.")
                .Trim();
        }

        private string SerializeWorkoutDays(List<WorkoutDay> workoutDays)
        {
            // Simple JSON-like serialization for workout days
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"workoutDays\": [");

            for (int i = 0; i < workoutDays.Count; i++)
            {
                var day = workoutDays[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"name\": \"{day.Name}\",");
                sb.AppendLine("      \"exercises\": [");

                for (int j = 0; j < day.Exercises.Count; j++)
                {
                    var exercise = day.Exercises[j];
                    sb.AppendLine("        {");
                    sb.AppendLine($"          \"name\": \"{exercise.Name}\",");
                    sb.AppendLine($"          \"setsAndReps\": \"{exercise.RecommendedSets}x{exercise.RecommendedReps}\",");
                    sb.AppendLine($"          \"instructions\": \"{exercise.Instructions?.Replace("\"", "\\\"")}\",");
                    sb.AppendLine($"          \"exerciseId\": {exercise.Id}");
                    sb.Append("        }");
                    if (j < day.Exercises.Count - 1) sb.AppendLine(",");
                    else sb.AppendLine();
                }

                sb.AppendLine("      ]");
                sb.Append("    }");
                if (i < workoutDays.Count - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion
    }
}
