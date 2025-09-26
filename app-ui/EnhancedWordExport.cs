using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GymRoutineGenerator.UI
{
    public class EnhancedWordExport
    {
        public async Task<bool> ExportToWordAsync(string filePath, string routineContent, string clientName)
        {
            try
            {
                // Create RTF document with proper formatting
                var rtfContent = CreateRTFDocument(routineContent, clientName);

                // Save as RTF file (which can be opened by Word)
                var rtfFilePath = filePath.Replace(".docx", ".rtf");
                await File.WriteAllTextAsync(rtfFilePath, rtfContent, Encoding.UTF8);

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

            // RTF Header
            rtf.AppendLine(@"{\rtf1\ansi\deff0");
            rtf.AppendLine(@"{\fonttbl{\f0\fswiss\fcharset0 Segoe UI;}{\f1\fswiss\fcharset0 Segoe UI;}{\f2\fmodern\fcharset0 Consolas;}}");
            rtf.AppendLine(@"{\colortbl;\red33\green37\blue41;\red25\green135\blue84;\red220\green53\blue69;\red13\green110\blue253;}");

            // Document title
            rtf.AppendLine(@"\f0\fs32\cf2\b GENERADOR DE RUTINAS DE GIMNASIO\par");
            rtf.AppendLine(@"\fs24\cf1\b0\par");

            // Add generation timestamp
            rtf.AppendLine($@"\fs20\cf1 Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\par");
            rtf.AppendLine(@"\par");

            // Process content with formatting
            var lines = routineContent.Split('\n');
            foreach (var line in lines)
            {
                var formattedLine = FormatRTFLine(line.Trim());
                rtf.AppendLine(formattedLine + @"\par");
            }

            // Footer
            rtf.AppendLine(@"\par\par");
            rtf.AppendLine(@"\fs16\cf1\i Documento generado automticamente por el Generador de Rutinas de Gimnasio\par");
            rtf.AppendLine(@"\fs14 Para ms informacin, contacte con su entrenador personal.\par");

            rtf.AppendLine("}");

            return rtf.ToString();
        }

        private string FormatRTFLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return @"\par";

            // Main headers
            if (line.Contains("RUTINA PERSONALIZADA") || line.Contains(""))
            {
                return @$"\f0\fs28\cf2\b {EscapeRTF(line)}\b0";
            }

            // Section headers
            if (line.StartsWith("") || line.StartsWith("") || line.StartsWith("") || line.StartsWith(""))
            {
                return @$"\f0\fs22\cf4\b {EscapeRTF(line)}\b0";
            }

            // Day headers
            if (line.Contains("DA") || line.Contains("TREN"))
            {
                return @$"\f0\fs20\cf2\b {EscapeRTF(line)}\b0";
            }

            // Exercise lines with numbers
            if (line.Contains("1") || line.Contains("2") || line.Contains("3") ||
                line.Contains("4") || line.Contains("5") || line.Contains("6"))
            {
                return @$"\f1\fs18\cf1 {EscapeRTF(line)}";
            }

            // Box drawing characters - use monospace font
            if (line.Contains("") || line.Contains("") || line.Contains("") ||
                line.Contains("") || line.Contains("") || line.Contains(""))
            {
                return @$"\f2\fs16\cf1 {EscapeRTF(line)}";
            }

            // Recommendations
            if (line.StartsWith(""))
            {
                return @$"\f1\fs18\cf2 {EscapeRTF(line)}";
            }

            // Regular text
            return @$"\f1\fs18\cf1 {EscapeRTF(line)}";
        }

        private string EscapeRTF(string text)
        {
            return text.Replace(@"\", @"\\")
                      .Replace("{", @"\{")
                      .Replace("}", @"\}")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "- ")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "")
                      .Replace("", "");
        }

        public async Task<bool> ExportToPDFAsync(string filePath, string routineContent, string clientName)
        {
            try
            {
                // Create HTML content for PDF export
                var htmlContent = CreateHTMLDocument(routineContent, clientName);

                // Save as HTML file (can be converted to PDF)
                var htmlFilePath = filePath.Replace(".docx", ".html").Replace(".rtf", ".html");
                await File.WriteAllTextAsync(htmlFilePath, htmlContent, Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string CreateHTMLDocument(string routineContent, string clientName)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<title>Rutina Personalizada - " + clientName + "</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    margin: 20px;
                    background-color: #f8f9fa;
                    color: #212529;
                }
                .container {
                    max-width: 800px;
                    margin: 0 auto;
                    background: white;
                    padding: 40px;
                    border-radius: 12px;
                    box-shadow: 0 4px 20px rgba(0,0,0,0.1);
                }
                .header {
                    text-align: center;
                    color: #198754;
                    margin-bottom: 30px;
                    padding-bottom: 20px;
                    border-bottom: 3px solid #198754;
                }
                .section-title {
                    color: #0d6efd;
                    font-weight: bold;
                    font-size: 1.2em;
                    margin: 20px 0 10px 0;
                }
                .exercise-day {
                    background: #e7f3ff;
                    padding: 15px;
                    margin: 15px 0;
                    border-radius: 8px;
                    border-left: 4px solid #0d6efd;
                }
                .exercise-list {
                    background: #f8f9fa;
                    padding: 15px;
                    margin: 10px 0;
                    border-radius: 6px;
                    font-family: 'Courier New', monospace;
                }
                .recommendations {
                    background: #fff3cd;
                    padding: 20px;
                    margin: 20px 0;
                    border-radius: 8px;
                    border-left: 4px solid #ffc107;
                }
                .info-box {
                    background: #d1ecf1;
                    padding: 15px;
                    margin: 15px 0;
                    border-radius: 8px;
                    border-left: 4px solid #17a2b8;
                }
                pre {
                    white-space: pre-wrap;
                    font-family: 'Courier New', monospace;
                    font-size: 0.9em;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");

            // Process content
            var lines = routineContent.Split('\n');
            var currentSection = "";

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (trimmedLine.Contains("RUTINA PERSONALIZADA"))
                {
                    html.AppendLine($"<div class='header'><h1>{trimmedLine}</h1></div>");
                }
                else if (trimmedLine.StartsWith("") || trimmedLine.StartsWith(""))
                {
                    currentSection = trimmedLine;
                    html.AppendLine($"<div class='section-title'>{trimmedLine}</div>");
                }
                else if (trimmedLine.Contains("DA") && trimmedLine.Contains("TREN"))
                {
                    html.AppendLine($"<div class='exercise-day'><h3>{trimmedLine}</h3>");
                }
                else if (trimmedLine.Contains("") || trimmedLine.Contains("") || trimmedLine.Contains(""))
                {
                    html.AppendLine($"<div class='exercise-list'><pre>{trimmedLine}</pre></div>");
                }
                else if (trimmedLine.StartsWith(""))
                {
                    html.AppendLine($"<div class='recommendations'><h3>{trimmedLine}</h3>");
                }
                else if (trimmedLine.StartsWith(""))
                {
                    html.AppendLine($"<p>{trimmedLine}</p>");
                }
                else
                {
                    html.AppendLine($"<p>{trimmedLine}</p>");
                }
            }

            // Footer
            html.AppendLine("<hr style='margin-top: 40px;'>");
            html.AppendLine($"<p style='text-align: center; color: #6c757d; font-size: 0.9em;'>Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("<p style='text-align: center; color: #6c757d; font-size: 0.8em;'>Generador de Rutinas de Gimnasio - Rutina personalizada</p>");

            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}