using System;
using System.Collections.Generic;

namespace GymRoutineGenerator.Domain;

/// <summary>
/// Represents a single training day with its exercises.
/// </summary>
public class WorkoutDay
{
    public string Name { get; set; } = string.Empty;
    public List<Exercise> Exercises { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public int DayNumber { get; set; }
    public string FocusAreas { get; set; } = string.Empty; // e.g., "Pecho, Triceps"
    public string[] MuscleGroups { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Parameters used to describe a customer's training profile.
/// </summary>
public class UserRoutineParameters
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int TrainingDaysPerWeek { get; set; }
    public string ExperienceLevel { get; set; } = string.Empty;
    public string PrimaryGoal { get; set; } = string.Empty;
    public List<string> Goals { get; set; } = new();
    public List<string> AvailableEquipment { get; set; } = new();
    public string GymType { get; set; } = string.Empty;
    public List<string> PhysicalLimitations { get; set; } = new();
    public List<string> AvoidExercises { get; set; } = new();
    public int RecommendedIntensity { get; set; }
    public int PreferredSessionDuration { get; set; }
    public bool IncludeCardio { get; set; }
    public bool IncludeFlexibility { get; set; }
    public List<string> PreferredExerciseTypes { get; set; } = new();
    public List<MuscleGroupFocus> MuscleGroupPreferences { get; set; } = new();
    public string FitnessLevel { get; set; } = string.Empty;
    public int TrainingDays { get; set; }
}

/// <summary>
/// Specifies how much focus a routine should place on a muscle group.
/// </summary>
public class MuscleGroupFocus
{
    public string MuscleGroup { get; set; } = string.Empty;
    public string EmphasisLevel { get; set; } = string.Empty; // High, Medium, Low
    public int Priority { get; set; }
}
