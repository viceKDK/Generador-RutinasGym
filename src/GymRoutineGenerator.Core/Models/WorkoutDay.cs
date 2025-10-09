namespace GymRoutineGenerator.Core.Models
{
    /// <summary>
    /// Representa un día de entrenamiento con sus ejercicios
    /// </summary>
    public class WorkoutDay
    {
        public string Name { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public int DayNumber { get; set; }
        public string FocusAreas { get; set; } = string.Empty; // e.g., "Pecho, Tríceps"
    }

    /// <summary>
    /// Parámetros para generación de rutinas personalizadas
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
        public string GymType { get; set; } = string.Empty; // "Casa", "Gimnasio Comercial", etc.
        public List<string> PhysicalLimitations { get; set; } = new();
        public List<string> AvoidExercises { get; set; } = new(); // Ejercicios a evitar
        public int RecommendedIntensity { get; set; } // 1-5
        public int PreferredSessionDuration { get; set; } // minutos
        public bool IncludeCardio { get; set; }
        public bool IncludeFlexibility { get; set; }
        public List<string> PreferredExerciseTypes { get; set; } = new();
        public List<MuscleGroupFocus> MuscleGroupPreferences { get; set; } = new();
        public string FitnessLevel { get; set; } = string.Empty; // Para compatibilidad con UI
        public int TrainingDays { get; set; } // Alias para TrainingDaysPerWeek
    }

    /// <summary>
    /// Enfoque en un grupo muscular específico
    /// </summary>
    public class MuscleGroupFocus
    {
        public string MuscleGroup { get; set; } = string.Empty;
        public string EmphasisLevel { get; set; } = string.Empty; // "Alto", "Medio", "Bajo"
        public int Priority { get; set; } // 1-5
    }
}
