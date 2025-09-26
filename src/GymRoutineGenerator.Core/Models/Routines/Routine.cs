using System;
using System.Collections.Generic;

namespace GymRoutineGenerator.Core.Models.Routines;

public class Routine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public int DurationWeeks { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public List<RoutineDay> Days { get; set; } = new();
    public RoutineMetrics Metrics { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class RoutineDay
{
    public int Id { get; set; }
    public int DayNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<RoutineExercise> Exercises { get; set; } = new();
    public string FocusArea { get; set; } = string.Empty;
    public string TargetIntensity { get; set; } = "Moderate";
    public int EstimatedDurationMinutes { get; set; }
    public string RestDay { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class RoutineExercise
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public string Equipment { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string SafetyTips { get; set; } = string.Empty;
    public List<ExerciseSet> Sets { get; set; } = new();
    public int RestTimeSeconds { get; set; } = 60;
    public string Difficulty { get; set; } = "Intermediate";
    public List<string> Variations { get; set; } = new();
    public string ImagePath { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class ExerciseSet
{
    public int Id { get; set; }
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "kg";
    public int RestSeconds { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Type { get; set; } = "Normal"; // Normal, Warmup, Dropset, Superset
}

public class RoutineMetrics
{
    public int TotalExercises { get; set; }
    public int TotalSets { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public List<string> MuscleGroupsCovered { get; set; } = new();
    public List<string> EquipmentRequired { get; set; } = new();
    public string DifficultyLevel { get; set; } = "Intermediate";
    public int CaloriesBurnedEstimate { get; set; }
}