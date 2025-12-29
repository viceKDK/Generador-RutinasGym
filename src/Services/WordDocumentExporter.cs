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
using GymRoutineGenerator.Domain;
using GymRoutineGenerator.Domain.Models;
using GymRoutineGenerator.Infrastructure;

namespace GymRoutineGenerator.Services
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

        private static string NormalizeForComparison(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var t = RemoveDiacritics(text);
            t = t.Replace("Ã¡", "a").Replace("Ã©", "e").Replace("Ã­", "i").Replace("Ã³", "o").Replace("Ãº", "u").Replace("Ã±", "n");
            t = t.Replace("tÃ©cnica", "tecnica").Replace("tǸcnica", "tecnica");
            var sb = new StringBuilder();
            foreach (var ch in t)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == ' ') sb.Append(ch);
            }
            return sb.ToString().ToLowerInvariant();
        }

        private void ProcessContentForWord(StringBuilder html, string routineContent)
        {
            // Implementación simplificada y robusta que convierte cada línea en un párrafo HTML
            if (string.IsNullOrEmpty(routineContent)) return;

            var lines = routineContent.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    html.AppendLine("<br/>");
                    continue;
                }

                // Escapar HTML y limpiar emojis/char problemáticos
                var safe = CleanEmojis(trimmed);
                safe = System.Net.WebUtility.HtmlEncode(safe);
                html.AppendLine($"<p>{safe}</p>");
            }
        }

        private static string CleanEmojis(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Eliminar caracteres surrogates (muchos emojis) y caracteres de control raros
            var cleaned = System.Text.RegularExpressions.Regex.Replace(input, "[\uD800-\uDFFF]", string.Empty);
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "[\u0000-\u001F&&[^\r\n\t]]", string.Empty);
            return cleaned;
        }

        private static string CreateCleanTextDocument(string routineContent, string clientName)
        {
            // Simple plain-text fallback
            var header = string.IsNullOrWhiteSpace(clientName) ? "Rutina" : $"Rutina para {clientName}";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            sb.AppendLine(new string('-', 40));
            sb.AppendLine(routineContent ?? string.Empty);
            return sb.ToString();
        }

        private static string CreatePDFReadyHTML(string routineContent, string clientName)
        {
            // Minimal HTML suitable for PDF conversion tools
            var title = string.IsNullOrWhiteSpace(clientName) ? "Rutina" : $"Rutina - {System.Net.WebUtility.HtmlEncode(clientName)}";
            var body = System.Net.WebUtility.HtmlEncode(routineContent ?? string.Empty).Replace("\n", "<br/>\n");
            return $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>{title}</title></head><body><h1>{title}</h1><div>{body}</div></body></html>";
        }

        public async Task<bool> ExportRoutineWithImagesAsync(
            string filePath,
            string routineContent,
            List<GymRoutineGenerator.Domain.WorkoutDay> plan,
            SQLiteExerciseImageDatabase imageDatabase,
            GymRoutineGenerator.Domain.Models.ExerciseSelectionEntry[]? manualSelection = null)
        {
            try
            {
                // Retrieve client name from the file path or generate a default
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var clientName = fileName.Replace("Rutina_", "").Split('_')[0];

                // Create rich HTML content from the structured plan
                var html = CreateRichWordHtml(plan, clientName);
                
                var wordFilePath = Path.ChangeExtension(filePath, ".doc");
                await File.WriteAllTextAsync(wordFilePath, html, Encoding.UTF8);

                // Optionally write a text backup
                var textFilePath = Path.ChangeExtension(filePath, ".txt");
                await File.WriteAllTextAsync(textFilePath, CreateCleanTextDocument(routineContent, clientName), Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string CreateRichWordHtml(List<GymRoutineGenerator.Domain.WorkoutDay> plan, string clientName)
        {
            var html = new StringBuilder();

            // Header standard
            html.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office'");
            html.AppendLine("xmlns:w='urn:schemas-microsoft-com:office:word'");
            html.AppendLine("xmlns='http://www.w3.org/TR/REC-html40'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine($"<title>Rutina de Gimnasio - {clientName}</title>");
            
            // Styles
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { font-family: 'Calibri', sans-serif; font-size: 11pt; }
                .main-title { font-size: 24pt; color: #2E74B5; text-align: center; font-weight: bold; margin-bottom: 20px; }
                .day-title { font-size: 16pt; color: #1F4E79; border-bottom: 2px solid #1F4E79; margin-top: 30px; margin-bottom: 10px; padding-bottom: 5px; }
                .exercise-container { margin-bottom: 20px; page-break-inside: avoid; border: 1px solid #ddd; padding: 10px; }
                .exercise-header { background-color: #f2f2f2; padding: 5px; font-weight: bold; font-size: 13pt; display: flex; justify-content: space-between; align-items: center; }
                .exercise-details { margin-top: 5px; }
                .exercise-img { max-width: 250px; height: auto; display: block; margin: 10px auto; border: 1px solid #ccc; }
                .sets-reps { font-weight: bold; color: #C00000; }
                .video-link { color: #0000FF; text-decoration: underline; margin-left: 10px; font-size: 10pt; }
                .summary-section { margin-top: 50px; border-top: 3px double #333; padding-top: 20px; }
                .summary-title { font-size: 18pt; text-align: center; text-decoration: underline; margin-bottom: 20px; }
                table { width: 100%; border-collapse: collapse; }
                td { vertical-align: top; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Main Title
            html.AppendLine($"<div class='main-title'>RUTINA DE ENTRENAMIENTO - {clientName.ToUpper()}</div>");

            // --- SECTION 1: DETAILED ROUTINE WITH IMAGES & VIDEOS ---
            if (plan != null)
            {
                foreach (var day in plan)
                {
                    html.AppendLine($"<div class='day-title'>{day.Name.ToUpper()} - {string.Join(", ", day.MuscleGroups)}</div>");

                    foreach (var exercise in day.Exercises)
                    {
                        html.AppendLine("<div class='exercise-container'>");
                        
                        // Header row with Name and Video Link
                        html.AppendLine("<div class='exercise-header'>");
                        html.Append($"<span>{exercise.Name}</span>");
                        
                        if (!string.IsNullOrWhiteSpace(exercise.VideoUrl))
                        {
                            html.Append($"<a href='{exercise.VideoUrl}' class='video-link' target='_blank'>[VER VIDEO]</a>");
                        }
                        html.AppendLine("</div>");

                        // Content table: Image on right (or bottom), Details on left
                        html.AppendLine("<table><tr>");
                        
                        // Details Column
                        html.AppendLine("<td style='width:60%; padding-right:15px;'>");
                        if (!string.IsNullOrWhiteSpace(exercise.SetsAndReps))
                        {
                            html.AppendLine($"<p><span class='sets-reps'>Series y Repeticiones:</span> {exercise.SetsAndReps}</p>");
                        }
                        
                        if (!string.IsNullOrWhiteSpace(exercise.Instructions))
                        {
                            html.AppendLine($"<p><b>Instrucciones:</b> {exercise.Instructions}</p>");
                        }
                        else if (!string.IsNullOrWhiteSpace(exercise.Description))
                        {
                             html.AppendLine($"<p><b>Descripción:</b> {exercise.Description}</p>");
                        }
                        html.AppendLine("</td>");

                        // Image Column
                        html.AppendLine("<td style='width:40%; text-align:center;'>");
                        if (exercise.ImageData != null && exercise.ImageData.Length > 0)
                        {
                            var base64 = Convert.ToBase64String(exercise.ImageData);
                            html.AppendLine($"<img src='data:image/jpeg;base64,{base64}' class='exercise-img' alt='{exercise.Name}' />");
                        }
                        else if (!string.IsNullOrWhiteSpace(exercise.ImagePath) && File.Exists(exercise.ImagePath))
                        {
                            // Try to embed local file if possible, otherwise just skip or link
                            try
                            {
                                var bytes = File.ReadAllBytes(exercise.ImagePath);
                                var base64 = Convert.ToBase64String(bytes);
                                var ext = Path.GetExtension(exercise.ImagePath).TrimStart('.').ToLower();
                                if (ext == "jpg") ext = "jpeg";
                                html.AppendLine($"<img src='data:image/{ext};base64,{base64}' class='exercise-img' alt='{exercise.Name}' />");
                            }
                            catch { /* Ignore image load error */ }
                        }
                        html.AppendLine("</td>");

                        html.AppendLine("</tr></table>");
                        html.AppendLine("</div>"); // End exercise-container
                    }
                }
            }

            // --- SECTION 2: SIMPLIFIED SUMMARY (TEXT ONLY) ---
            html.AppendLine("<div class='summary-section'>");
            html.AppendLine("<div class='summary-title'>RESUMEN SIMPLIFICADO (SOLO TEXTO)</div>");
            
            if (plan != null)
            {
                foreach (var day in plan)
                {
                    html.AppendLine($"<h3 style='color:#333; border-bottom:1px solid #ccc; margin-top:20px;'>{day.Name}</h3>");
                    html.AppendLine("<ul>");
                    foreach (var exercise in day.Exercises)
                    {
                        var seriesInfo = !string.IsNullOrWhiteSpace(exercise.SetsAndReps) ? $" - {exercise.SetsAndReps}" : "";
                        // Plain text, no links, no images
                        html.AppendLine($"<li><b>{exercise.Name}</b>{seriesInfo}</li>");
                    }
                    html.AppendLine("</ul>");
                }
            }
            html.AppendLine("</div>");

            // Footer
            html.AppendLine("<br/><hr/>");
            html.AppendLine("<p style='text-align:center; font-size:9pt; color:#666;'>Generado automticamente por GymRoutineGenerator</p>");

            html.AppendLine("</body></html>");
            return html.ToString();
        }
    }
}
