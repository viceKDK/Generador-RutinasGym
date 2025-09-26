using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Core.Models;
using System.Diagnostics;
using System.IO;
using SystemDrawing = System.Drawing;
using SystemDrawingImaging = System.Drawing.Imaging;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.Infrastructure.Documents;

public class WordDocumentService : IWordDocumentService
{
    private readonly ILogger<WordDocumentService> _logger;
    private readonly TemplateManagerService _templateManager;
    private readonly RoutineFormatterService _routineFormatter;

    public WordDocumentService(ILogger<WordDocumentService>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<WordDocumentService>.Instance;
        _templateManager = new TemplateManagerService();
        _routineFormatter = new RoutineFormatterService();
    }

    public async Task<DocumentGenerationResult> GenerateRoutineDocumentAsync(RoutineDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var defaultTemplate = await GetDefaultTemplateAsync(cancellationToken);
        return await GenerateDocumentWithTemplateAsync(request, defaultTemplate, cancellationToken);
    }

    public async Task<DocumentGenerationResult> GenerateDocumentWithTemplateAsync(RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger?.LogInformation($"Starting document generation for client: {request.ClientName}");

        var result = new DocumentGenerationResult();

        try
        {
            // Step 1: Determine file path and name
            var fileName = GenerateFileName(request);
            var filePath = DetermineFilePath(request, fileName);

            // Step 2: Create Word document
            using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

            // Step 3: Add main document part
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Step 4: Apply template styles
            await ApplyTemplateStylesAsync(document, template, cancellationToken);

            // Step 5: Generate document content
            await GenerateDocumentContentAsync(body, request, template, cancellationToken);

            // Step 6: Add headers and footers
            await AddHeadersAndFootersAsync(mainPart, request, template, cancellationToken);

            // Step 7: Finalize document
            mainPart.Document.Save();

            // Step 8: Collect statistics
            var fileInfo = new FileInfo(filePath);

            stopwatch.Stop();

            result.Success = true;
            result.FilePath = filePath;
            result.FileName = fileName;
            result.FileSizeBytes = fileInfo.Length;
            result.GenerationTime = stopwatch.Elapsed;
            result.Metadata = CreateDocumentMetadata(request, template);
            result.Statistics = await CalculateStatisticsAsync(request, cancellationToken);

            _logger?.LogInformation($"Document generated successfully in {stopwatch.ElapsedMilliseconds}ms: {filePath}");

            // Step 9: Auto-open if requested
            if (request.Settings.AutoOpenAfterGeneration)
            {
                await OpenDocumentAsync(filePath, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating document for client: {request.ClientName}");

            result.Success = false;
            result.Errors.Add($"Document generation failed: {ex.Message}");
            result.GenerationTime = stopwatch.Elapsed;

            return result;
        }
    }

    public async Task<List<DocumentTemplate>> GetAvailableTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _templateManager.GetAvailableTemplatesAsync();
    }

    public async Task<DocumentTemplate> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        return await _templateManager.GetTemplateAsync(templateId);
    }

    public async Task<DocumentPreview> PreviewDocumentAsync(RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken = default)
    {
        return await _templateManager.PreviewTemplateAsync(template, request);
    }

    // Private implementation methods
    private async Task ApplyTemplateStylesAsync(WordprocessingDocument document, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(5, cancellationToken);

        // Add styles part
        var stylesPart = document.MainDocumentPart?.AddNewPart<StyleDefinitionsPart>();
        if (stylesPart != null)
        {
            stylesPart.Styles = CreateStyles(template.Style);
        }
    }

    private async Task GenerateDocumentContentAsync(Body body, RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);

        // Title page
        if (template.Layout.IncludeCoverPage)
        {
            GenerateCoverPage(body, request, template);
            AddPageBreak(body);
        }

        // Table of contents
        if (template.Layout.IncludeTableOfContents)
        {
            GenerateTableOfContents(body, request, template);
            AddPageBreak(body);
        }

        // Client information section
        GenerateClientInfoSection(body, request, template);

        // Goals section
        GenerateGoalsSection(body, request, template);

        // Safety section
        if (template.Layout.IncludeSafetySection)
        {
            GenerateSafetySection(body, request, template);
        }

        // Routine content
        if (request.WeeklyPrograms.Any())
        {
            await GenerateWeeklyProgramsAsync(body, request, template, cancellationToken);
        }
        else if (request.Routine != null)
        {
            await GenerateSingleRoutineAsync(body, request, template, cancellationToken);
        }

        // Progress tracking section
        if (template.Layout.IncludeProgressSection)
        {
            GenerateProgressSection(body, request, template);
        }

        // Additional notes
        GenerateNotesSection(body, request, template);
    }

    private async Task AddHeadersAndFootersAsync(MainDocumentPart mainPart, RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(5, cancellationToken);

        // Add header
        if (template.Style.HeaderFooter.HeaderText != null || template.Style.HeaderFooter.IncludeLogo)
        {
            var headerPart = mainPart.AddNewPart<HeaderPart>();
            headerPart.Header = CreateHeader(request, template);
        }

        // Add footer
        if (!string.IsNullOrEmpty(template.Style.HeaderFooter.FooterText) || template.Style.HeaderFooter.IncludePageNumbers)
        {
            var footerPart = mainPart.AddNewPart<FooterPart>();
            footerPart.Footer = CreateFooter(request, template);
        }
    }

    private void GenerateCoverPage(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        // Title
        var titleParagraph = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(new FontSize { Val = "48" }, new Bold()),
                new Text("RUTINA DE ENTRENAMIENTO PERSONALIZADA")
            )
        );
        body.AppendChild(titleParagraph);

        // Spacing
        body.AppendChild(CreateEmptyParagraph());
        body.AppendChild(CreateEmptyParagraph());

        // Client info
        var clientParagraph = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(new FontSize { Val = "24" }),
                new Text($"Cliente: {request.ClientName}")
            )
        );
        body.AppendChild(clientParagraph);

        // Date
        var dateParagraph = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(new FontSize { Val = "18" }),
                new Text($"Fecha: {request.CreationDate:dd/MM/yyyy}")
            )
        );
        body.AppendChild(dateParagraph);

        // Gym info
        if (!string.IsNullOrEmpty(request.GymName))
        {
            body.AppendChild(CreateEmptyParagraph());
            var gymParagraph = new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize { Val = "16" }),
                    new Text($"Gimnasio: {request.GymName}")
                )
            );
            body.AppendChild(gymParagraph);
        }

        // Trainer info
        if (!string.IsNullOrEmpty(request.TrainerName))
        {
            var trainerParagraph = new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new FontSize { Val = "16" }),
                    new Text($"Entrenador: {request.TrainerName}")
                )
            );
            body.AppendChild(trainerParagraph);
        }
    }

    private void GenerateClientInfoSection(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        // Section title
        var titleParagraph = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                new Text("INFORMACIÓN DEL CLIENTE")
            )
        );
        body.AppendChild(titleParagraph);

        // Client details table
        var table = new Table();

        // Table properties
        var tableProps = new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }, // 100% width
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
            )
        );
        table.AppendChild(tableProps);

        // Add rows
        table.AppendChild(CreateInfoRow("Nombre:", request.ClientName));
        table.AppendChild(CreateInfoRow("Edad:", $"{request.ClientAge} años"));
        table.AppendChild(CreateInfoRow("Género:", request.ClientGender));
        table.AppendChild(CreateInfoRow("Fecha de creación:", request.CreationDate.ToString("dd/MM/yyyy")));

        body.AppendChild(table);
        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateGoalsSection(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        if (request.Goals == null) return;

        // Section title
        var titleParagraph = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                new Text("OBJETIVOS")
            )
        );
        body.AppendChild(titleParagraph);

        // Primary goal
        if (!string.IsNullOrEmpty(request.Goals.PrimaryGoal))
        {
            var goalParagraph = new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Objetivo Principal: ")),
                new Run(new Text(request.Goals.PrimaryGoal))
            );
            body.AppendChild(goalParagraph);
        }

        // Secondary goals
        if (request.Goals.SecondaryGoals.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Objetivos Secundarios:"))
            ));

            foreach (var goal in request.Goals.SecondaryGoals)
            {
                body.AppendChild(new Paragraph(
                    new Run(new Text($"• {goal}"))
                ));
            }
        }

        // Timeframe
        if (!string.IsNullOrEmpty(request.Goals.TargetTimeframe))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Plazo objetivo: ")),
                new Run(new Text(request.Goals.TargetTimeframe))
            ));
        }

        // Motivational message
        if (!string.IsNullOrEmpty(request.Goals.MotivationalMessage))
        {
            body.AppendChild(CreateEmptyParagraph());
            var motivationParagraph = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "200", After = "200" }
                ),
                new Run(
                    new RunProperties(new Italic(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }),
                    new Text($"💪 {request.Goals.MotivationalMessage}")
                )
            );
            body.AppendChild(motivationParagraph);
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateSafetySection(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        if (!request.SafetyNotes.Any()) return;

        // Section title
        var titleParagraph = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = "FF0000" }), // Red for safety
                new Text("⚠️ CONSIDERACIONES DE SEGURIDAD")
            )
        );
        body.AppendChild(titleParagraph);

        // Safety notes
        foreach (var safetyNote in request.SafetyNotes.OrderBy(s => s.Priority))
        {
            var priorityIcon = safetyNote.Priority switch
            {
                SafetyPriority.Critical => "🚨",
                SafetyPriority.High => "⚠️",
                SafetyPriority.Medium => "⚡",
                _ => "💡"
            };

            var noteParagraph = new Paragraph(
                new Run(
                    new RunProperties(new Bold()),
                    new Text($"{priorityIcon} {safetyNote.Category}: ")
                ),
                new Run(new Text(safetyNote.Note))
            );
            body.AppendChild(noteParagraph);

            // Warning signs
            if (safetyNote.WarningSignsToStop.Any())
            {
                body.AppendChild(new Paragraph(
                    new Run(
                        new RunProperties(new Italic()),
                        new Text($"   Señales de alarma: {string.Join(", ", safetyNote.WarningSignsToStop)}")
                    )
                ));
            }

            body.AppendChild(CreateEmptyParagraph());
        }
    }

    private async Task GenerateSingleRoutineAsync(Body body, RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);

        var routine = request.Routine;

        // Routine title
        var titleParagraph = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                new Text($"RUTINA: {routine.RoutineName}")
            )
        );
        body.AppendChild(titleParagraph);

        // Routine info
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Duración estimada: ")),
            new Run(new Text($"{routine.EstimatedDuration.TotalMinutes:F0} minutos"))
        ));

        body.AppendChild(CreateEmptyParagraph());

        // Client information section
        GenerateClientInfoSection(body, request, template);

        // Weekly program structure (day-by-day breakdown)
        if (request.WeeklyPrograms.Any())
        {
            _routineFormatter.FormatDayByDayBreakdown(body, request.WeeklyPrograms, template);
        }
        // Single routine fallback
        else if (request.Routine != null)
        {
            GenerateSingleRoutineSection(body, request.Routine, template);
        }

        // Progress tracking section
        _routineFormatter.FormatProgressTrackingSection(body, template);

        // Safety section
        if (template.Layout.IncludeSafetySection && request.SafetyNotes.Any())
        {
            GenerateSafetySection(body, request.SafetyNotes, template);
        }
    }

    private void AddInfoRow(Table table, string label, string value, DocumentTemplate template)
    {
        var row = new TableRow();

        var labelCell = new TableCell(new Paragraph(new Run(new RunProperties(new Bold()), new Text(label))));
        labelCell.AppendChild(new TableCellProperties(
            new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F0F0F0" }
        ));

        var valueCell = new TableCell(new Paragraph(new Run(new Text(value))));

        row.AppendChild(labelCell);
        row.AppendChild(valueCell);
        table.AppendChild(row);
    }

    private void GenerateSingleRoutineSection(Body body, object routine, DocumentTemplate template)
    {
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "18" }),
                    new Text("RUTINA DE ENTRENAMIENTO"))
        ));
        body.AppendChild(CreateEmptyParagraph());

        // Simple fallback implementation for single routines
        body.AppendChild(new Paragraph(
            new Run(new Text("Esta rutina será formateada con el nuevo sistema de formato."))
        ));
        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateSafetySection(Body body, List<DocumentSafetyNote> safetyNotes, DocumentTemplate template)
    {
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "16" },
                    new Color { Val = "FF0000" }),
                    new Text("⚠️ INFORMACIÓN DE SEGURIDAD"))
        ));
        body.AppendChild(CreateEmptyParagraph());

        foreach (var note in safetyNotes.OrderByDescending(n => n.Priority))
        {
            var priorityIcon = note.Priority switch
            {
                SafetyPriority.Critical => "🚨",
                SafetyPriority.High => "⚠️",
                SafetyPriority.Medium => "⚡",
                _ => "ℹ️"
            };

            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text($"{priorityIcon} {note.Category}: ")),
                new Run(new Text(note.Note))
            ));

            if (note.WarningSignsToStop.Any())
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new Indentation { Left = "300" }),
                    new Run(new RunProperties(new Bold(), new Color { Val = "FF0000" }), new Text("🛑 Detente si experimentas: ")),
                    new Run(new Text(string.Join(", ", note.WarningSignsToStop)))
                ));
            }
        }
        body.AppendChild(CreateEmptyParagraph());
    }

    private async Task GenerateWeeklyProgramsAsync(Body body, RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(30, cancellationToken);

        foreach (var week in request.WeeklyPrograms.OrderBy(w => w.WeekNumber))
        {
            // Week title
            var weekTitle = new Paragraph(
                new Run(
                    new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                    new Text($"SEMANA {week.WeekNumber}: {week.WeekFocus}")
                )
            );
            body.AppendChild(weekTitle);

            // Week notes
            if (!string.IsNullOrEmpty(week.WeekNotes))
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Italic()), new Text(week.WeekNotes))
                ));
                body.AppendChild(CreateEmptyParagraph());
            }

            // Daily workouts
            foreach (var day in week.DailyWorkouts.OrderBy(d => d.DayNumber))
            {
                await GenerateDailyWorkoutAsync(body, day, template, cancellationToken);
                body.AppendChild(CreateEmptyParagraph());
            }

            // Page break between weeks
            AddPageBreak(body);
        }
    }

    private async Task GenerateDailyWorkoutAsync(Body body, DailyWorkout day, DocumentTemplate template, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);

        // Day title
        var dayTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "20" }, new Bold(), new Color { Val = template.Style.ColorScheme.SecondaryColor.Replace("#", "") }),
                new Text($"DÍA {day.DayNumber}: {day.WorkoutName}")
            )
        );
        body.AppendChild(dayTitle);

        // Day info
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Músculos objetivo: ")),
            new Run(new Text(string.Join(", ", day.TargetMuscleGroups)))
        ));

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Duración estimada: ")),
            new Run(new Text($"{day.EstimatedDuration.TotalMinutes:F0} minutos"))
        ));

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Intensidad objetivo: ")),
            new Run(new Text(day.TargetIntensity.ToString()))
        ));

        body.AppendChild(CreateEmptyParagraph());

        // Warmup
        if (day.Warmup != null)
        {
            GenerateWarmupSection(body, day.Warmup, template);
        }

        // Exercise blocks
        foreach (var block in day.ExerciseBlocks.OrderBy(b => b.OrderInWorkout))
        {
            GenerateExerciseBlock(body, block, template);
        }

        // Cooldown
        if (day.Cooldown != null)
        {
            GenerateCooldownSection(body, day.Cooldown, template);
        }

        // Day notes
        if (!string.IsNullOrEmpty(day.DayNotes))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Notas del día: ")),
                new Run(new Text(day.DayNotes))
            ));
        }
    }

    private void GenerateWarmupSection(Body body, WarmupSection warmup, DocumentTemplate template)
    {
        // Warmup title
        var warmupTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "18" }, new Bold(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }),
                new Text($"🔥 {warmup.Title}")
            )
        );
        body.AppendChild(warmupTitle);

        // Duration
        if (warmup.Duration.TotalMinutes > 0)
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Duración: ")),
                new Run(new Text($"{warmup.Duration.TotalMinutes:F0} minutos"))
            ));
        }

        // Purpose
        if (!string.IsNullOrEmpty(warmup.Purpose))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Propósito: {warmup.Purpose}"))
            ));
        }

        // Warmup exercises
        if (warmup.Exercises.Any())
        {
            // Simple list instead of numbered list for now
            for (int i = 0; i < warmup.Exercises.Count; i++)
            {
                var exercise = warmup.Exercises[i];
                var exercisePara = new Paragraph(
                    new Run(new RunProperties(new Bold()), new Text($"{i + 1}. {exercise.Name}: ")),
                    new Run(new Text($"{exercise.Duration} - {exercise.Instructions}"))
                );
                body.AppendChild(exercisePara);
            }
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateMainWorkoutSection(Body body, List<ExerciseBlock> mainWorkout, DocumentTemplate template)
    {
        // Main workout title
        var mainTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "18" }, new Bold(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }),
                new Text("💪 ENTRENAMIENTO PRINCIPAL")
            )
        );
        body.AppendChild(mainTitle);

        foreach (var block in mainWorkout.OrderBy(b => b.OrderInWorkout))
        {
            // Block title
            if (!string.IsNullOrEmpty(block.BlockName))
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }), new Text(block.BlockName))
                ));
            }

            // Block purpose
            if (!string.IsNullOrEmpty(block.BlockPurpose))
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Italic()), new Text($"Propósito: {block.BlockPurpose}"))
                ));
            }

            // Block exercises
            foreach (var exercise in block.Exercises.OrderBy(e => e.OrderNumber))
            {
                GenerateStructuredExercise(body, exercise, template);
            }

            body.AppendChild(CreateEmptyParagraph());
        }
    }

    private void GenerateExerciseBlock(Body body, ExerciseBlock block, DocumentTemplate template)
    {
        // Block title
        if (!string.IsNullOrEmpty(block.BlockName))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }), new Text(block.BlockName))
            ));
        }

        // Block purpose
        if (!string.IsNullOrEmpty(block.BlockPurpose))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Propósito: {block.BlockPurpose}"))
            ));
        }

        // Block exercises
        foreach (var exercise in block.Exercises.OrderBy(e => e.OrderNumber))
        {
            GenerateDocumentExercise(body, exercise, template);
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateStructuredExercise(Body body, DocumentExercise exercise, DocumentTemplate template)
    {
        // Exercise name
        var exerciseTitle = new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "16" }), new Text($"{exercise.OrderNumber}. {exercise.ExerciseName}"))
        );
        body.AppendChild(exerciseTitle);

        // Parameters
        var parameters = exercise.Parameters;
        var paramText = $"Series: {parameters.Sets} | Repeticiones: {parameters.Reps} | Descanso: {parameters.RestPeriod}";

        if (!string.IsNullOrEmpty(parameters.Weight))
        {
            paramText += $" | Peso: {parameters.Weight}";
        }

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(paramText))
        ));

        // Instructions
        if (exercise.StepByStepInstructions.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Ejecución:"))
            ));

            for (int i = 0; i < exercise.StepByStepInstructions.Count; i++)
            {
                body.AppendChild(new Paragraph(
                    new Run(new Text($"{i + 1}. {exercise.StepByStepInstructions[i]}"))
                ));
            }
        }

        // Technique notes
        if (exercise.TechniqueTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Consejos técnicos: {string.Join(", ", exercise.TechniqueTips)}"))
            ));
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private async Task GenerateDocumentExerciseAsync(Body body, MainDocumentPart mainPart, DocumentExercise exercise, DocumentTemplate template, CancellationToken cancellationToken)
    {
        // Exercise name with potential image
        var exerciseTitle = new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "16" }), new Text($"{exercise.OrderNumber}. {exercise.ExerciseName}"))
        );

        // Add image placeholder indicator if image is available
        if (exercise.ImageInfo.HasImage && template.Layout.ExerciseLayout.ShowImages)
        {
            AddImageToExerciseParagraph(exerciseTitle, mainPart, exercise, template, cancellationToken);
        }

        body.AppendChild(exerciseTitle);

        // Parameters
        var parameters = exercise.Parameters;
        var paramText = $"Series: {parameters.Sets} | Repeticiones: {parameters.Reps} | Descanso: {parameters.RestPeriod}";

        if (!string.IsNullOrEmpty(parameters.Weight))
        {
            paramText += $" | Peso: {parameters.Weight}";
        }

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(paramText))
        ));

        // Target muscles
        if (exercise.TargetMuscles.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Músculos trabajados: {string.Join(", ", exercise.TargetMuscles)}"))
            ));
        }

        // Step-by-step instructions
        if (exercise.StepByStepInstructions.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Ejecución:"))
            ));

            for (int i = 0; i < exercise.StepByStepInstructions.Count; i++)
            {
                body.AppendChild(new Paragraph(
                    new Run(new Text($"{i + 1}. {exercise.StepByStepInstructions[i]}"))
                ));
            }
        }

        // Safety tips
        if (exercise.SafetyTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold(), new Color { Val = "FF0000" }), new Text("⚠️ Seguridad: ")),
                new Run(new Text(string.Join(". ", exercise.SafetyTips)))
            ));
        }

        // Technique tips
        if (exercise.TechniqueTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }), new Text("💡 Consejos: ")),
                new Run(new Text(string.Join(". ", exercise.TechniqueTips)))
            ));
        }

        // Variations
        if (exercise.Variations.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Variaciones:"))
            ));

            foreach (var variation in exercise.Variations)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Italic()), new Text($"• {variation.VariationName}: {variation.Description}"))
                ));
            }
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateDocumentExercise(Body body, DocumentExercise exercise, DocumentTemplate template)
    {
        // Exercise name with image placeholder
        var exerciseTitle = new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "16" }), new Text($"{exercise.OrderNumber}. {exercise.ExerciseName}"))
        );

        // Add image placeholder indicator if image is available
        if (exercise.ImageInfo.HasImage && template.Layout.ExerciseLayout.ShowImages)
        {
            var imageRun = new Run(
                new Text(" [Imagen disponible]") { Space = SpaceProcessingModeValues.Preserve }
            );
            imageRun.RunProperties = new RunProperties(
                new Italic(),
                new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }
            );
            exerciseTitle.AppendChild(imageRun);
        }

        body.AppendChild(exerciseTitle);

        // Parameters
        var parameters = exercise.Parameters;
        var paramText = $"Series: {parameters.Sets} | Repeticiones: {parameters.Reps} | Descanso: {parameters.RestPeriod}";

        if (!string.IsNullOrEmpty(parameters.Weight))
        {
            paramText += $" | Peso: {parameters.Weight}";
        }

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text(paramText))
        ));

        // Target muscles
        if (exercise.TargetMuscles.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Músculos trabajados: {string.Join(", ", exercise.TargetMuscles)}"))
            ));
        }

        // Step-by-step instructions
        if (exercise.StepByStepInstructions.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Ejecución:"))
            ));

            for (int i = 0; i < exercise.StepByStepInstructions.Count; i++)
            {
                body.AppendChild(new Paragraph(
                    new Run(new Text($"{i + 1}. {exercise.StepByStepInstructions[i]}"))
                ));
            }
        }

        // Safety tips
        if (exercise.SafetyTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold(), new Color { Val = "FF0000" }), new Text("⚠️ Seguridad: ")),
                new Run(new Text(string.Join(". ", exercise.SafetyTips)))
            ));
        }

        // Technique tips
        if (exercise.TechniqueTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }), new Text("💡 Consejos: ")),
                new Run(new Text(string.Join(". ", exercise.TechniqueTips)))
            ));
        }

        // Variations
        if (exercise.Variations.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Variaciones:"))
            ));

            foreach (var variation in exercise.Variations)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Italic()), new Text($"• {variation.VariationName}: {variation.Description}"))
                ));
            }
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateCooldownSection(Body body, CooldownSection cooldown, DocumentTemplate template)
    {
        // Cooldown title
        var cooldownTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "18" }, new Bold(), new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }),
                new Text($"🧘‍♀️ {cooldown.Title}")
            )
        );
        body.AppendChild(cooldownTitle);

        // Duration
        if (cooldown.Duration.TotalMinutes > 0)
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Duración: ")),
                new Run(new Text($"{cooldown.Duration.TotalMinutes:F0} minutos"))
            ));
        }

        // Purpose
        if (!string.IsNullOrEmpty(cooldown.Purpose))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Propósito: {cooldown.Purpose}"))
            ));
        }

        // Cooldown exercises
        if (cooldown.Exercises.Any())
        {
            for (int i = 0; i < cooldown.Exercises.Count; i++)
            {
                var exercise = cooldown.Exercises[i];
                var exercisePara = new Paragraph(
                    new Run(new RunProperties(new Bold()), new Text($"{i + 1}. {exercise.Name}: ")),
                    new Run(new Text($"{exercise.Duration} - {exercise.Instructions}"))
                );
                body.AppendChild(exercisePara);
            }
        }

        // Recovery tips
        if (cooldown.RecoveryTips.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Consejos de recuperación:"))
            ));

            foreach (var tip in cooldown.RecoveryTips)
            {
                body.AppendChild(new Paragraph(
                    new Run(new Text($"• {tip}"))
                ));
            }
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private void GenerateProgressSection(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        // Progress title
        var progressTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                new Text("📈 SEGUIMIENTO DE PROGRESO")
            )
        );
        body.AppendChild(progressTitle);

        // Progress tracking table
        var table = new Table();

        // Table properties
        var tableProps = new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }, // 100% width
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
            )
        );
        table.AppendChild(tableProps);

        // Header row
        var headerRow = new TableRow();
        headerRow.AppendChild(CreateTableCell("Ejercicio", true));
        headerRow.AppendChild(CreateTableCell("Semana 1", true));
        headerRow.AppendChild(CreateTableCell("Semana 2", true));
        headerRow.AppendChild(CreateTableCell("Semana 3", true));
        headerRow.AppendChild(CreateTableCell("Semana 4", true));
        headerRow.AppendChild(CreateTableCell("Notas", true));
        table.AppendChild(headerRow);

        // Add sample rows for main exercises
        var mainExercises = GetMainExercisesFromRequest(request);
        foreach (var exerciseName in mainExercises.Take(8)) // Limit to 8 exercises
        {
            var row = new TableRow();
            row.AppendChild(CreateTableCell(exerciseName));
            row.AppendChild(CreateTableCell("")); // Empty cells for user to fill
            row.AppendChild(CreateTableCell(""));
            row.AppendChild(CreateTableCell(""));
            row.AppendChild(CreateTableCell(""));
            row.AppendChild(CreateTableCell(""));
            table.AppendChild(row);
        }

        body.AppendChild(table);
        body.AppendChild(CreateEmptyParagraph());

        // Progress notes section
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Notas de progreso:"))
        ));

        for (int i = 1; i <= 6; i++)
        {
            body.AppendChild(new Paragraph(
                new Run(new Text($"Semana {i}: _______________________________________________"))
            ));
            body.AppendChild(CreateEmptyParagraph());
        }
    }

    private void GenerateNotesSection(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        // Notes title
        var notesTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold(), new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                new Text("📝 NOTAS ADICIONALES")
            )
        );
        body.AppendChild(notesTitle);

        // General notes
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold()), new Text("Observaciones generales:"))
        ));

        for (int i = 1; i <= 8; i++)
        {
            body.AppendChild(new Paragraph(
                new Run(new Text($"{i}. _______________________________________________"))
            ));
        }

        body.AppendChild(CreateEmptyParagraph());

        // Contact info
        if (!string.IsNullOrEmpty(request.TrainerName))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Contacto del entrenador:"))
            ));
            body.AppendChild(new Paragraph(
                new Run(new Text($"Entrenador: {request.TrainerName}"))
            ));
        }

        if (!string.IsNullOrEmpty(request.GymName))
        {
            body.AppendChild(new Paragraph(
                new Run(new Text($"Gimnasio: {request.GymName}"))
            ));
        }
    }

    // Helper methods
    private string GenerateFileName(RoutineDocumentRequest request)
    {
        var date = request.CreationDate.ToString("yyyy-MM-dd");
        var clientName = SanitizeFileName(request.ClientName);
        var fileName = !string.IsNullOrEmpty(request.Settings.OutputFileName)
            ? request.Settings.OutputFileName
            : $"Rutina_{clientName}_{date}";

        return fileName.EndsWith(".docx") ? fileName : $"{fileName}.docx";
    }

    private string DetermineFilePath(RoutineDocumentRequest request, string fileName)
    {
        var directory = !string.IsNullOrEmpty(request.Settings.OutputDirectory)
            ? request.Settings.OutputDirectory
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rutinas");

        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Rutina" : sanitized;
    }

    private Styles CreateStyles(TemplateStyle templateStyle)
    {
        var styles = new Styles();

        // Default paragraph style
        var defaultStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };

        defaultStyle.AppendChild(new StyleName { Val = "Normal" });
        defaultStyle.AppendChild(new StyleRunProperties(
            new RunFonts { Ascii = templateStyle.FontScheme.BodyFont, HighAnsi = templateStyle.FontScheme.BodyFont },
            new FontSize { Val = templateStyle.FontScheme.FontSizes["Body"].ToString() },
            new Color { Val = templateStyle.ColorScheme.TextColor.Replace("#", "") }
        ));

        styles.AppendChild(defaultStyle);

        return styles;
    }

    private Header CreateHeader(RoutineDocumentRequest request, DocumentTemplate template)
    {
        var header = new Header();

        var paragraph = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(new FontSize { Val = "20" }),
                new Text(template.Style.HeaderFooter.HeaderText ?? $"{request.GymName} - Rutina Personalizada")
            )
        );

        header.AppendChild(paragraph);
        return header;
    }

    private Footer CreateFooter(RoutineDocumentRequest request, DocumentTemplate template)
    {
        var footer = new Footer();

        var paragraph = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(new FontSize { Val = "16" }),
                new Text(template.Style.HeaderFooter.FooterText ?? "Generado por GymRoutine Generator")
            )
        );

        footer.AppendChild(paragraph);
        return footer;
    }

    private TableRow CreateInfoRow(string label, string value)
    {
        var row = new TableRow();
        row.AppendChild(CreateTableCell(label, true));
        row.AppendChild(CreateTableCell(value));
        return row;
    }

    private TableCell CreateTableCell(string text, bool isBold = false)
    {
        var cell = new TableCell();

        var runProperties = new RunProperties();
        if (isBold)
        {
            runProperties.AppendChild(new Bold());
        }

        var paragraph = new Paragraph(new Run(runProperties, new Text(text)));
        cell.AppendChild(paragraph);

        return cell;
    }

    private Paragraph CreateEmptyParagraph()
    {
        return new Paragraph(new Run(new Text("")));
    }

    private void AddPageBreak(Body body)
    {
        var pageBreak = new Paragraph(
            new Run(
                new Break { Type = BreakValues.Page }
            )
        );
        body.AppendChild(pageBreak);
    }

    private List<string> GetMainExercisesFromRequest(RoutineDocumentRequest request)
    {
        var exercises = new List<string>();

        if (request.Routine?.MainWorkout != null)
        {
            exercises.AddRange(request.Routine.MainWorkout
                .SelectMany(block => block.Exercises)
                .Select(ex => ex.BaseExercise.Name));
        }

        if (request.WeeklyPrograms.Any())
        {
            exercises.AddRange(request.WeeklyPrograms
                .SelectMany(week => week.DailyWorkouts)
                .SelectMany(day => day.ExerciseBlocks)
                .SelectMany(block => block.Exercises)
                .Select(ex => ex.ExerciseName));
        }

        return exercises.Distinct().ToList();
    }

    private async Task OpenDocumentAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(100, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, $"Could not auto-open document: {filePath}");
        }
    }

    // Template creation methods
    private DocumentTemplate CreateBasicTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "basic",
            Name = "Plantilla Básica",
            Description = "Plantilla simple y limpia para rutinas básicas",
            Type = TemplateType.Basic,
            IsDefault = false,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#333333",
                    SecondaryColor = "#666666",
                    AccentColor = "#999999",
                    TextColor = "#000000",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Arial",
                    BodyFont = "Arial",
                    AccentFont = "Arial"
                },
                HeaderFooter = new HeaderFooterStyle
                {
                    FooterText = "Rutina básica generada por GymRoutine Generator"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = false,
                IncludeTableOfContents = false,
                IncludeProgressSection = true,
                IncludeSafetySection = true
            }
        };
    }

    private DocumentTemplate CreateStandardTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "standard",
            Name = "Plantilla Estándar",
            Description = "Plantilla equilibrada para uso general",
            Type = TemplateType.Standard,
            IsDefault = true,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#2E86AB",
                    SecondaryColor = "#A23B72",
                    AccentColor = "#F18F01",
                    TextColor = "#333333",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Calibri",
                    BodyFont = "Calibri",
                    AccentFont = "Calibri Light"
                },
                HeaderFooter = new HeaderFooterStyle
                {
                    FooterText = "Rutina generada por GymRoutine Generator"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                IncludeProgressSection = true,
                IncludeSafetySection = true
            }
        };
    }

    private DocumentTemplate CreateProfessionalTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "professional",
            Name = "Plantilla Profesional",
            Description = "Plantilla elegante para entrenadores profesionales",
            Type = TemplateType.Professional,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#1B4332",
                    SecondaryColor = "#2D6A4F",
                    AccentColor = "#40916C",
                    TextColor = "#1B4332",
                    BackgroundColor = "#FFFFFF",
                    HeaderColor = "#F8F9FA"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Times New Roman",
                    BodyFont = "Times New Roman",
                    AccentFont = "Times New Roman"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = true,
                IncludeProgressSection = true,
                IncludeSafetySection = true
            }
        };
    }

    private DocumentTemplate CreateGymTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "gym",
            Name = "Plantilla Gimnasio",
            Description = "Plantilla diseñada para gimnasios comerciales",
            Type = TemplateType.Gym,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#E63946",
                    SecondaryColor = "#F77F00",
                    AccentColor = "#FCBF49",
                    TextColor = "#2A2A2A",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Impact",
                    BodyFont = "Arial",
                    AccentFont = "Arial Bold"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                IncludeProgressSection = true,
                IncludeSafetySection = true
            }
        };
    }

    private DocumentTemplate CreateRehabilitationTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "rehabilitation",
            Name = "Plantilla Rehabilitación",
            Description = "Plantilla especializada para programas de rehabilitación",
            Type = TemplateType.Rehabilitation,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#006A6B",
                    SecondaryColor = "#0A9396",
                    AccentColor = "#94D2BD",
                    TextColor = "#003D3E",
                    BackgroundColor = "#FFFFFF"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                IncludeProgressSection = true,
                IncludeSafetySection = true,
                WarmupLayout = new SectionLayout { ShowSafetyTips = true },
                ExerciseLayout = new SectionLayout { ShowSafetyTips = true },
                CooldownLayout = new SectionLayout { ShowSafetyTips = true }
            }
        };
    }

    private async Task<DocumentTemplate> GetDefaultTemplateAsync(CancellationToken cancellationToken)
    {
        var templates = await GetAvailableTemplatesAsync(cancellationToken);
        return templates.FirstOrDefault(t => t.IsDefault) ?? CreateStandardTemplate();
    }

    private DocumentMetadata CreateDocumentMetadata(RoutineDocumentRequest request, DocumentTemplate template)
    {
        return new DocumentMetadata
        {
            Title = $"Rutina de Entrenamiento - {request.ClientName}",
            Subject = "Rutina personalizada de ejercicios",
            Author = request.TrainerName ?? "GymRoutine Generator",
            Company = request.GymName ?? "",
            Keywords = new List<string> { "rutina", "ejercicios", "entrenamiento", "fitness" },
            Comments = $"Generada usando plantilla {template.Name}",
            CreationDate = request.CreationDate
        };
    }

    private async Task<GenerationStatistics> CalculateStatisticsAsync(RoutineDocumentRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(5, cancellationToken);

        var stats = new GenerationStatistics
        {
            TotalPages = EstimatePageCount(request),
            TotalExercises = CountTotalExercises(request),
            TotalImages = CountTotalImages(request),
            TotalSections = CountTotalSections(request)
        };

        stats.SectionCounts = new Dictionary<string, int>
        {
            ["Warmup"] = CountWarmupSections(request),
            ["MainWorkout"] = CountMainWorkoutSections(request),
            ["Cooldown"] = CountCooldownSections(request),
            ["Safety"] = request.SafetyNotes.Count,
            ["Progress"] = 1
        };

        return stats;
    }

    private int EstimatePageCount(RoutineDocumentRequest request)
    {
        var pageCount = 1; // Cover page

        if (request.WeeklyPrograms.Any())
        {
            pageCount += request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Count * 2); // Estimate 2 pages per day
        }
        else if (request.Routine != null)
        {
            pageCount += 3; // Basic routine
        }

        pageCount += 2; // Progress and notes pages

        return pageCount;
    }

    private int CountTotalExercises(RoutineDocumentRequest request)
    {
        var count = 0;

        if (request.Routine?.MainWorkout != null)
        {
            count += request.Routine.MainWorkout.Sum(block => block.Exercises.Count);
        }

        if (request.WeeklyPrograms.Any())
        {
            count += request.WeeklyPrograms
                .Sum(week => week.DailyWorkouts
                    .Sum(day => day.ExerciseBlocks
                        .Sum(block => block.Exercises.Count)));
        }

        return count;
    }

    private int CountTotalImages(RoutineDocumentRequest request)
    {
        // For now, assume some exercises have images
        return (int)(CountTotalExercises(request) * 0.7); // 70% have images
    }

    private int CountTotalSections(RoutineDocumentRequest request)
    {
        var sections = 3; // Client info, goals, safety

        if (request.WeeklyPrograms.Any())
        {
            sections += request.WeeklyPrograms.Count; // One section per week
        }
        else
        {
            sections += 1; // Single routine section
        }

        sections += 2; // Progress and notes

        return sections;
    }

    private int CountWarmupSections(RoutineDocumentRequest request)
    {
        if (request.WeeklyPrograms.Any())
        {
            return request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Count);
        }
        return request.Routine?.Warmup != null ? 1 : 0;
    }

    private int CountMainWorkoutSections(RoutineDocumentRequest request)
    {
        if (request.WeeklyPrograms.Any())
        {
            return request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Sum(d => d.ExerciseBlocks.Count));
        }
        return request.Routine?.MainWorkout?.Count ?? 0;
    }

    private int CountCooldownSections(RoutineDocumentRequest request)
    {
        if (request.WeeklyPrograms.Any())
        {
            return request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Count);
        }
        return request.Routine?.Cooldown != null ? 1 : 0;
    }

    private void GenerateTableOfContents(Body body, RoutineDocumentRequest request, DocumentTemplate template)
    {
        var tocTitle = new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "28" }, new Bold()),
                new Text("ÍNDICE")
            )
        );
        body.AppendChild(tocTitle);
        body.AppendChild(CreateEmptyParagraph());

        var tocItems = new List<(string title, int pageNumber)>
        {
            ("Información del Cliente", 3),
            ("Objetivos", 3),
            ("Consideraciones de Seguridad", 4)
        };

        if (request.WeeklyPrograms.Any())
        {
            var pageNumber = 5;
            foreach (var week in request.WeeklyPrograms)
            {
                tocItems.Add(($"Semana {week.WeekNumber}: {week.WeekFocus}", pageNumber));
                pageNumber += week.DailyWorkouts.Count * 2;
            }
        }
        else if (request.Routine != null)
        {
            tocItems.Add(($"Rutina: {request.Routine.RoutineName}", 5));
        }

        tocItems.Add(("Seguimiento de Progreso", tocItems.Last().pageNumber + 2));
        tocItems.Add(("Notas Adicionales", tocItems.Last().pageNumber + 1));

        foreach (var item in tocItems)
        {
            var tocParagraph = new Paragraph(
                new Run(new Text(item.title)),
                new Run(new Text($" .......................... {item.pageNumber}"))
            );
            body.AppendChild(tocParagraph);
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    private List<PagePreview> GeneratePagePreviews(RoutineDocumentRequest request, DocumentTemplate template)
    {
        var previews = new List<PagePreview>();
        var pageNumber = 1;

        // Cover page
        if (template.Layout.IncludeCoverPage)
        {
            previews.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = "Portada",
                ContentSummary = new List<string> { "Título", "Información del cliente", "Fecha" }
            });
        }

        // Table of contents
        if (template.Layout.IncludeTableOfContents)
        {
            previews.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = "Índice",
                ContentSummary = new List<string> { "Estructura del documento", "Números de página" }
            });
        }

        // Client info and goals
        previews.Add(new PagePreview
        {
            PageNumber = pageNumber++,
            PageTitle = "Información y Objetivos",
            ContentSummary = new List<string> { "Datos del cliente", "Objetivos principales", "Consideraciones de seguridad" }
        });

        // Routine content
        if (request.WeeklyPrograms.Any())
        {
            foreach (var week in request.WeeklyPrograms)
            {
                foreach (var day in week.DailyWorkouts)
                {
                    previews.Add(new PagePreview
                    {
                        PageNumber = pageNumber++,
                        PageTitle = $"Semana {week.WeekNumber}, Día {day.DayNumber}",
                        ContentSummary = new List<string> { day.WorkoutName, $"{day.ExerciseBlocks.Sum(b => b.Exercises.Count)} ejercicios", $"{day.EstimatedDuration.TotalMinutes:F0} min" }
                    });
                }
            }
        }
        else if (request.Routine != null)
        {
            previews.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = request.Routine.RoutineName,
                ContentSummary = new List<string> { $"{request.Routine.MainWorkout.Sum(b => b.Exercises.Count)} ejercicios", $"{request.Routine.EstimatedDuration.TotalMinutes:F0} min" }
            });
        }

        // Progress and notes
        if (template.Layout.IncludeProgressSection)
        {
            previews.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = "Seguimiento de Progreso",
                ContentSummary = new List<string> { "Tabla de progreso", "Notas semanales" }
            });
        }

        previews.Add(new PagePreview
        {
            PageNumber = pageNumber,
            PageTitle = "Notas Adicionales",
            ContentSummary = new List<string> { "Observaciones", "Información de contacto" }
        });

        return previews;
    }

    private DocumentStructure GenerateDocumentStructure(RoutineDocumentRequest request, DocumentTemplate template)
    {
        var structure = new DocumentStructure();
        var sections = new List<DocumentSection>();

        if (template.Layout.IncludeCoverPage)
        {
            sections.Add(new DocumentSection { SectionName = "Portada", StartPage = 1, PageCount = 1 });
        }

        if (template.Layout.IncludeTableOfContents)
        {
            sections.Add(new DocumentSection { SectionName = "Índice", StartPage = 2, PageCount = 1 });
        }

        sections.Add(new DocumentSection { SectionName = "Información del Cliente", StartPage = 3, PageCount = 1 });

        if (request.WeeklyPrograms.Any())
        {
            var startPage = sections.Last().StartPage + sections.Last().PageCount;
            foreach (var week in request.WeeklyPrograms)
            {
                var weekSection = new DocumentSection
                {
                    SectionName = $"Semana {week.WeekNumber}",
                    StartPage = startPage,
                    PageCount = week.DailyWorkouts.Count * 2,
                    SubSections = week.DailyWorkouts.Select(d => $"Día {d.DayNumber}: {d.WorkoutName}").ToList()
                };
                sections.Add(weekSection);
                startPage += weekSection.PageCount;
            }
        }
        else if (request.Routine != null)
        {
            sections.Add(new DocumentSection
            {
                SectionName = "Rutina Principal",
                StartPage = sections.Last().StartPage + sections.Last().PageCount,
                PageCount = 2
            });
        }

        if (template.Layout.IncludeProgressSection)
        {
            sections.Add(new DocumentSection
            {
                SectionName = "Seguimiento de Progreso",
                StartPage = sections.Last().StartPage + sections.Last().PageCount,
                PageCount = 1
            });
        }

        sections.Add(new DocumentSection
        {
            SectionName = "Notas Adicionales",
            StartPage = sections.Last().StartPage + sections.Last().PageCount,
            PageCount = 1
        });

        structure.Sections = sections;
        structure.EstimatedPageCount = sections.Sum(s => s.PageCount);
        structure.EstimatedReadingTime = TimeSpan.FromMinutes(structure.EstimatedPageCount * 3); // 3 minutes per page

        return structure;
    }

    private PreviewStatistics CalculatePreviewStatistics(RoutineDocumentRequest request)
    {
        var totalExercises = CountTotalExercises(request);

        return new PreviewStatistics
        {
            TotalExercises = totalExercises,
            ExercisesWithImages = (int)(totalExercises * 0.7), // Assume 70% have images
            TotalInstructions = totalExercises * 3, // Average 3 instructions per exercise
            SafetyNotesCount = request.SafetyNotes.Count,
            ContentBreakdown = new Dictionary<string, int>
            {
                ["Ejercicios"] = totalExercises,
                ["Bloques de entrenamiento"] = CountMainWorkoutSections(request),
                ["Días de entrenamiento"] = request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Count),
                ["Semanas programadas"] = request.WeeklyPrograms.Count
            }
        };
    }

    // Image handling methods for Story 5.2: Exercise Image Integration
    private async Task<ImagePart?> AddImageToDocument(MainDocumentPart mainPart, ExerciseImageInfo imageInfo, CancellationToken cancellationToken)
    {
        try
        {
            if (!imageInfo.HasImage)
                return null;

            byte[] imageBytes;

            // Get image data from different sources
            if (imageInfo.ImageData.Length > 0)
            {
                imageBytes = imageInfo.ImageData;
            }
            else if (!string.IsNullOrEmpty(imageInfo.ImagePath) && File.Exists(imageInfo.ImagePath))
            {
                imageBytes = await File.ReadAllBytesAsync(imageInfo.ImagePath, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(imageInfo.ImageUrl))
            {
                // Download image from URL
                using var httpClient = new HttpClient();
                imageBytes = await httpClient.GetByteArrayAsync(imageInfo.ImageUrl, cancellationToken);
            }
            else
            {
                // Use placeholder image
                imageBytes = CreatePlaceholderImageBytes();
            }

            // Optimize image for document
            var optimizedBytes = OptimizeImageForDocument(imageBytes, imageInfo.DisplaySettings);

            // Determine image type
            var partType = GetImagePartType(imageInfo.ImageFormat);

            // Add image part to document
            var imagePart = mainPart.AddImagePart(ImagePartType.Png); // Simplified for now
            using var stream = new MemoryStream(optimizedBytes);
            imagePart.FeedData(stream);

            return imagePart;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, $"Failed to add image to document");
            return null;
        }
    }

    private byte[] OptimizeImageForDocument(byte[] originalBytes, ImageDisplaySettings settings)
    {
        try
        {
            using var originalStream = new MemoryStream(originalBytes);
            using var originalImage = SystemDrawing.Image.FromStream(originalStream);

            // Calculate optimal dimensions
            var (newWidth, newHeight) = CalculateOptimalImageSize(
                originalImage.Width, originalImage.Height,
                settings.Width, settings.Height,
                settings.MaintainAspectRatio);

            // Create optimized image
            using var optimizedImage = new SystemDrawing.Bitmap(newWidth, newHeight);
            using var graphics = SystemDrawing.Graphics.FromImage(optimizedImage);

            // High quality scaling
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);

            // Save with quality settings
            using var optimizedStream = new MemoryStream();
            var encoder = GetImageEncoder(SystemDrawingImaging.ImageFormat.Jpeg);
            var encoderParams = new SystemDrawingImaging.EncoderParameters(1);
            encoderParams.Param[0] = new SystemDrawingImaging.EncoderParameter(SystemDrawingImaging.Encoder.Quality, (long)settings.Quality);

            optimizedImage.Save(optimizedStream, encoder, encoderParams);
            return optimizedStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to optimize image, using original");
            return originalBytes;
        }
    }

    private (int width, int height) CalculateOptimalImageSize(int originalWidth, int originalHeight, int targetWidth, int targetHeight, bool maintainAspectRatio)
    {
        if (!maintainAspectRatio)
            return (targetWidth, targetHeight);

        // Calculate aspect ratio scaling
        var scaleX = (double)targetWidth / originalWidth;
        var scaleY = (double)targetHeight / originalHeight;
        var scale = Math.Min(scaleX, scaleY);

        return ((int)(originalWidth * scale), (int)(originalHeight * scale));
    }

    private byte[] CreatePlaceholderImageBytes()
    {
        // Create a simple placeholder image
        using var placeholder = new SystemDrawing.Bitmap(200, 150);
        using var graphics = SystemDrawing.Graphics.FromImage(placeholder);

        // Fill with light gray background
        graphics.FillRectangle(SystemDrawing.Brushes.LightGray, 0, 0, 200, 150);

        // Add border
        graphics.DrawRectangle(SystemDrawing.Pens.DarkGray, 0, 0, 199, 149);

        // Add text
        using var font = new SystemDrawing.Font("Arial", 12, SystemDrawing.FontStyle.Bold);
        var text = "Imagen no disponible";
        var textSize = graphics.MeasureString(text, font);
        var x = (200 - textSize.Width) / 2;
        var y = (150 - textSize.Height) / 2;
        graphics.DrawString(text, font, SystemDrawing.Brushes.DarkGray, x, y);

        // Convert to bytes
        using var stream = new MemoryStream();
        placeholder.Save(stream, SystemDrawingImaging.ImageFormat.Png);
        return stream.ToArray();
    }

    private string GetImagePartType(string format)
    {
        return format.ToLower() switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "tiff" or "tif" => "image/tiff",
            _ => "image/png"
        };
    }

    private SystemDrawingImaging.ImageCodecInfo GetImageEncoder(SystemDrawingImaging.ImageFormat format)
    {
        var codecs = SystemDrawingImaging.ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid) ??
               codecs.First(codec => codec.FormatID == SystemDrawingImaging.ImageFormat.Png.Guid);
    }

    // Simplified image integration for Story 5.2 - placeholder implementation
    // Full OpenXML Drawing integration would be implemented here in future iterations

    private void AddImageToExerciseParagraph(Paragraph paragraph, MainDocumentPart mainPart, DocumentExercise exercise, DocumentTemplate template, CancellationToken cancellationToken)
    {
        if (!exercise.ImageInfo.HasImage || !template.Layout.ExerciseLayout.ShowImages)
            return;

        try
        {
            // For now, add a placeholder for image integration
            // This will be expanded when we implement full image embedding
            var imageRun = new Run(
                new Text(" [Imagen del ejercicio]") { Space = SpaceProcessingModeValues.Preserve }
            );

            imageRun.RunProperties = new RunProperties(
                new Italic(),
                new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }
            );

            paragraph.AppendChild(imageRun);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, $"Failed to add image to exercise paragraph for {exercise.ExerciseName}");
        }
    }
}