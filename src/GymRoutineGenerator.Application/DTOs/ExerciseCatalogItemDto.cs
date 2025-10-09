using System.Collections.Generic;

namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// DTO resumido para el gestor de ejercicios en la UI.
/// </summary>
public class ExerciseCatalogItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Equipment { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public bool HasImage { get; set; }
    public List<string> TargetMuscles { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public bool IsActive { get; set; }
}
