using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GymRoutineGenerator.UI
{
    public class ImprovedExportService
    {
        public async Task<bool> ExportToWordAsync(string filePath, string routineContent, string clientName)
        {
            try
            {
                // Create RTF document with Word-compatible formatting
                var rtfContent = CreateRTFDocument(routineContent, clientName);

                // Save as RTF file that can be opened by Word and saved as DOCX
                var rtfFilePath = Path.ChangeExtension(filePath, ".rtf");
                await File.WriteAllTextAsync(rtfFilePath, rtfContent, Encoding.UTF8);

                // If user selected DOCX, create both RTF and a simple text file
                if (filePath.EndsWith(".docx"))
                {
                    var textContent = CreatePlainTextDocument(routineContent, clientName);
                    var textFilePath = Path.ChangeExtension(filePath, ".txt");
                    await File.WriteAllTextAsync(textFilePath, textContent, Encoding.UTF8);

                    // Rename RTF file to show it can be opened with Word
                    var wordCompatiblePath = filePath.Replace(".docx", "_Word_Compatible.rtf");
                    if (File.Exists(rtfFilePath))
                    {
                        File.Move(rtfFilePath, wordCompatiblePath);
                    }
                }

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
                // Create HTML document that can be converted to PDF
                var htmlContent = CreateHTMLDocument(routineContent, clientName);

                // Save as HTML file with PDF-ready styling
                var htmlFilePath = Path.ChangeExtension(filePath, ".html");
                await File.WriteAllTextAsync(htmlFilePath, htmlContent, Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string CreateRTFDocument(string routineContent, string clientName)
        {
            var rtf = new StringBuilder();

            // RTF Header with proper formatting
            rtf.AppendLine(@"{\rtf1\ansi\deff0");
            rtf.AppendLine(@"{\fonttbl{\f0\fswiss\fcharset0 Arial;}{\f1\fswiss\fcharset0 Arial Bold;}{\f2\fmodern\fcharset0 Courier New;}}");
            rtf.AppendLine(@"{\colortbl;\red0\green0\blue0;\red25\green135\blue84;\red220\green53\blue69;\red13\green110\blue253;}");

            // Document title
            rtf.AppendLine(@"\f1\fs28\cf2\b\qc GENERADOR DE RUTINAS DE GIMNASIO\par");
            rtf.AppendLine(@"\f0\fs20\cf1\b0\qc\par");

            // Timestamp
            rtf.AppendLine($@"\f0\fs18\cf1\qc Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\par");
            rtf.AppendLine(@"\par\par");

            // Process content
            var lines = routineContent.Split('\n');
            foreach (var line in lines)
            {
                var formattedLine = FormatRTFLine(line.Trim());
                rtf.AppendLine(formattedLine + @"\par");
            }

            // Footer
            rtf.AppendLine(@"\par\par");
            rtf.AppendLine(@"\f0\fs14\cf1\i\qc Documento compatible con Microsoft Word\par");
            rtf.AppendLine(@"\f0\fs12\cf1\qc Para ms informacin, contacte con su entrenador personal.\par");

            rtf.AppendLine("}");
            return rtf.ToString();
        }

        private string CreatePlainTextDocument(string routineContent, string clientName)
        {
            var content = new StringBuilder();

            content.AppendLine("========================================");
            content.AppendLine("    GENERADOR DE RUTINAS DE GIMNASIO    ");
            content.AppendLine("========================================");
            content.AppendLine();
            content.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
            content.AppendLine();

            // Clean content by removing emojis and special characters
            var cleanContent = routineContent
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "")
                .Replace("", "- ")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "|")
                .Replace("", "-")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "+")
                .Replace("", "|")
                .Replace("", "-")
                .Replace("", "+")
                .Replace("", "+");

            content.AppendLine(cleanContent);
            content.AppendLine();
            content.AppendLine("========================================");
            content.AppendLine("Documento compatible con Microsoft Word");
            content.AppendLine("Abra el archivo RTF adjunto en Word para");
            content.AppendLine("obtener el formato completo con colores.");
            content.AppendLine("========================================");

            return content.ToString();
        }

        private string CreateHTMLDocument(string routineContent, string clientName)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"<title>Rutina Personalizada - {clientName}</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                @media print {
                    body { margin: 0; }
                    .no-print { display: none; }
                }
                body {
                    font-family: 'Arial', sans-serif;
                    line-height: 1.6;
                    margin: 0;
                    padding: 20px;
                    background-color: white;
                    color: #212529;
                }
                .container {
                    max-width: 800px;
                    margin: 0 auto;
                    background: white;
                    padding: 40px;
                    border-radius: 0;
                    box-shadow: none;
                }
                .header {
                    text-align: center;
                    color: #198754;
                    margin-bottom: 30px;
                    padding-bottom: 20px;
                    border-bottom: 3px solid #198754;
                }
                .header h1 {
                    margin: 0;
                    font-size: 24px;
                }
                .section-title {
                    color: #0d6efd;
                    font-weight: bold;
                    font-size: 16px;
                    margin: 20px 0 10px 0;
                }
                .exercise-day {
                    background: #f8f9fa;
                    padding: 15px;
                    margin: 15px 0;
                    border-radius: 5px;
                    border-left: 4px solid #0d6efd;
                }
                .exercise-list {
                    background: #f8f9fa;
                    padding: 15px;
                    margin: 10px 0;
                    border-radius: 5px;
                    font-family: 'Courier New', monospace;
                    font-size: 13px;
                    white-space: pre-wrap;
                }
                .recommendations {
                    background: #fff3cd;
                    padding: 20px;
                    margin: 20px 0;
                    border-radius: 5px;
                    border-left: 4px solid #ffc107;
                }
                .info-box {
                    background: #d1ecf1;
                    padding: 15px;
                    margin: 15px 0;
                    border-radius: 5px;
                    border-left: 4px solid #17a2b8;
                }
                .footer {
                    text-align: center;
                    margin-top: 40px;
                    padding-top: 20px;
                    border-top: 1px solid #dee2e6;
                    color: #6c757d;
                    font-size: 12px;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");

            // Process content
            var lines = routineContent.Split('\n');
            bool inExerciseSection = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (trimmedLine.Contains("RUTINA PERSONALIZADA"))
                {
                    html.AppendLine($"<div class='header'><h1>{CleanForHTML(trimmedLine)}</h1></div>");
                }
                else if (trimmedLine.StartsWith("") || trimmedLine.StartsWith(""))
                {
                    html.AppendLine($"<div class='section-title'>{CleanForHTML(trimmedLine)}</div>");
                }
                else if (trimmedLine.Contains("DA") && trimmedLine.Contains("TREN"))
                {
                    if (inExerciseSection) html.AppendLine("</div>");
                    html.AppendLine($"<div class='exercise-day'><h3>{CleanForHTML(trimmedLine)}</h3>");
                    inExerciseSection = true;
                }
                else if (trimmedLine.StartsWith(""))
                {
                    if (inExerciseSection) { html.AppendLine("</div>"); inExerciseSection = false; }
                    html.AppendLine($"<div class='recommendations'><h3>{CleanForHTML(trimmedLine)}</h3>");
                }
                else if (trimmedLine.Contains("") || trimmedLine.Contains("") || trimmedLine.Contains("") ||
                         trimmedLine.Contains("") || trimmedLine.Contains("") || trimmedLine.Contains(""))
                {
                    html.AppendLine($"<div class='exercise-list'>{CleanForHTML(trimmedLine)}</div>");
                }
                else if (trimmedLine.StartsWith(""))
                {
                    html.AppendLine($"<p>{CleanForHTML(trimmedLine)}</p>");
                }
                else
                {
                    html.AppendLine($"<p>{CleanForHTML(trimmedLine)}</p>");
                }
            }

            if (inExerciseSection) html.AppendLine("</div>");

            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine($"<p>Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("<p>Generador de Rutinas de Gimnasio - Rutina personalizada</p>");
            html.AppendLine("<p class='no-print'>Para convertir a PDF: Archivo > Imprimir > Guardar como PDF</p>");
            html.AppendLine("</div>");

            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string FormatRTFLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return @"\par";

            // Main headers
            if (line.Contains("RUTINA PERSONALIZADA") || line.Contains(""))
            {
                return @$"\f1\fs24\cf2\b\qc {EscapeRTF(line)}\b0\ql";
            }

            // Section headers
            if (line.StartsWith("") || line.StartsWith("") || line.StartsWith("") || line.StartsWith(""))
            {
                return @$"\f1\fs18\cf4\b {EscapeRTF(line)}\b0";
            }

            // Day headers
            if (line.Contains("DA") || line.Contains("TREN"))
            {
                return @$"\f1\fs16\cf2\b {EscapeRTF(line)}\b0";
            }

            // Exercise lines
            if (line.Contains("1") || line.Contains("2") || line.Contains("3") ||
                line.Contains("4") || line.Contains("5") || line.Contains("6"))
            {
                return @$"\f0\fs14\cf1 {EscapeRTF(line)}";
            }

            // Recommendations
            if (line.StartsWith(""))
            {
                return @$"\f0\fs14\cf2 {EscapeRTF(line)}";
            }

            // Box drawing - use smaller font
            if (line.Contains("") || line.Contains("") || line.Contains(""))
            {
                return @$"\f2\fs12\cf1 {EscapeRTF(line)}";
            }

            // Regular text
            return @$"\f0\fs14\cf1 {EscapeRTF(line)}";
        }

        private string EscapeRTF(string text)
        {
            return text.Replace(@"\", @"\\")
                      .Replace("{", @"\{")
                      .Replace("}", @"\}")
                      .Replace("", "")
                      .Replace("", "[Usuario] ")
                      .Replace("", "[Objetivos] ")
                      .Replace("", "[Importante] ")
                      .Replace("", "[Rutina] ")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "- ");
        }

        private string CleanForHTML(string text)
        {
            return text.Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("&", "&amp;");
        }
    }
}