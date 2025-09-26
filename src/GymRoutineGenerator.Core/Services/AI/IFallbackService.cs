using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;

namespace GymRoutineGenerator.Core.Services.AI;

public interface IFallbackService
{
    /// <summary>
    /// Verifica si el servicio de IA está disponible
    /// </summary>
    Task<bool> IsAIServiceAvailableAsync();

    /// <summary>
    /// Genera una rutina usando plantillas predefinidas cuando la IA no está disponible
    /// </summary>
    Task<Routine> GenerateBasicRoutineAsync(string clientName, string goal, int durationWeeks);

    /// <summary>
    /// Obtiene recomendaciones básicas sin IA
    /// </summary>
    Task<List<string>> GetBasicRecommendationsAsync(string goal);

    /// <summary>
    /// Modifica rutina existente de forma básica
    /// </summary>
    Task<Routine> ModifyRoutineBasicAsync(Routine routine, string modification);
}

public enum FallbackMode
{
    AIAvailable,
    AIUnavailable_BasicMode,
    AIUnavailable_TemplateOnly
}