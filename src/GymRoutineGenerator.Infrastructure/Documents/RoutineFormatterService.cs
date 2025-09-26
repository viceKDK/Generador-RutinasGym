using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Core.Models;
using Microsoft.Extensions.Logging;

namespace GymRoutineGenerator.Infrastructure.Documents;

public class RoutineFormatterService
{
    private readonly ILogger<RoutineFormatterService>? _logger;

    public RoutineFormatterService(ILogger<RoutineFormatterService>? logger = null)
    {
        _logger = logger;
    }

    public void FormatDayByDayBreakdown(Body body, List<WeeklyProgram> weeklyPrograms, DocumentTemplate template)
    {
        if (!weeklyPrograms.Any()) return;

        // Title for weekly programs
        var weeklyTitle = new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(new RunProperties(new Bold(), new FontSize { Val = "20" }),
                    new Text("PROGRAMA SEMANAL DE ENTRENAMIENTO"))
        );
        body.AppendChild(weeklyTitle);
        body.AppendChild(CreateEmptyParagraph());

        foreach (var week in weeklyPrograms.OrderBy(w => w.WeekNumber))
        {
            FormatWeeklyProgram(body, week, template);
        }
    }

    public void FormatWeeklyProgram(Body body, WeeklyProgram week, DocumentTemplate template)
    {
        // Week header
        var weekHeader = new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "16" },
                    new Color { Val = template.Style.ColorScheme.PrimaryColor.Replace("#", "") }),
                    new Text($"SEMANA {week.WeekNumber}"))
        );

        if (!string.IsNullOrEmpty(week.WeekFocus))
        {
            weekHeader.AppendChild(new Run(new Text($" - {week.WeekFocus}") { Space = SpaceProcessingModeValues.Preserve }));
        }

        body.AppendChild(weekHeader);

        // Week notes
        if (!string.IsNullOrEmpty(week.WeekNotes))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Objetivos: {week.WeekNotes}"))
            ));
        }

        body.AppendChild(CreateEmptyParagraph());

        // Daily workouts
        foreach (var day in week.DailyWorkouts.OrderBy(d => d.DayNumber))
        {
            FormatDailyWorkout(body, day, template);
        }

        // Week progression goals
        if (week.ProgressionGoals != null && week.ProgressionGoals.WeeklyTargets.Any())
        {
            FormatProgressionGoals(body, week.ProgressionGoals, template);
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    public void FormatDailyWorkout(Body body, DailyWorkout day, DocumentTemplate template)
    {
        // Day header with styling
        var dayHeader = CreateStyledHeader($"DÍA {day.DayNumber}: {day.WorkoutName}", template, 2);
        body.AppendChild(dayHeader);

        // Target muscle groups
        if (day.TargetMuscleGroups.Any())
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text("Músculos objetivo: ")),
                new Run(new Text(string.Join(", ", day.TargetMuscleGroups)))
            ));
        }

        // Workout info box
        CreateWorkoutInfoBox(body, day, template);

        // Warmup section
        if (day.Warmup != null && day.Warmup.Exercises.Any())
        {
            FormatWarmupSection(body, day.Warmup, template);
        }

        // Exercise blocks
        foreach (var block in day.ExerciseBlocks.OrderBy(b => b.OrderInWorkout))
        {
            FormatExerciseBlock(body, block, template);
        }

        // Cooldown section
        if (day.Cooldown != null && day.Cooldown.Exercises.Any())
        {
            FormatCooldownSection(body, day.Cooldown, template);
        }

        // Day notes
        if (!string.IsNullOrEmpty(day.DayNotes))
        {
            FormatDayNotes(body, day.DayNotes, template);
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    public void FormatExerciseBlock(Body body, ExerciseBlock block, DocumentTemplate template)
    {
        // Block header
        if (!string.IsNullOrEmpty(block.BlockName))
        {
            var blockHeader = new Paragraph(
                new Run(new RunProperties(new Bold(), new FontSize { Val = "14" },
                        new Color { Val = template.Style.ColorScheme.SecondaryColor.Replace("#", "") }),
                        new Text(block.BlockName.ToUpper()))
            );
            body.AppendChild(blockHeader);
        }

        // Block purpose
        if (!string.IsNullOrEmpty(block.BlockPurpose))
        {
            body.AppendChild(new Paragraph(
                new Run(new RunProperties(new Italic()), new Text($"Propósito: {block.BlockPurpose}"))
            ));
        }

        // Format exercises based on template display format
        switch (template.Layout.ExerciseLayout.DisplayFormat)
        {
            case ExerciseDisplayFormat.TableFormat:
                FormatExercisesAsTable(body, block.Exercises, template);
                break;
            case ExerciseDisplayFormat.CardFormat:
                FormatExercisesAsCards(body, block.Exercises, template);
                break;
            case ExerciseDisplayFormat.DetailedFormat:
                FormatExercisesDetailed(body, block.Exercises, template);
                break;
            case ExerciseDisplayFormat.ListFormat:
            default:
                FormatExercisesAsList(body, block.Exercises, template);
                break;
        }

        body.AppendChild(CreateEmptyParagraph());
    }

    public void FormatExercisesAsTable(Body body, List<DocumentExercise> exercises, DocumentTemplate template)
    {
        if (!exercises.Any()) return;

        // Create table
        var table = new Table();

        // Table properties
        var tableProperties = new TableProperties(
            new TableBorders(
                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
            ),
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "100%" }
        );
        table.AppendChild(tableProperties);

        // Header row
        var headerRow = new TableRow();
        var headers = new[] { "Ejercicio", "Series", "Repeticiones", "Descanso", "Notas" };

        foreach (var header in headers)
        {
            var cell = new TableCell();
            cell.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text(header))
            ));
            cell.AppendChild(new TableCellProperties(
                new Shading { Val = ShadingPatternValues.Clear,
                             Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F0F0F0" }
            ));
            headerRow.AppendChild(cell);
        }
        table.AppendChild(headerRow);

        // Exercise rows
        foreach (var exercise in exercises.OrderBy(e => e.OrderNumber))
        {
            var row = new TableRow();

            // Exercise name cell
            var nameCell = new TableCell();
            nameCell.AppendChild(new Paragraph(
                new Run(new RunProperties(new Bold()), new Text(exercise.ExerciseName))
            ));
            if (exercise.TargetMuscles.Any())
            {
                nameCell.AppendChild(new Paragraph(
                    new Run(new RunProperties(new FontSize { Val = "18" }, new Italic()),
                            new Text($"({string.Join(", ", exercise.TargetMuscles)})"))
                ));
            }
            row.AppendChild(nameCell);

            // Parameters cells
            row.AppendChild(new TableCell(new Paragraph(new Run(new Text(exercise.Parameters.Sets.ToString())))));
            row.AppendChild(new TableCell(new Paragraph(new Run(new Text(exercise.Parameters.Reps)))));
            row.AppendChild(new TableCell(new Paragraph(new Run(new Text(exercise.Parameters.RestPeriod)))));

            // Notes cell
            var notesCell = new TableCell();
            var notesParagraph = new Paragraph();

            if (exercise.TechniqueTips.Any() && template.Layout.ExerciseLayout.ShowTechniqueTips)
            {
                notesParagraph.AppendChild(new Run(new Text(string.Join(". ", exercise.TechniqueTips.Take(2)))));
            }
            else if (exercise.SafetyTips.Any() && template.Layout.ExerciseLayout.ShowSafetyTips)
            {
                notesParagraph.AppendChild(new Run(new Text(string.Join(". ", exercise.SafetyTips.Take(1)))));
            }

            notesCell.AppendChild(notesParagraph);
            row.AppendChild(notesCell);

            table.AppendChild(row);
        }

        body.AppendChild(table);
    }

    public void FormatExercisesAsCards(Body body, List<DocumentExercise> exercises, DocumentTemplate template)
    {
        foreach (var exercise in exercises.OrderBy(e => e.OrderNumber))
        {
            // Card container (using a bordered paragraph)
            var cardBorder = new ParagraphBorders(
                new TopBorder { Val = BorderValues.Single, Size = 6, Color = template.Style.ColorScheme.AccentColor.Replace("#", "") },
                new BottomBorder { Val = BorderValues.Single, Size = 6, Color = template.Style.ColorScheme.AccentColor.Replace("#", "") },
                new LeftBorder { Val = BorderValues.Single, Size = 6, Color = template.Style.ColorScheme.AccentColor.Replace("#", "") },
                new RightBorder { Val = BorderValues.Single, Size = 6, Color = template.Style.ColorScheme.AccentColor.Replace("#", "") }
            );

            // Card header
            var cardHeader = new Paragraph(
                new ParagraphProperties(
                    cardBorder,
                    new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F8F9FA" }
                ),
                new Run(new RunProperties(new Bold(), new FontSize { Val = "14" }),
                        new Text($"{exercise.OrderNumber}. {exercise.ExerciseName}"))
            );
            body.AppendChild(cardHeader);

            // Parameters line
            var paramText = $"🏋️ {exercise.Parameters.Sets} series × {exercise.Parameters.Reps} reps • ⏱️ {exercise.Parameters.RestPeriod}";
            if (!string.IsNullOrEmpty(exercise.Parameters.Weight))
            {
                paramText += $" • ⚖️ {exercise.Parameters.Weight}";
            }

            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Indentation { Left = "200" }),
                new Run(new RunProperties(new Bold()), new Text(paramText))
            ));

            // Target muscles
            if (exercise.TargetMuscles.Any())
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new Indentation { Left = "200" }),
                    new Run(new RunProperties(new Italic()), new Text($"🎯 {string.Join(", ", exercise.TargetMuscles)}"))
                ));
            }

            // Instructions (if enabled)
            if (exercise.StepByStepInstructions.Any() && template.Layout.ExerciseLayout.ShowInstructions)
            {
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(new Indentation { Left = "200" }),
                    new Run(new RunProperties(new Bold()), new Text("📋 Ejecución:"))
                ));

                for (int i = 0; i < Math.Min(exercise.StepByStepInstructions.Count, 3); i++)
                {
                    body.AppendChild(new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "400" }),
                        new Run(new Text($"{i + 1}. {exercise.StepByStepInstructions[i]}"))
                    ));
                }
            }

            body.AppendChild(CreateEmptyParagraph());
        }
    }

    public void FormatExercisesDetailed(Body body, List<DocumentExercise> exercises, DocumentTemplate template)
    {
        foreach (var exercise in exercises.OrderBy(e => e.OrderNumber))
        {
            // Exercise title
            var exerciseTitle = CreateStyledHeader($"{exercise.OrderNumber}. {exercise.ExerciseName}", template, 3);
            body.AppendChild(exerciseTitle);

            // Target muscles and equipment
            var infoLine = new Paragraph();
            if (exercise.TargetMuscles.Any())
            {
                infoLine.AppendChild(new Run(new RunProperties(new Bold()), new Text("Músculos: ")));
                infoLine.AppendChild(new Run(new Text($"{string.Join(", ", exercise.TargetMuscles)} | ")));
            }
            if (!string.IsNullOrEmpty(exercise.Equipment))
            {
                infoLine.AppendChild(new Run(new RunProperties(new Bold()), new Text("Equipo: ")));
                infoLine.AppendChild(new Run(new Text(exercise.Equipment)));
            }
            body.AppendChild(infoLine);

            // Parameters box
            CreateExerciseParametersBox(body, exercise.Parameters, template);

            // Step-by-step instructions
            if (exercise.StepByStepInstructions.Any() && template.Layout.ExerciseLayout.ShowInstructions)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Bold(), new FontSize { Val = "12" }), new Text("EJECUCIÓN:"))
                ));

                for (int i = 0; i < exercise.StepByStepInstructions.Count; i++)
                {
                    body.AppendChild(new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        new Run(new RunProperties(new Bold()), new Text($"{i + 1}. ")),
                        new Run(new Text(exercise.StepByStepInstructions[i]))
                    ));
                }
            }

            // Safety tips
            if (exercise.SafetyTips.Any() && template.Layout.ExerciseLayout.ShowSafetyTips)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Bold(), new Color { Val = "FF0000" }), new Text("⚠️ SEGURIDAD:"))
                ));
                foreach (var tip in exercise.SafetyTips)
                {
                    body.AppendChild(new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        new Run(new Text($"• {tip}"))
                    ));
                }
            }

            // Technique tips
            if (exercise.TechniqueTips.Any() && template.Layout.ExerciseLayout.ShowTechniqueTips)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Bold(),
                            new Color { Val = template.Style.ColorScheme.AccentColor.Replace("#", "") }),
                            new Text("💡 CONSEJOS:"))
                ));
                foreach (var tip in exercise.TechniqueTips)
                {
                    body.AppendChild(new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        new Run(new Text($"• {tip}"))
                    ));
                }
            }

            // Variations
            if (exercise.Variations.Any() && template.Layout.ExerciseLayout.ShowVariations)
            {
                body.AppendChild(new Paragraph(
                    new Run(new RunProperties(new Bold()), new Text("🔄 VARIACIONES:"))
                ));
                foreach (var variation in exercise.Variations)
                {
                    body.AppendChild(new Paragraph(
                        new ParagraphProperties(new Indentation { Left = "300" }),
                        new Run(new RunProperties(new Bold()), new Text($"• {variation.VariationName}: ")),
                        new Run(new Text(variation.Description))
                    ));
                }
            }

            body.AppendChild(CreateEmptyParagraph());
        }
    }

    public void FormatExercisesAsList(Body body, List<DocumentExercise> exercises, DocumentTemplate template)
    {
        foreach (var exercise in exercises.OrderBy(e => e.OrderNumber))
        {
            // Simple list format
            var exerciseParagraph = new Paragraph(
                new Run(new RunProperties(new Bold()), new Text($"{exercise.OrderNumber}. {exercise.ExerciseName} - ")),
                new Run(new Text($"{exercise.Parameters.Sets} × {exercise.Parameters.Reps}, {exercise.Parameters.RestPeriod}"))
            );

            if (exercise.TargetMuscles.Any())
            {
                exerciseParagraph.AppendChild(new Run(new Text($" ({string.Join(", ", exercise.TargetMuscles)})")));
            }

            body.AppendChild(exerciseParagraph);
        }
        body.AppendChild(CreateEmptyParagraph());
    }

    public void FormatProgressTrackingSection(Body body, DocumentTemplate template)
    {
        if (!template.Layout.IncludeProgressSection) return;

        body.AppendChild(CreateStyledHeader("SEGUIMIENTO DE PROGRESO", template, 1));

        // Progress tracking table
        var progressTable = new Table();

        var tableProps = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 6 },
                new BottomBorder { Val = BorderValues.Single, Size = 6 },
                new LeftBorder { Val = BorderValues.Single, Size = 6 },
                new RightBorder { Val = BorderValues.Single, Size = 6 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
            ),
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "100%" }
        );
        progressTable.AppendChild(tableProps);

        // Header
        var headerRow = new TableRow();
        var progressHeaders = new[] { "Semana", "Peso/Intensidad", "Repeticiones", "Observaciones", "Calificación (1-10)" };

        foreach (var header in progressHeaders)
        {
            var cell = new TableCell(new Paragraph(new Run(new RunProperties(new Bold()), new Text(header))));
            cell.AppendChild(new TableCellProperties(
                new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F0F0F0" }
            ));
            headerRow.AppendChild(cell);
        }
        progressTable.AppendChild(headerRow);

        // Empty rows for tracking
        for (int week = 1; week <= 8; week++)
        {
            var row = new TableRow();
            for (int col = 0; col < 5; col++)
            {
                var cell = new TableCell(new Paragraph(new Run(new Text(col == 0 ? $"Semana {week}" : ""))));
                if (col == 0)
                {
                    cell.AppendChild(new TableCellProperties(
                        new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F8F9FA" }
                    ));
                }
                row.AppendChild(cell);
            }
            progressTable.AppendChild(row);
        }

        body.AppendChild(progressTable);
        body.AppendChild(CreateEmptyParagraph());
    }

    // Helper methods
    private void CreateWorkoutInfoBox(Body body, DailyWorkout day, DocumentTemplate template)
    {
        // Info box with workout details
        var infoParagraph = new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(
                    new LeftBorder { Val = BorderValues.Single, Size = 12,
                                   Color = template.Style.ColorScheme.AccentColor.Replace("#", "") }
                ),
                new Indentation { Left = "200" },
                new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F8F9FA" }
            )
        );

        var infoText = $"⏱️ Duración: {day.EstimatedDuration.TotalMinutes} min";
        if (day.TargetIntensity.ToString() != "NotSpecified")
        {
            infoText += $" | 💪 Intensidad: {day.TargetIntensity}";
        }

        infoParagraph.AppendChild(new Run(new RunProperties(new Bold()), new Text(infoText)));
        body.AppendChild(infoParagraph);
        body.AppendChild(CreateEmptyParagraph());
    }

    private void CreateExerciseParametersBox(Body body, DocumentExerciseParameters parameters, DocumentTemplate template)
    {
        var paramBox = new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new Shading { Fill = template.Style.ColorScheme.HeaderColor?.Replace("#", "") ?? "F0F0F0" }
            )
        );

        var paramText = $"📊 {parameters.Sets} series × {parameters.Reps} repeticiones";
        if (!string.IsNullOrEmpty(parameters.Weight))
        {
            paramText += $" | ⚖️ {parameters.Weight}";
        }
        paramText += $" | ⏱️ Descanso: {parameters.RestPeriod}";

        if (!string.IsNullOrEmpty(parameters.Tempo))
        {
            paramText += $" | 🎵 Tempo: {parameters.Tempo}";
        }

        paramBox.AppendChild(new Run(new RunProperties(new Bold()), new Text(paramText)));
        body.AppendChild(paramBox);
    }

    private void FormatWarmupSection(Body body, WarmupSection warmup, DocumentTemplate template)
    {
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "14" }), new Text("🔥 CALENTAMIENTO"))
        ));

        foreach (var exercise in warmup.Exercises)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Indentation { Left = "200" }),
                new Run(new RunProperties(new Bold()), new Text($"• {exercise.Name}: ")),
                new Run(new Text($"{exercise.Duration} - {exercise.Instructions}"))
            ));
        }
        body.AppendChild(CreateEmptyParagraph());
    }

    private void FormatCooldownSection(Body body, CooldownSection cooldown, DocumentTemplate template)
    {
        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold(), new FontSize { Val = "14" }), new Text("❄️ ENFRIAMIENTO"))
        ));

        foreach (var exercise in cooldown.Exercises)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Indentation { Left = "200" }),
                new Run(new RunProperties(new Bold()), new Text($"• {exercise.Name}: ")),
                new Run(new Text($"{exercise.Duration} - {exercise.Instructions}"))
            ));
        }
        body.AppendChild(CreateEmptyParagraph());
    }

    private void FormatDayNotes(Body body, string notes, DocumentTemplate template)
    {
        body.AppendChild(new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(new LeftBorder {
                    Val = BorderValues.Single, Size = 8,
                    Color = template.Style.ColorScheme.AccentColor.Replace("#", "")
                }),
                new Indentation { Left = "300" }
            ),
            new Run(new RunProperties(new Bold()), new Text("📝 Notas del día: ")),
            new Run(new RunProperties(new Italic()), new Text(notes))
        ));
    }

    private void FormatProgressionGoals(Body body, ProgressionGoals goals, DocumentTemplate template)
    {
        if (!goals.WeeklyTargets.Any()) return;

        body.AppendChild(new Paragraph(
            new Run(new RunProperties(new Bold(),
                    new Color { Val = template.Style.ColorScheme.SecondaryColor.Replace("#", "") }),
                    new Text("🎯 OBJETIVOS DE PROGRESIÓN:"))
        ));

        foreach (var target in goals.WeeklyTargets)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Indentation { Left = "300" }),
                new Run(new Text($"• {target}"))
            ));
        }
    }

    private Paragraph CreateStyledHeader(string text, DocumentTemplate template, int level)
    {
        var fontSize = level switch
        {
            1 => "20",
            2 => "16",
            3 => "14",
            _ => "12"
        };

        var color = level switch
        {
            1 => template.Style.ColorScheme.PrimaryColor,
            2 => template.Style.ColorScheme.SecondaryColor,
            _ => template.Style.ColorScheme.AccentColor
        };

        return new Paragraph(
            new Run(new RunProperties(
                new Bold(),
                new FontSize { Val = fontSize },
                new Color { Val = color.Replace("#", "") }
            ), new Text(text))
        );
    }

    private Paragraph CreateEmptyParagraph()
    {
        return new Paragraph(new Run(new Text("")));
    }
}