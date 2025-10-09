using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities
{
    public class ExerciseSearchHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        public DateTime SearchedAt { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(500)]
        public string SearchQuery { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SearchType { get; set; } = string.Empty; // "Text", "Image", "AI"

        public int ResultCount { get; set; }

        public bool WasSuccessful { get; set; }

        [MaxLength(1000)]
        public string AIResponse { get; set; } = string.Empty;
    }
}