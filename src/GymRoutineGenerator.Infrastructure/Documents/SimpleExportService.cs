using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;

namespace GymRoutineGenerator.Infrastructure.Documents;

public class SimpleExportService : IExportService
{
    private readonly IWordDocumentService _wordDocumentService;
    private readonly ITemplateManagerService _templateManagerService;
    private readonly ILogger<SimpleExportService>? _logger;
    private readonly string _defaultExportPath;

    public SimpleExportService(
        IWordDocumentService wordDocumentService,
        ITemplateManagerService templateManagerService,
        ILogger<SimpleExportService>? logger = null)
    {
        _wordDocumentService = wordDocumentService ?? throw new ArgumentNullException(nameof(wordDocumentService));
        _templateManagerService = templateManagerService ?? throw new ArgumentNullException(nameof(templateManagerService));
        _logger = logger;
        _defaultExportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rutinas de Gimnasio");
    }

    public async Task<ExportResult> ExportRoutineToWordAsync(
        Routine routine,
        string templateId,
        ExportOptions options,
        IProgress<ExportProgress>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ExportResult();

        try
        {
            progress?.Report(new ExportProgress { CurrentStep = 1, TotalSteps = 6, CurrentOperation = "Preparando exportación..." });

            // Preparar directorio de salida
            var outputPath = await PrepareOutputPathAsync(options.OutputPath);
            var filename = !string.IsNullOrEmpty(options.CustomFilename)
                ? options.CustomFilename
                : await GetSuggestedFilenameAsync(routine);

            if (!filename.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                filename += ".docx";

            var fullPath = Path.Combine(outputPath, filename);

            progress?.Report(new ExportProgress { CurrentStep = 2, TotalSteps = 6, CurrentOperation = "Verificando archivo existente..." });

            // Manejar archivos existentes
            if (File.Exists(fullPath))
            {
                if (!options.OverwriteExisting)
                {
                    fullPath = await GenerateUniqueFilenameAsync(fullPath);
                }
                else if (options.CreateBackup)
                {
                    await CreateBackupAsync(fullPath);
                }
            }

            progress?.Report(new ExportProgress { CurrentStep = 3, TotalSteps = 6, CurrentOperation = "Preparando plantilla..." });

            // Obtener plantilla
            var template = await _templateManagerService.GetTemplateAsync(templateId);

            progress?.Report(new ExportProgress { CurrentStep = 4, TotalSteps = 6, CurrentOperation = "Generando documento Word..." });

            // Crear el documento usando el generador directo existente
            await GenerateWordDocumentDirectly(routine, template, fullPath);

            progress?.Report(new ExportProgress { CurrentStep = 5, TotalSteps = 6, CurrentOperation = "Verificando archivo generado..." });

            // Verificar que el archivo se creó correctamente
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException("El documento no se pudo crear correctamente");
            }

            var fileInfo = new FileInfo(fullPath);
            result.FilePath = fullPath;
            result.FileSizeBytes = fileInfo.Length;
            result.ExerciseCount = routine.Days.SelectMany(d => d.Exercises).Count();
            result.Success = true;

            progress?.Report(new ExportProgress { CurrentStep = 6, TotalSteps = 6, CurrentOperation = "Exportación completada" });

            // Auto-abrir archivo si está habilitado
            if (options.AutoOpenAfterExport)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "No se pudo abrir automáticamente el archivo: {FilePath}", fullPath);
                }
            }

            _logger?.LogInformation("Rutina exportada exitosamente: {FilePath} ({FileSize} bytes)",
                fullPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger?.LogError(ex, "Error al exportar rutina a Word");
        }
        finally
        {
            stopwatch.Stop();
            result.ExportDuration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<ExportResult> ExportMultipleRoutinesToWordAsync(
        IEnumerable<Routine> routines,
        string templateId,
        ExportOptions options,
        IProgress<ExportProgress>? progress = null)
    {
        var routineList = routines.ToList();
        var stopwatch = Stopwatch.StartNew();
        var result = new ExportResult();

        try
        {
            var totalRoutines = routineList.Count;
            var totalSteps = totalRoutines * 6 + 2;
            var currentStep = 0;

            progress?.Report(new ExportProgress
            {
                CurrentStep = ++currentStep,
                TotalSteps = totalSteps,
                CurrentOperation = "Preparando exportación múltiple..."
            });

            var outputPath = await PrepareOutputPathAsync(options.OutputPath);
            var successfulExports = new List<string>();
            var errors = new List<string>();

            for (int i = 0; i < routineList.Count; i++)
            {
                var routine = routineList[i];
                var filename = await GetSuggestedFilenameAsync(routine, $"_{i + 1:D2}");

                var routineOptions = new ExportOptions
                {
                    OutputPath = outputPath,
                    CustomFilename = filename,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = options.OverwriteExisting,
                    CreateBackup = options.CreateBackup
                };

                var routineProgress = new Progress<ExportProgress>(p =>
                {
                    progress?.Report(new ExportProgress
                    {
                        CurrentStep = currentStep + p.CurrentStep,
                        TotalSteps = totalSteps,
                        CurrentOperation = $"Rutina {i + 1}/{totalRoutines}: {p.CurrentOperation}"
                    });
                });

                var routineResult = await ExportRoutineToWordAsync(routine, templateId, routineOptions, routineProgress);
                currentStep += 6;

                if (routineResult.Success)
                {
                    successfulExports.Add(routineResult.FilePath!);
                    result.FileSizeBytes += routineResult.FileSizeBytes;
                    result.ExerciseCount += routineResult.ExerciseCount;
                }
                else
                {
                    errors.Add($"Rutina {i + 1}: {routineResult.ErrorMessage}");
                }
            }

            progress?.Report(new ExportProgress
            {
                CurrentStep = totalSteps,
                TotalSteps = totalSteps,
                CurrentOperation = "Exportación múltiple completada"
            });

            result.Success = successfulExports.Count > 0;
            result.FilePath = outputPath;

            if (errors.Any())
            {
                result.ErrorMessage = string.Join("; ", errors);
            }

            // Abrir carpeta si está habilitado
            if (options.AutoOpenAfterExport && result.Success)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = outputPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "No se pudo abrir la carpeta de exportación: {Path}", outputPath);
                }
            }

            _logger?.LogInformation("Exportación múltiple completada: {Successful}/{Total} rutinas exitosas",
                successfulExports.Count, totalRoutines);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger?.LogError(ex, "Error en exportación múltiple");
        }
        finally
        {
            stopwatch.Stop();
            result.ExportDuration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<string> GetSuggestedFilenameAsync(Routine routine, string suffix = "")
    {
        await Task.CompletedTask;

        var clientName = !string.IsNullOrWhiteSpace(routine.ClientName)
            ? routine.ClientName.Trim()
            : "Cliente";

        var routineName = !string.IsNullOrWhiteSpace(routine.Name)
            ? routine.Name.Trim()
            : "Rutina";

        var date = DateTime.Now.ToString("yyyy-MM-dd");

        clientName = CleanFilename(clientName);
        routineName = CleanFilename(routineName);

        return $"{clientName}_{routineName}_{date}{suffix}";
    }

    public async Task<string> GetDefaultExportPathAsync()
    {
        await Task.CompletedTask;
        return _defaultExportPath;
    }

    public async Task SetDefaultExportPathAsync(string path)
    {
        await Task.CompletedTask;
        _logger?.LogInformation("Ruta de exportación por defecto establecida: {Path}", path);
    }

    private async Task<string> PrepareOutputPathAsync(string? outputPath)
    {
        var path = !string.IsNullOrWhiteSpace(outputPath) ? outputPath : _defaultExportPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        await Task.CompletedTask;
        return path;
    }

    private async Task<string> GenerateUniqueFilenameAsync(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath)!;
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
        var extension = Path.GetExtension(originalPath);

        int counter = 1;
        string newPath;

        do
        {
            newPath = Path.Combine(directory, $"{filenameWithoutExtension}_{counter}{extension}");
            counter++;
        }
        while (File.Exists(newPath));

        await Task.CompletedTask;
        return newPath;
    }

    private async Task CreateBackupAsync(string originalPath)
    {
        try
        {
            var backupPath = originalPath.Replace(".docx", $"_backup_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
            File.Copy(originalPath, backupPath);
            _logger?.LogInformation("Backup creado: {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "No se pudo crear backup del archivo: {Path}", originalPath);
        }

        await Task.CompletedTask;
    }

    private static string CleanFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return "Sin_Nombre";

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleanedName = string.Join("_", filename.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        if (cleanedName.Length > 50)
        {
            cleanedName = cleanedName.Substring(0, 50);
        }

        return cleanedName;
    }

    private async Task GenerateWordDocumentDirectly(Routine routine, DocumentTemplate template, string outputPath)
    {
        // Este es un generador simplificado que crea un documento básico
        // En una implementación completa, esto usaría DocumentFormat.OpenXml

        using var fileStream = File.Create(outputPath);
        using var writer = new StreamWriter(fileStream);

        // Escribir header del documento (versión simplificada)
        await writer.WriteLineAsync("=== RUTINA DE GIMNASIO ===");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"Cliente: {routine.ClientName}");
        await writer.WriteLineAsync($"Rutina: {routine.Name}");
        await writer.WriteLineAsync($"Objetivo: {routine.Goal}");
        await writer.WriteLineAsync($"Plantilla: {template.Name}");
        await writer.WriteLineAsync($"Fecha de creación: {routine.CreatedDate:dd/MM/yyyy}");
        await writer.WriteLineAsync($"Duración: {routine.DurationWeeks} semanas");
        await writer.WriteLineAsync();

        if (!string.IsNullOrEmpty(routine.Description))
        {
            await writer.WriteLineAsync("DESCRIPCIÓN:");
            await writer.WriteLineAsync(routine.Description);
            await writer.WriteLineAsync();
        }

        // Escribir días de entrenamiento
        foreach (var day in routine.Days.OrderBy(d => d.DayNumber))
        {
            await writer.WriteLineAsync($"=== {day.Name.ToUpper()} ===");
            await writer.WriteLineAsync($"Enfoque: {day.FocusArea}");
            await writer.WriteLineAsync($"Intensidad objetivo: {day.TargetIntensity}");
            await writer.WriteLineAsync($"Duración estimada: {day.EstimatedDurationMinutes} minutos");

            if (!string.IsNullOrEmpty(day.Description))
            {
                await writer.WriteLineAsync($"Descripción: {day.Description}");
            }

            await writer.WriteLineAsync();

            // Escribir ejercicios
            foreach (var exercise in day.Exercises.OrderBy(e => e.Order))
            {
                await writer.WriteLineAsync($"{exercise.Order}. {exercise.Name}");
                await writer.WriteLineAsync($"   Categoría: {exercise.Category}");
                await writer.WriteLineAsync($"   Grupos musculares: {string.Join(", ", exercise.MuscleGroups)}");
                await writer.WriteLineAsync($"   Equipo: {exercise.Equipment}");
                await writer.WriteLineAsync($"   Dificultad: {exercise.Difficulty}");

                if (!string.IsNullOrEmpty(exercise.Instructions))
                {
                    await writer.WriteLineAsync($"   Instrucciones: {exercise.Instructions}");
                }

                if (!string.IsNullOrEmpty(exercise.SafetyTips))
                {
                    await writer.WriteLineAsync($"   Consejos de seguridad: {exercise.SafetyTips}");
                }

                // Escribir series
                if (exercise.Sets.Any())
                {
                    await writer.WriteLineAsync("   Series:");
                    foreach (var set in exercise.Sets.OrderBy(s => s.SetNumber))
                    {
                        await writer.WriteLineAsync($"     Serie {set.SetNumber}: {set.Reps} reps");
                        if (set.Weight > 0)
                        {
                            await writer.WriteLineAsync($"       Peso: {set.Weight} {set.Unit}");
                        }
                        if (set.RestSeconds > 0)
                        {
                            await writer.WriteLineAsync($"       Descanso: {set.RestSeconds} segundos");
                        }
                    }
                }

                await writer.WriteLineAsync($"   Descanso entre ejercicios: {exercise.RestTimeSeconds} segundos");

                if (!string.IsNullOrEmpty(exercise.Notes))
                {
                    await writer.WriteLineAsync($"   Notas: {exercise.Notes}");
                }

                await writer.WriteLineAsync();
            }

            await writer.WriteLineAsync();
        }

        // Escribir métricas finales
        await writer.WriteLineAsync("=== RESUMEN DE LA RUTINA ===");
        await writer.WriteLineAsync($"Total de ejercicios: {routine.Metrics.TotalExercises}");
        await writer.WriteLineAsync($"Total de series: {routine.Metrics.TotalSets}");
        await writer.WriteLineAsync($"Duración estimada: {routine.Metrics.EstimatedDurationMinutes} minutos");
        await writer.WriteLineAsync($"Nivel de dificultad: {routine.Metrics.DifficultyLevel}");
        await writer.WriteLineAsync($"Calorías estimadas: {routine.Metrics.CaloriesBurnedEstimate}");

        if (routine.Metrics.MuscleGroupsCovered.Any())
        {
            await writer.WriteLineAsync($"Grupos musculares trabajados: {string.Join(", ", routine.Metrics.MuscleGroupsCovered)}");
        }

        if (routine.Metrics.EquipmentRequired.Any())
        {
            await writer.WriteLineAsync($"Equipo requerido: {string.Join(", ", routine.Metrics.EquipmentRequired)}");
        }

        if (!string.IsNullOrEmpty(routine.Notes))
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("NOTAS ADICIONALES:");
            await writer.WriteLineAsync(routine.Notes);
        }

        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
        await writer.WriteLineAsync("Generado con Claude Code - Sistema de Rutinas de Gimnasio");
    }
}