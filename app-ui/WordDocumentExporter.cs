using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            html.AppendLine($"<div class='timestamp'>Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

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

                    html.AppendLine("<div class='warning-box'>");
                    html.AppendLine("<div class='subsection-header'> RECOMENDACIONES IMPORTANTES</div>");
                    html.AppendLine("<ul>");
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

                    html.AppendLine("<div class='warning-section'>");
                    html.AppendLine("<div class='exercise-title'> RECOMENDACIONES IMPORTANTES</div>");
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
    }
}