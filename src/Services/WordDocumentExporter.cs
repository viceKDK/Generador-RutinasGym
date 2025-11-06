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
                // For MVP: reuse CreateWordCompatibleHTML and write .doc HTML file
                var clientName = string.Empty;
                var html = CreateWordCompatibleHTML(routineContent ?? string.Empty, clientName);
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
    }
}
