using MediatR;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Application.Commands.Documents;

/// <summary>
/// Command to export a workout plan to Word document
/// </summary>
public record ExportWorkoutPlanToWordCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public WorkoutPlan? WorkoutPlan { get; init; }
    public string OutputPath { get; init; } = string.Empty;
}

/// <summary>
/// Handler for exporting workout plans to Word documents
/// </summary>
public class ExportWorkoutPlanToWordCommandHandler
    : IRequestHandler<ExportWorkoutPlanToWordCommand, Result<string>>
{
    private readonly IDocumentExportDomainService _exportService;

    public ExportWorkoutPlanToWordCommandHandler(IDocumentExportDomainService exportService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    }

    public async Task<Result<string>> Handle(
        ExportWorkoutPlanToWordCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return Result.Failure<string>("El nombre de usuario es requerido.");
            }

            if (request.WorkoutPlan == null)
            {
                return Result.Failure<string>("No hay plan de entrenamiento para exportar.");
            }

            if (string.IsNullOrWhiteSpace(request.OutputPath))
            {
                return Result.Failure<string>("La ruta de salida es requerida.");
            }

            // Export to Word using the domain service
            var success = await _exportService.ExportToWordAsync(
                request.UserName,
                request.WorkoutPlan,
                request.OutputPath,
                cancellationToken
            );

            if (!success)
            {
                return Result.Failure<string>("Error al exportar la rutina a Word. Por favor, verifique que no haya un archivo abierto con el mismo nombre.");
            }

            return Result<string>.Success(request.OutputPath);
        }
        catch (IOException ioEx)
        {
            return Result.Failure<string>($"Error de archivo: {ioEx.Message}. El archivo puede estar abierto en otra aplicación.");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            return Result.Failure<string>($"Error de permisos: {uaEx.Message}. Verifique que tiene permisos de escritura en el directorio.");
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Error al exportar a Word: {ex.Message}");
        }
    }
}
