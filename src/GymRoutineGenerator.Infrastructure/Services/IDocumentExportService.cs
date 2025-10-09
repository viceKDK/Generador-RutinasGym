using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Infrastructure.Services
{
    /// <summary>
    /// Servicio para exportación de rutinas a documentos Word/HTML/PDF
    /// </summary>
    public interface IDocumentExportService
    {
        /// <summary>
        /// Exporta rutina a formato Word (HTML compatible con Word)
        /// </summary>
        Task<bool> ExportToWordAsync(string filePath, string routineContent, string clientName);

        /// <summary>
        /// Exporta rutina a Word con imágenes de la base de datos
        /// </summary>
        Task<bool> ExportToWordWithImagesAsync(string filePath, List<WorkoutDay> workoutDays, UserRoutineParameters profile);

        /// <summary>
        /// Exporta rutina a HTML optimizado para PDF
        /// </summary>
        Task<bool> ExportToPDFAsync(string filePath, string routineContent, string clientName);

        /// <summary>
        /// Exporta con mejoras de IA (explicaciones, progresiones, etc.)
        /// </summary>
        Task<bool> ExportToWordWithAIAsync(string filePath, List<WorkoutDay> workoutDays, UserRoutineParameters profile, bool includeAIExplanations = true);

        /// <summary>
        /// Verifica si las mejoras de IA están disponibles
        /// </summary>
        bool IsAIEnabled { get; }
    }
}
