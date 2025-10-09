using MediatR;
using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Application.Commands.Documents;

/// <summary>
/// Command to export a workout plan to PDF document
/// </summary>
public record ExportWorkoutPlanToPDFCommand : IRequest<Result<string>>
{
    public string UserName { get; init; } = string.Empty;
    public WorkoutPlan? WorkoutPlan { get; init; }
    public string OutputPath { get; init; } = string.Empty;
}

/// <summary>
/// Handler for exporting workout plans to PDF documents
/// </summary>
public class ExportWorkoutPlanToPDFCommandHandler
    : IRequestHandler<ExportWorkoutPlanToPDFCommand, Result<string>>
{
    private readonly IDocumentExportDomainService _exportService;

    public ExportWorkoutPlanToPDFCommandHandler(IDocumentExportDomainService exportService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    }

    public async Task<Result<string>> Handle(
        ExportWorkoutPlanToPDFCommand request,
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

            // Export to PDF using the domain service
            var success = await _exportService.ExportToPDFAsync(
                request.UserName,
                request.WorkoutPlan,
                request.OutputPath,
                cancellationToken
            );

            if (!success)
            {
                return Result.Failure<string>("Error al exportar la rutina a PDF. La funcionalidad de PDF está en desarrollo.");
            }

            return Result<string>.Success(request.OutputPath);
        }
        catch (NotImplementedException)
        {
            return Result.Failure<string>("La exportación a PDF aún no está completamente implementada. Por favor, use la exportación a Word.");
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
            return Result.Failure<string>($"Error al exportar a PDF: {ex.Message}");
        }
    }
}
