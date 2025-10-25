using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using WColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using GymRoutineGenerator.UI.Models;

namespace GymRoutineGenerator.UI
{
    public class WordDocumentExporter
    {
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
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Document header
            html.AppendLine("<div class='header'>GENERADOR DE RUTINAS DE GIMNASIO</div>");

            // Process content
            ProcessContentForWord(html, routineContent);

            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Documento generado automticamente</p>");
            html.AppendLine("<p>Para ms informacin, contacte con su entrenador personal</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
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

                // Skip the main header as we already added it
                if (trimmedLine.Contains("RUTINA PERSONALIZADA")) continue;
                if (trimmedLine.Contains("")) continue;

                // Section headers
                if (trimmedLine.Contains("INFORMACIN PERSONAL") || trimmedLine.Contains(""))
                {
                    if (inExerciseSection) { html.AppendLine("</table>"); inExerciseSection = false; }
                    if (inWarnings) { html.AppendLine("</div>"); inWarnings = false; }

                    html.AppendLine("<div class='section-header'> INFORMACIN PERSONAL</div>");
                    html.AppendLine("<table class='info-table'>");
                    inPersonalInfo = true;
                    continue;
                }

                if (trimmedLine.Contains("OBJETIVOS") || trimmedLine.Contains(""))
                {
                    if (inPersonalInfo) { html.AppendLine("</table>"); inPersonalInfo = false; }

                    html.AppendLine("<div class='section-header'> OBJETIVOS SELECCIONADOS</div>");
                    html.AppendLine("<ul>");
                    inGoals = true;
                    continue;
                }

                if (trimmedLine.Contains("RUTINA DE ENTRENAMIENTO") || trimmedLine.Contains(""))
                {
                    if (inGoals) { html.AppendLine("</ul>"); inGoals = false; }

                    html.AppendLine("<div class='section-header'> RUTINA DE ENTRENAMIENTO</div>");
                    continue;
                }

                if (trimmedLine.Contains("IMPORTANTE") || trimmedLine.Contains(""))
                {
                    if (inExerciseSection) { html.AppendLine("</table>"); inExerciseSection = false; }
                    // Skip recomendaciones section
                    inWarnings = true;
                    continue;
                }

                // Day headers (exercise sections)
                if (trimmedLine.Contains("DA") && trimmedLine.Contains("TREN"))
                {
                    if (inExerciseSection) html.AppendLine("</table>");

                    html.AppendLine($"<div class='subsection-header'>{CleanEmojis(trimmedLine)}</div>");
                    html.AppendLine("<table class='exercise-table'>");
                    html.AppendLine("<tr><th>Ejercicio</th><th>Series y Repeticiones</th></tr>");
                    inExerciseSection = true;
                    continue;
                }

                // Content processing based on current section
                if (inPersonalInfo && trimmedLine.StartsWith(""))
                {
                    var parts = trimmedLine.Substring(1).Trim().Split(':', 2);
                    if (parts.Length == 2)
                    {
                        html.AppendLine($"<tr><td><strong>{CleanEmojis(parts[0]).Trim()}</strong></td><td>{CleanEmojis(parts[1]).Trim()}</td></tr>");
                    }
                }
                else if (inGoals && (trimmedLine.StartsWith(" ") || trimmedLine.StartsWith("")))
                {
                    var goalText = trimmedLine.Replace(" ", "").Replace("", "").Replace("", "").Trim();
                    if (!string.IsNullOrEmpty(goalText))
                    {
                        html.AppendLine($"<li>{CleanEmojis(goalText)}</li>");
                    }
                }
                else if (inExerciseSection && (trimmedLine.Contains(". ") && !trimmedLine.StartsWith("")))
                {
                    // Exercise line
                    var parts = trimmedLine.Split(" - ", 2);
                    if (parts.Length == 2)
                    {
                        var exercise = parts[0].Trim();
                        var sets = parts[1].Trim();
                        html.AppendLine($"<tr><td>{CleanEmojis(exercise)}</td><td>{sets}</td></tr>");
                    }
                }
                else if (inWarnings && (trimmedLine.StartsWith("") || trimmedLine.StartsWith("")))
                {
                    var warningText = trimmedLine.Replace("", "").Replace("", "").Trim();
                    if (!string.IsNullOrEmpty(warningText))
                    {
                        html.AppendLine($"<li>{CleanEmojis(warningText)}</li>");
                    }
                }
                else if (!trimmedLine.Contains("") && !trimmedLine.Contains("") && !trimmedLine.Contains("") &&
                         !trimmedLine.Contains("") && !trimmedLine.Contains("") && !trimmedLine.Contains(""))
                {
                    // Regular text
                    if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith(""))
                    {
                        html.AppendLine($"<p>{CleanEmojis(trimmedLine)}</p>");
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
                var cleanLine = CleanEmojis(line)
                    .Replace("", "+").Replace("", "+")
                    .Replace("", "+").Replace("", "+")
                    .Replace("", "|").Replace("", "-")
                    .Replace("", "+").Replace("", "+")
                    .Replace("", "+").Replace("", "+")
                    .Replace("", "|").Replace("", "=");

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
            html.AppendLine("<p>Para convertir a PDF: Archivo  Imprimir  Guardar como PDF</p>");
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
                if (trimmedLine.Contains("")) continue;

                if (trimmedLine.Contains("INFORMACIN PERSONAL") || trimmedLine.Contains(""))
                {
                    html.AppendLine("<div class='section-header'> INFORMACIN PERSONAL</div>");
                    html.AppendLine("<div class='info-section'>");
                    continue;
                }

                if (trimmedLine.Contains("OBJETIVOS") || trimmedLine.Contains(""))
                {
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='section-header'> OBJETIVOS</div>");
                    html.AppendLine("<div class='info-section'>");
                    continue;
                }

                if (trimmedLine.Contains("RUTINA DE ENTRENAMIENTO") || trimmedLine.Contains(""))
                {
                    html.AppendLine("</div>");
                    html.AppendLine("<div class='section-header'> RUTINA DE ENTRENAMIENTO</div>");
                    continue;
                }

                if (trimmedLine.Contains("DA") && trimmedLine.Contains("TREN"))
                {
                    if (inExerciseSection) html.AppendLine("</ol></div>");

                    html.AppendLine("<div class='exercise-section'>");
                    html.AppendLine($"<div class='exercise-title'>{CleanEmojis(trimmedLine)}</div>");
                    html.AppendLine("<ol class='exercise-list'>");
                    inExerciseSection = true;
                    continue;
                }

                if (trimmedLine.Contains("IMPORTANTE") || trimmedLine.Contains(""))
                {
                    if (inExerciseSection) { html.AppendLine("</ol></div>"); inExerciseSection = false; }

                    // Omitir secciones de advertencias/recomendaciones automÃ¡ticas
                    continue;
                }

                if (inExerciseSection && trimmedLine.Contains(". ") && !trimmedLine.StartsWith(""))
                {
                    var cleanExercise = CleanEmojis(trimmedLine);
                    html.AppendLine($"<li>{cleanExercise}</li>");
                }
                else if (trimmedLine.StartsWith("") || trimmedLine.StartsWith(""))
                {
                    var cleanText = CleanEmojis(trimmedLine.Replace("", "").Replace("", "").Trim());
                    if (!string.IsNullOrEmpty(cleanText))
                    {
                        html.AppendLine($"<p> {cleanText}</p>");
                    }
                }
            }

            if (inExerciseSection) html.AppendLine("</ol></div>");
            html.AppendLine("</div>");
        }

        private string CleanEmojis(string text)
        {
            return text.Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("1", "1.")
                      .Replace("2", "2.")
                      .Replace("3", "3.")
                      .Replace("4", "4.")
                      .Replace("5", "5.")
                      .Replace("6", "6.");
        }

        public async Task<bool> ExportRoutineWithImagesAsync(
            string filePath,
            string routineContent,
            List<WorkoutDay> workoutPlan,
            SQLiteExerciseImageDatabase imageDatabase,
            IReadOnlyList<ExerciseSelectionEntry>? manualSelection = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Validar parÃ¡metros
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        System.Diagnostics.Debug.WriteLine("Error: filePath es null o vacÃ­o");
                        return false;
                    }

                    if (workoutPlan == null || workoutPlan.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: workoutPlan es null o vacÃ­o");
                        return false;
                    }

                    // Cambiar extensiÃ³n a .docx para formato OpenXml
                    var docxFilePath = Path.ChangeExtension(filePath, ".docx");
                    System.Diagnostics.Debug.WriteLine($"Exportando a: {docxFilePath}");

                    // Crear buscador automÃ¡tico de imÃ¡genes
                    var imageFinder = new AutomaticImageFinder();
                    System.Diagnostics.Debug.WriteLine($"AutomaticImageFinder creado. ImÃ¡genes en cache: {imageFinder.GetCachedImageCount()}");

                    // Crear documento Word
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(docxFilePath, WordprocessingDocumentType.Document))
                    {
                        // Agregar parte principal del documento
                        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = new Body();

                        // TÃ­tulo principal
                        body.Append(CreateHeading("RUTINA DE GIMNASIO PERSONALIZADA", 1));
                        body.Append(CreateParagraph(""));

                        // InformaciÃ³n del documento
                        body.Append(CreateParagraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}"));
                        body.Append(CreateParagraph(""));

                        // Parsear informaciÃ³n personal del routineContent
                        var lines = routineContent.Split('\n');
                        bool inPersonalInfo = false;
                        bool inObjectives = false;

                        foreach (var line in lines)
                        {
                            var trimmed = line.Trim();

                            if (trimmed.Contains("INFORMACIÃ“N PERSONAL") || trimmed.Contains("INFORMACION PERSONAL"))
                            {
                                body.Append(CreateHeading("INFORMACIÃ“N PERSONAL", 2));
                                inPersonalInfo = true;
                                inObjectives = false;
                                continue;
                            }

                            if (trimmed.Contains("OBJETIVOS"))
                            {
                                body.Append(CreateParagraph(""));
                                body.Append(CreateHeading("OBJETIVOS SELECCIONADOS", 2));
                                inPersonalInfo = false;
                                inObjectives = true;
                                continue;
                            }

                            if (trimmed.Contains("RUTINA DE ENTRENAMIENTO"))
                            {
                                inPersonalInfo = false;
                                inObjectives = false;
                                break;
                            }

                            if (inPersonalInfo && trimmed.Length > 0 && !trimmed.Contains("â”") && !trimmed.Contains("â•"))
                            {
                                body.Append(CreateParagraph(CleanEmojis(trimmed)));
                            }

                            if (inObjectives && trimmed.Length > 0 && !trimmed.Contains("â”") && !trimmed.Contains("â•"))
                            {
                                body.Append(CreateBulletPoint(CleanEmojis(trimmed)));
                            }
                        }

                        // Rutina de entrenamiento
                        body.Append(CreateParagraph(""));
                        body.Append(CreateHeading("RUTINA DE ENTRENAMIENTO", 2));
                        body.Append(CreateParagraph(""));

                        // Agregar cada dÃ­a de entrenamiento
                        foreach (var day in workoutPlan)
                        {
                            body.Append(CreateHeading(day.Name, 3));
                            body.Append(CreateParagraph(""));

                            foreach (var exercise in day.Exercises)
                            {
                                // Nombre del ejercicio en negrita
                                body.Append(CreateParagraph($"{exercise.Name}", true));

                                // Series y repeticiones
                                body.Append(CreateParagraph($"  {exercise.SetsAndReps}"));

                                // Instrucciones
                                if (!string.IsNullOrWhiteSpace(exercise.Instructions))
                                {
                                    var norm = RemoveDiacritics(exercise.Instructions).ToLowerInvariant();
                                    var isDefault = norm.Contains("mantener tecnica correcta") || norm.Equals("mantener tecnica correcta");
                                    if (!isDefault)
                                    {
                                        body.Append(CreateParagraph($"  {exercise.Instructions}", false, true));
                                    }
                                }

                                // Buscar y agregar imagen automÃ¡ticamente
                                string? imagePath = null;

                                System.Diagnostics.Debug.WriteLine($"Buscando imagen para: {exercise.Name}");

                                // 1. Intentar buscar en la base de datos primero
                                var imageInfo = imageDatabase.FindExerciseImage(exercise.Name);
                                if (imageInfo != null && !string.IsNullOrWhiteSpace(imageInfo.ImagePath) && File.Exists(imageInfo.ImagePath))
                                {
                                    imagePath = imageInfo.ImagePath;
                                    System.Diagnostics.Debug.WriteLine($"  âœ“ Imagen encontrada en BD: {imagePath}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"  âœ— No se encontrÃ³ imagen en BD");
                                }

                                // 2. Si no estÃ¡ en BD, buscar automÃ¡ticamente en docs/ejercicios
                                if (string.IsNullOrEmpty(imagePath))
                                {
                                    imagePath = imageFinder.FindImageForExercise(exercise.Name);
                                    if (!string.IsNullOrEmpty(imagePath))
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  âœ“ Imagen encontrada por AutomaticImageFinder: {imagePath}");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  âœ— No se encontrÃ³ imagen en sistema de archivos");
                                    }
                                }

                                // 3. Insertar imagen si se encontrÃ³
                                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                                {
                                    try
                                    {
                                        InsertImage(mainPart, body, imagePath, 400, 300);
                                        System.Diagnostics.Debug.WriteLine($"  âœ“ Imagen insertada exitosamente");
                                    }
                                    catch (Exception imgEx)
                                    {
                                        // Si falla la imagen, continuar sin ella
                                        System.Diagnostics.Debug.WriteLine($"  âœ— Error al insertar imagen: {imgEx.Message}");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"  âš  No se insertÃ³ imagen para {exercise.Name}");
                                }

                                body.Append(CreateParagraph(""));
                            }

                            body.Append(CreateParagraph(""));
                        }
                        // Omitido: recomendaciones automaticas
                        if (manualSelection != null && manualSelection.Count > 0)
                        {
                            body.Append(CreateParagraph(""));
                            body.Append(CreateHeading("SELECCIÃ“N MANUAL DE EJERCICIOS", 2));
                            body.Append(CreateParagraph("Ejercicios agregados desde la galerÃ­a manual durante esta sesiÃ³n:"));
                            body.Append(CreateParagraph(""));

                            for (var index = 0; index < manualSelection.Count; index++)
                            {
                                var entry = manualSelection[index];
                                var summaryBuilder = new StringBuilder($"{index + 1}. {entry.DisplayName}");

                                if (!string.IsNullOrWhiteSpace(entry.Source))
                                {
                                    summaryBuilder.Append($" ({entry.Source})");
                                }

                                body.Append(CreateParagraph(summaryBuilder.ToString()));

                                if (entry.MuscleGroups.Count > 0)
                                {
                                    body.Append(CreateParagraph($"   Grupos musculares: {string.Join(", ", entry.MuscleGroups)}"));
                                }

                                if (!string.IsNullOrWhiteSpace(entry.ImagePath))
                                {
                                    body.Append(CreateParagraph($"   Imagen: {Path.GetFileName(entry.ImagePath)}"));
                                }
                            }
                        }

                        mainPart.Document.Append(body);
                        mainPart.Document.Save();
                    }

                    System.Diagnostics.Debug.WriteLine($"Documento Word exportado exitosamente a: {docxFilePath}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al exportar a Word: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    return false;
                }
            });
        }

        private Paragraph CreateHeading(string text, int level)
        {
            var para = new Paragraph();
            var run = new Run();
            var runProps = new RunProperties();

            // Configurar tamaÃ±o segÃºn nivel
            int fontSize = level switch
            {
                1 => 32, // 16pt
                2 => 28, // 14pt
                3 => 24, // 12pt
                _ => 22  // 11pt
            };

            runProps.Append(new Bold());
            runProps.Append(new FontSize { Val = fontSize.ToString() });

            // Color segÃºn nivel
            if (level == 1)
            {
                runProps.Append(new WColor { Val = "1F7A54" }); // Verde
            }
            else if (level == 2)
            {
                runProps.Append(new WColor { Val = "0D6EFD" }); // Azul
            }
            else
            {
                runProps.Append(new WColor { Val = "198754" }); // Verde mÃ¡s claro
            }

            run.Append(runProps);
            run.Append(new Text(text));
            para.Append(run);

            // Agregar espaciado
            var paraProps = new ParagraphProperties();
            paraProps.Append(new SpacingBetweenLines { After = "200", Before = "200" });
            para.InsertAt(paraProps, 0);

            return para;
        }

        private Paragraph CreateParagraph(string text, bool bold = false, bool italic = false)
        {
            var para = new Paragraph();
            var run = new Run();

            if (bold || italic)
            {
                var runProps = new RunProperties();
                if (bold) runProps.Append(new Bold());
                if (italic) runProps.Append(new Italic());
                run.Append(runProps);
            }

            run.Append(new Text(text));
            para.Append(run);
            return para;
        }

        private Paragraph CreateBulletPoint(string text)
        {
            var para = new Paragraph();

            var paraProps = new ParagraphProperties();
            var numProps = new NumberingProperties();
            numProps.Append(new NumberingLevelReference { Val = 0 });
            numProps.Append(new NumberingId { Val = 1 });
            paraProps.Append(numProps);
            para.Append(paraProps);

            var run = new Run();
            run.Append(new Text(text));
            para.Append(run);

            return para;
        }

        private void InsertImage(MainDocumentPart mainPart, Body body, string imagePath, int width, int height)
        {
            // Determinar tipo de imagen por extensiÃ³n y crear ImagePart apropiado
            var extension = Path.GetExtension(imagePath).ToLowerInvariant();

            ImagePart imagePart = extension switch
            {
                ".png" => mainPart.AddImagePart(ImagePartType.Png),
                ".jpg" or ".jpeg" => mainPart.AddImagePart(ImagePartType.Jpeg),
                ".gif" => mainPart.AddImagePart(ImagePartType.Gif),
                ".bmp" => mainPart.AddImagePart(ImagePartType.Bmp),
                ".webp" => mainPart.AddImagePart(ImagePartType.Png), // Webp no soportado, usar PNG
                _ => mainPart.AddImagePart(ImagePartType.Jpeg)
            };

            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = width * 9525L, Cy = height * 9525L },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = 1U, Name = "Picture 1" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties { Id = 0U, Name = "imagen.jpg" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip { Embed = mainPart.GetIdOfPart(imagePart) },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset { X = 0L, Y = 0L },
                                        new A.Extents { Cx = width * 9525L, Cy = height * 9525L }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                )
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U
                });

            var paragraph = new Paragraph(new Run(element));

            // Centrar imagen
            var paraProps = new ParagraphProperties();
            paraProps.Append(new Justification { Val = JustificationValues.Center });
            paragraph.InsertAt(paraProps, 0);

            body.Append(paragraph);
        }
    }
}



