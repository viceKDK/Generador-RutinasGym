using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymRoutineGenerator.Data.Entities
{
    public class RoutineExercise
    {
        [Key]
        public int Id { get; set; }

        public int UserRoutineId { get; set; }
        public virtual UserRoutine UserRoutine { get; set; } = null!;

        public int? ExerciseId { get; set; } // Null si es ejercicio personalizado
        public virtual Exercise? Exercise { get; set; }

        [Required]
        [MaxLength(200)]
        public string ExerciseName { get; set; } = string.Empty;

        public int DayNumber { get; set; } // 1, 2, 3, etc.

        [MaxLength(100)]
        public string DayName { get; set; } = string.Empty; // "Pecho + Tríceps"

        public int OrderInDay { get; set; } // Orden dentro del día

        [Required]
        [MaxLength(50)]
        public string SetsAndReps { get; set; } = string.Empty; // "3x10", "4x8-12"

        [MaxLength(1000)]
        public string Instructions { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ImageInfo { get; set; } = string.Empty;

        public bool IsCustomExercise { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}