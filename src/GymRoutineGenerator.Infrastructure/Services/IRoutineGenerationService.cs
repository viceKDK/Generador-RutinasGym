using GymRoutineGenerator.Core.Models;
using Entities = GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Infrastructure.Services
{
    /// <summary>
    /// Servicio para generación inteligente de rutinas de gimnasio
    /// Combina generación con IA y métodos clásicos
    /// </summary>
    public interface IRoutineGenerationService
    {
        /// <summary>
        /// Genera una rutina personalizada usando IA si está disponible, o método clásico como fallback
        /// </summary>
        Task<RoutineGenerationResult> GeneratePersonalizedRoutineAsync(UserRoutineParameters parameters);

        /// <summary>
        /// Genera una rutina personalizada con estructura, retornando texto y workout days
        /// </summary>
        Task<(string text, List<WorkoutDay>? workouts)> GeneratePersonalizedRoutineWithStructureAsync(Entities.UserProfile profile);

        /// <summary>
        /// Genera una rutina alternativa diferente para el mismo perfil
        /// </summary>
        Task<RoutineGenerationResult> GenerateAlternativeRoutineAsync(UserRoutineParameters parameters);

        /// <summary>
        /// Genera una rutina alternativa simple (retorna solo texto)
        /// </summary>
        Task<string> GenerateAlternativeRoutineAsync(Entities.UserProfile profile);

        /// <summary>
        /// Verifica si el servicio de IA está disponible
        /// </summary>
        Task<bool> IsAIAvailableAsync();

        /// <summary>
        /// Obtiene el estado del servicio de IA
        /// </summary>
        Task<string> GetAIStatusAsync();
    }

    /// <summary>
    /// Resultado de la generación de rutina
    /// </summary>
    public class RoutineGenerationResult
    {
        public bool Success { get; set; }
        public string RoutineText { get; set; } = string.Empty;
        public List<WorkoutDay> WorkoutDays { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public bool GeneratedWithAI { get; set; }
        public TimeSpan GenerationTime { get; set; }
    }
}
